using System.Text;

namespace PdfDocument;

/// <summary>
/// Main class for building PDF documents.
/// Manages pages, images, fonts and the structure of the final PDF file.
/// </summary>
public sealed class PdfBuilder : IDisposable
{
    private readonly List<PdfPage> _pages = [];
    private int _nextId = 1;
    private readonly Dictionary<int, long> _offsets = [];
    private readonly Dictionary<int, string> _objects = [];
    private int _fontId;
    private int _pagesId;
    private int _catalogId;

    private readonly Dictionary<string, ImageInfo> _images = new(StringComparer.Ordinal);

    /// <summary>
    /// Adds a new page to the document.
    /// </summary>
    /// <param name="width">Page width in points (default: 612).</param>
    /// <param name="height">Page height in points (default: 792).</param>
    /// <returns>The newly created page.</returns>
    public PdfPage AddPage(double width = PdfConstants.DefaultPageWidth,
                           double height = PdfConstants.DefaultPageHeight)
    {
        var page = new PdfPage(width, height);
        _pages.Add(page);
        return page;
    }

    /// <summary>
    /// Registers a JPEG image for use on document pages.
    /// </summary>
    /// <param name="name">Unique name for reference when drawing.</param>
    /// <param name="data">JPEG image bytes.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">If the name already exists or size exceeds 512 KB.</exception>
    /// <exception cref="InvalidDataException">If the JPEG SOF0 marker is not found.</exception>
    public void AddImage(string name, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(data);

        // Prevent PDF injection through image name (CWE-79)
        if (!PdfConstants.IsValidPdfName(name))
            throw new ArgumentException("Image name must be alphanumeric (dashes and underscores allowed).", nameof(name));

        if (_images.ContainsKey(name))
            throw new ArgumentException($"Image '{name}' already exists.", nameof(name));

        if (data.Length > PdfConstants.MaxImageSizeBytes)
            throw new ArgumentException(
                $"Image '{name}' exceeds {PdfConstants.MaxImageSizeBytes / 1024} KB.",
                nameof(data));

        var (width, height) = GetJpegDimensions(data);
        _images[name] = new ImageInfo(data, width, height);
    }

    /// <summary>
    /// Registers a JPEG image from a file.
    /// </summary>
    public void AddImage(string name, string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        AddImage(name, File.ReadAllBytes(filePath));
    }

    /// <summary>
    /// Saves the PDF document to the specified path.
    /// </summary>
    /// <param name="path">Output file path.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="path"/> is null.</exception>
    public void Save(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        using var fs = new FileStream(path, FileMode.Create);
        using var writer = new StreamWriter(fs, Encoding.ASCII, leaveOpen: true);

        writer.WriteLine(PdfConstants.PdfHeader);

        // ── Font ────────────────────────────────────────────────────────
        _fontId = AllocateId();
        WriteIndirectObject(writer, fs, _fontId, PdfConstants.DefaultFontDefinition);

        _pagesId = AllocateId();

        // ── Images ──────────────────────────────────────────────────────
        var imageObjIds = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var kv in _images)
        {
            string name = kv.Key;
            var info = kv.Value;
            int objId = AllocateId();

            imageObjIds[name] = objId;
            _offsets[objId] = fs.Position;
            WriteImageObject(fs, objId, info);
        }

        // ── Pages ───────────────────────────────────────────────────────
        List<int> pageIds = new(_pages.Count);
        foreach (var page in _pages)
        {
            string content = page.Canvas.GetContent();
            int contentId = AllocateId();

            string contentObj = $"<< /Length {content.Length} >>\nstream\n{content}\nendstream";
            WriteIndirectObject(writer, fs, contentId, contentObj);

            string resources = $"/Font << /F1 {_fontId} 0 R >>";

            if (page.Canvas.UsedImages.Count > 0)
            {
                resources += " /XObject << ";
                foreach (var imgName in page.Canvas.UsedImages)
                {
                    if (imageObjIds.TryGetValue(imgName, out int objId))
                        resources += $"/{imgName} {objId} 0 R ";
                }
                resources += ">>";
            }

            int pageId = AllocateId();
            string pageDict = $"<< /Type /Page /Parent {_pagesId} 0 R " +
                              $"/Contents {contentId} 0 R " +
                              $"/MediaBox [0 0 {page.Width} {page.Height}] " +
                              $"/Resources << {resources} >> >>";
            WriteIndirectObject(writer, fs, pageId, pageDict);
            pageIds.Add(pageId);
        }

        // ── Pages root ──────────────────────────────────────────────────
        string kids = string.Join(" ", pageIds.ConvertAll(id => $"{id} 0 R"));
        string pagesDict = $"<< /Type /Pages /Kids [{kids}] /Count {pageIds.Count} >>";
        WriteIndirectObject(writer, fs, _pagesId, pagesDict);

        // ── Catalog ─────────────────────────────────────────────────────
        _catalogId = AllocateId();
        string catalogDict = $"<< /Type /Catalog /Pages {_pagesId} 0 R >>";
        WriteIndirectObject(writer, fs, _catalogId, catalogDict);

        // ── Cross-reference table ───────────────────────────────────────
        long startXref = fs.Position;
        writer.WriteLine(PdfConstants.XrefMarker);
        writer.WriteLine($"0 {_nextId}");
        writer.WriteLine("0000000000 65535 f ");
        for (int i = 1; i < _nextId; i++)
            writer.WriteLine($"{_offsets[i]:D10} 00000 n ");

        writer.WriteLine(PdfConstants.TrailerMarker);
        writer.WriteLine($"<< /Size {_nextId} /Root {_catalogId} 0 R >>");
        writer.WriteLine(PdfConstants.StartXrefMarker);
        writer.WriteLine(startXref);
        writer.WriteLine(PdfConstants.EofMarker);

        writer.Flush();
    }

    /// <summary>No unmanaged resources to release. Kept for compatibility.</summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // ── Private methods ───────────────────────────────────────────────

    /// <summary>
    /// Extracts JPEG image dimensions from raw bytes,
    /// locating the SOF0 (Start Of Frame 0) marker.
    /// </summary>
    private static (int width, int height) GetJpegDimensions(byte[] data)
    {
        // CWE-125: ensure room for marker (2) + length (2) + SOF0 data (8)
        const int MinSof0Size = 2 + 2 + 8;
        if (data.Length < MinSof0Size)
            throw new InvalidDataException("JPEG data is too small to contain SOF0 marker.");

        for (int index = 0; index < data.Length - 1; index++)
        {
            if (data[index] != PdfConstants.JpegMarkerPrefix)
                continue;

            if (data[index + 1] == PdfConstants.JpegSof0Marker)
            {
                int sofOffset = index + 5;

                // CWE-125: verify we have enough bytes remaining for height (2) + width (2)
                if (sofOffset + 3 >= data.Length)
                    throw new InvalidDataException("JPEG SOF0 marker found but data is truncated.");

                int imgHeight = (data[sofOffset] << 8) | data[sofOffset + 1];
                int imgWidth = (data[sofOffset + 2] << 8) | data[sofOffset + 3];
                return (imgWidth, imgHeight);
            }

            index++; // Skip marker byte; loop increment handles the rest
        }
        throw new InvalidDataException("SOF0 marker not found. The image may not be valid JPEG.");
    }

    /// <summary>Allocates the next PDF object ID.</summary>
    private int AllocateId() => _nextId++;

    /// <summary>
    /// Writes an indirect PDF object: records its offset, stores it, and writes to stream.
    /// Reduces repetition of the _objects/_offsets/WriteObject pattern.
    /// </summary>
    private void WriteIndirectObject(StreamWriter writer, FileStream fs, int id, string content)
    {
        _objects[id] = content;
        _offsets[id] = fs.Position;
        WriteObject(writer, id, content);
    }

    /// <summary>Writes a PDF object in "id 0 obj ... endobj" format.</summary>
    private static void WriteObject(StreamWriter writer, int id, string content)
    {
        writer.WriteLine($"{id} 0 obj");
        writer.WriteLine(content);
        writer.WriteLine("endobj");
    }

    /// <summary>
    /// Writes an image object to the PDF (binary stream with header).
    /// </summary>
    private static void WriteImageObject(FileStream fs, int objId, ImageInfo info)
    {
        string header = $"{objId} 0 obj\n" +
                        $"<< /Type /XObject /Subtype /Image " +
                        $"/Width {info.Width} /Height {info.Height} " +
                        $"/ColorSpace /DeviceRGB /BitsPerComponent 8 " +
                        $"/Filter /DCTDecode /Length {info.Data.Length} >>\nstream\n";

        byte[] headerBytes = Encoding.ASCII.GetBytes(header);
        fs.Write(headerBytes, 0, headerBytes.Length);
        fs.Write(info.Data, 0, info.Data.Length);
        byte[] footerBytes = Encoding.ASCII.GetBytes("\nendstream\nendobj\n");
        fs.Write(footerBytes, 0, footerBytes.Length);
    }

    /// <summary>
    /// Registered image information.
    /// </summary>
    private sealed record ImageInfo(byte[] Data, int Width, int Height);
}

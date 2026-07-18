using System.Buffers;
using System.Text;

namespace PdfDocument;

/// <summary>
/// PDF drawing canvas. Accumulates low-level PDF operator commands
/// to form the content stream of a page.
/// Not thread-safe — all drawing commands must be issued from a single thread.
/// </summary>
public class PdfCanvas
{
    // Pre-allocate a reasonable capacity to avoid frequent resizing
    private readonly StringBuilder _cmds = new(4096);
    private readonly HashSet<string> _usedImages = [];

    /// <summary>Set of image names referenced in this canvas.</summary>
    public IReadOnlySet<string> UsedImages => _usedImages;

    /// <summary>
    /// Horizontal text alignment.
    /// </summary>
    public enum TextAlign
    {
        /// <summary>Left-aligned.</summary>
        Left,
        /// <summary>Centered.</summary>
        Center,
        /// <summary>Right-aligned.</summary>
        Right
    }

    // Lazily initialize WinAnsiEncoding — registers CodePagesEncodingProvider
    // on first access. This avoids the try-catch + NotSupportedException penalty
    // that was observed in the dotnet-dump analysis.
    private static readonly Encoding WinAnsiEncoding = GetWinAnsiEncoding();

    private static Encoding GetWinAnsiEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1252);
    }

    /// <summary>
    /// Initializes the canvas with default colors (black) for stroke and fill.
    /// </summary>
    public PdfCanvas()
    {
        _cmds.AppendLine("0 0 0 RG");
        _cmds.AppendLine("0 0 0 rg");
    }

    /// <summary>
    /// Formats a double value in the invariant culture (dot as decimal separator)
    /// with two decimal places. Uses cached CultureInfo to avoid lookup overhead.
    /// </summary>
    private static string F(double value)
        => value.ToString("F2", PdfConstants.InvariantCulture);

    // ── Basic shapes ─────────────────────────────────────────────────

    /// <summary>Draws a line between two points.</summary>
    public void DrawLine(double x1, double y1, double x2, double y2)
        => _cmds.AppendLine($"{F(x1)} {F(y1)} m {F(x2)} {F(y2)} l S");

    /// <summary>Draws a rectangle outline.</summary>
    public void DrawRectangle(double x, double y, double w, double h)
        => _cmds.AppendLine($"{F(x)} {F(y)} {F(w)} {F(h)} re S");

    /// <summary>Fills a rectangle with the current color.</summary>
    public void FillRectangle(double x, double y, double w, double h)
        => _cmds.AppendLine($"{F(x)} {F(y)} {F(w)} {F(h)} re f");

    /// <summary>Draws a grid of lines.</summary>
    public void DrawGrid(double x, double y, double w, double h, int cols, int rows)
    {
        if (cols <= 0 || rows <= 0) return;

        double cellWidth = w / cols;
        double cellHeight = h / rows;

        for (int i = 0; i <= rows; i++)
            DrawLine(x, y + i * cellHeight, x + w, y + i * cellHeight);

        for (int i = 0; i <= cols; i++)
            DrawLine(x + i * cellWidth, y, x + i * cellWidth, y + h);
    }

    // ── Images ───────────────────────────────────────────────────────

    /// <summary>
    /// Places a previously registered image (via <c>PdfBuilder.AddImage</c>).
    /// </summary>
    public void DrawImage(string name, double x, double y, double width, double height)
    {
        ArgumentNullException.ThrowIfNull(name);

        // Prevent PDF injection through image name (CWE-79)
        if (!PdfConstants.IsValidPdfName(name))
            throw new ArgumentException("Image name must be alphanumeric (dashes and underscores allowed).", nameof(name));

        // HashSet.Add is idempotent (no-op if already present)
        _usedImages.Add(name);

        _cmds.AppendLine($"q {F(width)} 0 0 {F(height)} {F(x)} {F(y)} cm /{name} Do Q");
    }

    // ── Text ─────────────────────────────────────────────────────────

    /// <summary>
    /// Draws simple text at the specified position.
    /// WinAnsi (1252) encoding is used for PDF compatibility.
    /// </summary>
    public void DrawText(string text, double x, double y, double size = 12)
    {
        string escaped = EscapePdfString(text);
        _cmds.AppendLine($"BT /F1 {F(size)} Tf {F(x)} {F(y)} Td ({escaped}) Tj ET");
    }

    /// <summary>
    /// Draws aligned text within a bounding area.
    /// </summary>
    public void DrawTextAligned(
        string text, double x, double y, double width, double height,
        TextAlign align = TextAlign.Left, double fontSize = 10)
    {
        double textWidth = text.Length * PdfConstants.TextWidthFactor * fontSize;
        double textHeight = fontSize;

        double posX = align switch
        {
            TextAlign.Center => x + (width - textWidth) / 2.0,
            TextAlign.Right => x + width - textWidth,
            _ => x
        };

        double posY = y + (height - textHeight) / 2.0 + (fontSize * PdfConstants.TextVerticalAdjust);
        DrawText(text, posX, posY, fontSize);
    }

    /// <summary>Draws a cell with optional border and aligned text.</summary>
    public void DrawCell(
        string text, double x, double y, double width, double height,
        TextAlign align = TextAlign.Left, double fontSize = 10, bool drawBorder = true)
    {
        if (drawBorder) DrawRectangle(x, y, width, height);
        DrawTextAligned(text, x, y, width, height, align, fontSize);
    }

    // ── Table ────────────────────────────────────────────────────────

    /// <summary>Draws a complete table with rows, columns and text.</summary>
    /// <param name="data">2D string array [row, column].</param>
    /// <param name="x">X position of the bottom-left corner.</param>
    /// <param name="y">Y position of the bottom-left corner.</param>
    /// <param name="colWidths">Width of each column.</param>
    /// <param name="rowHeight">Height of each row.</param>
    /// <param name="headerAlign">Alignment for the header row (first row).</param>
    /// <param name="dataAlign">Alignment for data rows.</param>
    /// <param name="fontSize">Font size.</param>
    /// <exception cref="ArgumentException">If column count does not match.</exception>
    public void DrawTable(
        string[,] data, double x, double y, double[] colWidths, double rowHeight,
        TextAlign headerAlign = TextAlign.Center, TextAlign dataAlign = TextAlign.Left,
        double fontSize = 10)
    {
        int rows = data.GetLength(0);
        int cols = data.GetLength(1);

        if (cols != colWidths.Length)
            throw new ArgumentException(
                $"Column count ({cols}) does not match provided widths ({colWidths.Length}).",
                nameof(colWidths));

        DrawTableGrid(x, y, rows, cols, colWidths, rowHeight);
        DrawTableText(data, x, y, rows, cols, colWidths, rowHeight, headerAlign, dataAlign, fontSize);
    }

    // ── Barcode ──────────────────────────────────────────────────────

    /// <summary>Draws a barcode from a list of bars.</summary>
    public void DrawBarcode(List<Bar> bars, double x, double y, double moduleWidth, double height)
    {
        double currentX = x;
        foreach (var bar in bars)
        {
            double barWidth = bar.Width * moduleWidth;
            if (bar.Black)
                FillRectangle(currentX, y, barWidth, height);
            currentX += barWidth;
        }
    }

    // ── Utilities ────────────────────────────────────────────────────

    /// <summary>Returns all accumulated PDF command content.</summary>
    public string GetContent() => _cmds.ToString();

    /// <summary>Draws the horizontal and vertical lines of a table grid.</summary>
    private void DrawTableGrid(double x, double y, int rows, int cols, double[] colWidths, double rowHeight)
    {
        double totalWidth = colWidths.Sum();

        // Horizontal lines
        for (int r = 0; r <= rows; r++)
            DrawLine(x, y + r * rowHeight, x + totalWidth, y + r * rowHeight);

        // Vertical lines
        double currentX = x;
        for (int c = 0; c <= cols; c++)
        {
            DrawLine(currentX, y, currentX, y + rows * rowHeight);
            if (c < cols) currentX += colWidths[c];
        }
    }

    /// <summary>Draws the text cells of a table.</summary>
    private void DrawTableText(string[,] data, double x, double y, int rows, int cols,
        double[] colWidths, double rowHeight, TextAlign headerAlign, TextAlign dataAlign, double fontSize)
    {
        double currentX;
        for (int r = 0; r < rows; r++)
        {
            currentX = x;
            for (int c = 0; c < cols; c++)
            {
                TextAlign align = r == 0 ? headerAlign : dataAlign;
                DrawTextAligned(
                    data[r, c], currentX, y + r * rowHeight,
                    colWidths[c], rowHeight, align, fontSize);
                currentX += colWidths[c];
            }
        }
    }

    /// <summary>
    /// Escapes a string for use in PDF text operators,
    /// encoding in WinAnsi (1252) and escaping special characters.
    /// Single-pass: writes to a pooled char buffer then constructs
    /// the result string — zero intermediate StringBuilder/byte[] allocations.
    /// </summary>
    private static string EscapePdfString(string text)
    {
        // Guard against string exhaustion: limit text length to prevent OOM
        if (text.Length > PdfConstants.MaxTextLength)
            throw new ArgumentException(
                $"Text exceeds maximum length of {PdfConstants.MaxTextLength} characters for PDF rendering.",
                nameof(text));

        // Rent byte buffer from ArrayPool (avoids byte[] allocation per call)
        int maxByteCount = WinAnsiEncoding.GetMaxByteCount(text.Length);
        byte[] rentedBytes = ArrayPool<byte>.Shared.Rent(maxByteCount);
        try
        {
            int byteCount = WinAnsiEncoding.GetBytes(text, 0, text.Length, rentedBytes, 0);
            Span<byte> src = rentedBytes.AsSpan(0, byteCount);

            // Worst case: every byte becomes \xxx octal (4 chars each).
            // Rent generously then build result string from the used portion.
            char[] rentedChars = ArrayPool<char>.Shared.Rent(byteCount * 4);
            try
            {
                int pos = 0;
                foreach (byte b in src)
                {
                    if (b == '(' || b == ')' || b == '\\')
                    {
                        rentedChars[pos++] = '\\';
                        rentedChars[pos++] = (char)b;
                    }
                    else if (b < 32 || b > 126)
                    {
                        rentedChars[pos++] = '\\';
                        rentedChars[pos++] = (char)('0' + (b / 64));
                        rentedChars[pos++] = (char)('0' + ((b / 8) % 8));
                        rentedChars[pos++] = (char)('0' + (b % 8));
                    }
                    else
                    {
                        rentedChars[pos++] = (char)b;
                    }
                }

                return new string(rentedChars, 0, pos);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rentedChars);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedBytes);
        }
    }
}

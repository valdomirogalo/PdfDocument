namespace PdfDocument;

/// <summary>
/// Code 39 barcode generation (alphanumeric).
/// </summary>
public static class Code39
{
    // Code 39 encoding table: character → 9-bit pattern (1=wide bar, 0=narrow bar)
    private static readonly Dictionary<char, string> EncodingTable = new()
    {
        {'0', "000110100"}, {'1', "100100001"}, {'2', "001100001"}, {'3', "101100000"},
        {'4', "000110001"}, {'5', "100110000"}, {'6', "001110000"}, {'7', "000100101"},
        {'8', "100100100"}, {'9', "001100100"}, {'A', "100001001"}, {'B', "001001001"},
        {'C', "101001000"}, {'D', "000011001"}, {'E', "100011000"}, {'F', "001011000"},
        {'G', "000001101"}, {'H', "100001100"}, {'I', "001001100"}, {'J', "000011100"},
        {'K', "100000011"}, {'L', "001000011"}, {'M', "101000010"}, {'N', "000010011"},
        {'O', "100010010"}, {'P', "001010010"}, {'Q', "000000111"}, {'R', "100000110"},
        {'S', "001000110"}, {'T', "000010110"}, {'U', "110000001"}, {'V', "011000001"},
        {'W', "111000000"}, {'X', "010010001"}, {'Y', "110010000"}, {'Z', "011010000"},
        {'-', "010000101"}, {'.', "110000100"}, {' ', "011000100"}, {'$', "010101000"},
        {'/', "010100010"}, {'+', "010001010"}, {'%', "000101010"}, {'*', "010010100"}
    };

    /// <summary>
    /// Generates the bar list for the specified text in Code 39 format.
    /// </summary>
    /// <param name="text">Text to encode (alphanumeric plus - . $ / + %).</param>
    /// <returns>List of bars representing the barcode.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentException">If the text contains invalid characters.</exception>
    public static List<Bar> Generate(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        text = text.ToUpperInvariant();

        if (text.Length == 0 || !text.All(c => EncodingTable.ContainsKey(c)))
        {
            var invalid = text.FirstOrDefault(c => !EncodingTable.ContainsKey(c));
            throw new ArgumentException(
                $"Invalid character for Code 39: '{(invalid == default ? "(empty)" : invalid.ToString())}'.",
                nameof(text));
        }

        var bars = new List<Bar>(text.Length * 10 + 18); // Estimated capacity
        AppendChar('*', bars); // start/stop
        foreach (char c in text)
            AppendChar(c, bars);
        AppendChar('*', bars);
        return bars;
    }

    /// <summary>
    /// Adds the bars for a character to the list, including the inter-character gap.
    /// </summary>
    private static void AppendChar(char c, List<Bar> bars)
    {
        string code = EncodingTable[c];
        foreach (char bit in code)
        {
            bool isBar = bit == '1';
            int width = isBar ? PdfConstants.BarBlackWidth : PdfConstants.BarWhiteWidth;
            bars.Add(new Bar { Width = width, Black = isBar });
        }
        // Inter-character gap (1 white module)
        bars.Add(new Bar { Width = PdfConstants.InterCharGapWidth, Black = false });
    }
}

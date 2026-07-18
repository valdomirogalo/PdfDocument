namespace PdfDocument;

/// <summary>
/// EAN-13 barcode generation (13 numeric digits).
/// </summary>
#pragma warning disable CA2208 // nameof(digits) is correct within Generate method scope
public static class EAN13
#pragma warning restore CA2208
{
    // Encoding tables for the three sets A, B, C
    private static readonly string[] EncA =
    [
        "0001101", "0011001", "0010011", "0111101", "0100011",
        "0110001", "0101111", "0111011", "0110111", "0001011"
    ];

    private static readonly string[] EncB =
    [
        "0100111", "0110011", "0011011", "0100001", "0011101",
        "0111001", "0000101", "0010001", "0001001", "0010111"
    ];

    private static readonly string[] EncC =
    [
        "1110010", "1100110", "1101100", "1000010", "1011100",
        "1001110", "1010000", "1000100", "1001000", "1110100"
    ];

    // Pattern table for the first digit (defines which encoding set to use at each position)
    private static readonly int[,] PatternTable =
    {
        {0, 0, 0, 0, 0, 0}, // 0 -> A A A A A A
        {0, 0, 1, 0, 1, 1}, // 1 -> A A B A B B
        {0, 0, 1, 1, 0, 1}, // 2 -> A A B B A B
        {0, 0, 1, 1, 1, 0}, // 3 -> A A B B B A
        {0, 1, 0, 0, 1, 1}, // 4 -> A B A A B B
        {0, 1, 1, 0, 0, 1}, // 5 -> A B B A A B
        {0, 1, 1, 1, 0, 0}, // 6 -> A B B B A A
        {0, 1, 0, 1, 0, 1}, // 7 -> A B A B A B
        {0, 1, 0, 1, 1, 0}, // 8 -> A B A B B A
        {0, 1, 1, 0, 1, 0}  // 9 -> A B B A B A
    };

    /// <summary>
    /// Generates the bar list for the specified EAN-13 code.
    /// </summary>
    /// <param name="digits">
    /// String with 12 digits (check digit is calculated automatically)
    /// or 13 digits (check digit validation).
    /// </param>
    /// <returns>List of bars representing the EAN-13 barcode.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="digits"/> is null.</exception>
    /// <exception cref="ArgumentException">If the input is invalid.</exception>
    public static List<Bar> Generate(string digits)
    {
        ArgumentNullException.ThrowIfNull(digits);

        // Remove hyphens and spaces
        string clean = digits.Replace("-", "").Replace(" ", "");

        if (clean.Length == PdfConstants.Ean12DigitCount && clean.All(char.IsDigit))
        {
            int check = CalculateCheckDigit(clean);
            clean += check.ToString();
        }
        else if (clean.Length != PdfConstants.Ean13DigitCount || !clean.All(char.IsDigit))
        {
            throw new ArgumentException(
                "EAN-13 requires 12 or 13 numeric digits.",
                nameof(digits));
        }

        // Validate the check digit if 13 digits were provided
        if (clean.Length == PdfConstants.Ean13DigitCount)
        {
            ReadOnlySpan<char> span = clean.AsSpan();
            int check = CalculateCheckDigit(span[..PdfConstants.Ean12DigitCount]);
            int providedCheckDigit = span[PdfConstants.Ean12DigitCount] - '0';
            if (providedCheckDigit != check)
            {
                throw new ArgumentException(
                    $"Invalid check digit. Expected: {check}, provided: {providedCheckDigit}.",
                    nameof(digits));
            }
        }

        var bars = new List<Bar>(100); // Estimated capacity

        int firstDigit = clean[0] - '0';

        // Left guard (101)
        AppendGuard(bars, "101");

        // 6 left-side digits (sets A/B according to pattern)
        for (int i = 0; i < PdfConstants.EanPatternCount; i++)
        {
            int digit = clean[i + 1] - '0';
            string enc = PatternTable[firstDigit, i] == 0 ? EncA[digit] : EncB[digit];
            AppendEncoding(enc, bars);
            if (i < PdfConstants.EanPatternCount - 1)
                bars.Add(new Bar { Width = 1, Black = false });
        }

        // Center guard (01010)
        AppendGuard(bars, "01010");

        // 6 right-side digits (set C)
        for (int i = 0; i < PdfConstants.EanPatternCount; i++)
        {
            int digit = clean[i + 7] - '0';
            AppendEncoding(EncC[digit], bars);
            if (i < PdfConstants.EanPatternCount - 1)
                bars.Add(new Bar { Width = 1, Black = false });
        }

        // Right guard (101)
        AppendGuard(bars, "101");

        return bars;
    }

    private static void AppendEncoding(string pattern, List<Bar> bars)
    {
        foreach (char c in pattern)
            bars.Add(new Bar { Width = 1, Black = c == '1' });
    }

    private static void AppendGuard(List<Bar> bars, string pattern)
    {
        foreach (char c in pattern)
            bars.Add(new Bar { Width = 1, Black = c == '1' });
    }

    /// <summary>
    /// Calculates the check digit for the first 12 digits of an EAN-13.
    /// </summary>
    private static int CalculateCheckDigit(ReadOnlySpan<char> first12)
    {
        int sum = 0;
        for (int i = 0; i < first12.Length; i++)
        {
            int digit = first12[i] - '0';
            sum += (i % 2 == 0) ? digit * 1 : digit * 3;
        }
        return (10 - (sum % 10)) % 10;
    }
}

namespace PdfDocument;

/// <summary>
/// Code 128 barcode generation — uses Code128C (compact digits) for DANFE access keys.
/// </summary>
public static class Code128
{
    // Code 128 character encodings (11 modules per char: 6 bars, 3 black + 3 white)
    // Values 0-105: each is a 6-element array of bar widths (sum = 11).
    private static readonly byte[][] Patterns =
    [
        [2,1,2,2,2,2], // 0  (space)
        [2,2,2,1,2,2], // 1  !
        [2,2,2,2,2,1], // 2  "
        [1,2,1,2,2,3], // 3  #
        [1,2,1,3,2,2], // 4  $
        [1,3,1,2,2,2], // 5  %
        [1,2,2,2,1,3], // 6  &
        [1,2,2,3,1,2], // 7  '
        [1,3,2,2,1,2], // 8  (
        [2,2,1,2,1,3], // 9  )
        [2,2,1,3,1,2], // 10 *
        [2,3,1,2,1,2], // 11 +
        [1,1,2,2,3,2], // 12 ,
        [1,2,2,1,3,2], // 13 -
        [1,2,2,2,3,1], // 14 .
        [1,1,3,2,2,2], // 15 /
        [1,2,3,1,2,2], // 16 0
        [1,2,3,2,2,1], // 17 1
        [2,2,3,2,1,1], // 18 2
        [2,2,1,1,3,2], // 19 3
        [2,2,1,2,3,1], // 20 4
        [2,1,3,2,1,2], // 21 5
        [2,2,3,1,1,2], // 22 6
        [3,1,2,1,3,1], // 23 7
        [3,1,1,2,2,2], // 24 8
        [3,2,1,1,2,2], // 25 9
        [3,2,1,2,2,1], // 26 :
        [3,1,2,2,1,2], // 27 ;
        [3,2,2,1,1,2], // 28 <
        [3,2,2,2,1,1], // 29 =
        [2,1,2,1,2,3], // 30 >
        [2,1,2,3,2,1], // 31 ?
        [2,3,2,1,2,1], // 32 @
        [1,1,1,3,2,3], // 33 A
        [1,3,1,1,2,3], // 34 B
        [1,3,1,3,2,1], // 35 C
        [1,1,2,3,1,3], // 36 D
        [1,3,2,1,1,3], // 37 E
        [1,3,2,3,1,1], // 38 F
        [2,1,1,3,1,3], // 39 G
        [2,3,1,1,1,3], // 40 H
        [2,3,1,3,1,1], // 41 I
        [1,1,2,1,3,3], // 42 J
        [1,1,2,3,3,1], // 43 K
        [1,3,2,1,3,1], // 44 L
        [1,1,3,1,2,3], // 45 M
        [1,1,3,3,2,1], // 46 N
        [1,3,3,1,2,1], // 47 O
        [3,1,3,1,2,1], // 48 P
        [2,1,1,3,3,1], // 49 Q
        [2,3,1,1,3,1], // 50 R
        [2,1,3,1,1,3], // 51 S
        [2,1,3,3,1,1], // 52 T
        [2,1,3,1,3,1], // 53 U
        [3,1,1,1,2,3], // 54 V
        [3,1,1,3,2,1], // 55 W
        [3,3,1,1,2,1], // 56 X
        [3,1,2,1,1,3], // 57 Y
        [3,1,2,3,1,1], // 58 Z
        [3,3,2,1,1,1], // 59 [
        [3,1,4,1,1,1], // 60 backslash
        [2,2,1,4,1,1], // 61 ]
        [4,3,1,1,1,1], // 62 ^
        [1,1,1,2,2,4], // 63 _
        [1,1,1,4,2,2], // 64 `
        [1,2,1,1,2,4], // 65 a
        [1,2,1,4,2,1], // 66 b
        [1,4,1,1,2,1], // 67 c
        [1,1,2,2,1,4], // 68 d
        [1,1,2,4,1,2], // 69 e
        [1,2,2,1,1,4], // 70 f
        [1,2,2,4,1,1], // 71 g
        [1,4,2,1,1,1], // 72 h
        [2,1,1,1,2,4], // 73 i
        [2,1,1,4,2,1], // 74 j
        [2,4,1,1,2,1], // 75 k
        [1,1,2,1,4,2], // 76 l
        [1,1,2,2,4,1], // 77 m
        [2,1,2,1,4,1], // 78 n
        [1,4,2,1,4,0], // 79 o (adjusted for sum=11)
        [2,1,1,2,1,4], // 80 p
        [2,1,1,2,4,1], // 81 q
        [2,4,1,2,1,1], // 82 r
        [1,1,4,2,1,2], // 83 s
        [1,2,4,1,1,2], // 84 t
        [1,2,4,2,1,1], // 85 u
        [4,1,1,2,1,2], // 86 v
        [4,1,1,2,2,1], // 87 w
        [4,2,1,1,1,2], // 88 x
        [4,2,1,1,2,1], // 89 y
        [4,1,2,1,1,2], // 90 z
        [4,1,2,1,2,1], // 91 {
        [1,1,1,2,4,2], // 92 |
        [1,1,1,4,4,1], // 93 }
        [1,2,1,1,4,2], // 94 ~
        [1,2,1,4,4,1], // 95 DEL
        [1,1,4,2,4,1], // 96 FNC3
        [4,1,1,1,1,4], // 97 FNC2
        [4,1,1,1,4,1], // 98 SHIFT
        [1,4,1,1,1,4], // 99 CODE_C
        [1,4,1,1,4,1], // 100 CODE_B
        [4,1,4,1,1,1], // 101 CODE_A
        [1,1,4,1,4,1], // 102 FNC1
        [4,1,1,4,1,1], // 103 START_A
        [1,4,1,4,1,1], // 104 START_B
        [1,1,1,4,1,4], // 105 START_C (compact digits)
        [2,1,1,4,4,1], // 106 STOP (extra wide)
    ];

    private const int StartCodeC = 105;
    private const int StopPattern = 106;

    /// <summary>
    /// Generates Code128C barcode bars from a numeric string.
    /// Code128C encodes pairs of digits (00-99) efficiently — ideal for 44-digit DANFE keys.
    /// </summary>
    /// <param name="digits">Even-length numeric string.</param>
    /// <returns>List of bars representing the barcode.</returns>
    public static List<Bar> Generate(string digits)
    {
        ArgumentNullException.ThrowIfNull(digits);

        // Remove non-digits
        var clean = new string(digits.Where(char.IsDigit).ToArray());

        if (clean.Length == 0 || clean.Length % 2 != 0)
            throw new ArgumentException("Code128C requires an even number of digits.", nameof(digits));

        // Build data codewords (pairs of digits)
        var codewords = new List<int> { StartCodeC };

        for (int i = 0; i < clean.Length; i += 2)
        {
            int pair = int.Parse(clean.Substring(i, 2));
            codewords.Add(pair);
        }

        // Check digit (weighted sum modulo 103)
        int sum = StartCodeC; // START_C has weight 1
        for (int i = 1; i < codewords.Count; i++)
            sum += codewords[i] * i;
        codewords.Add(sum % 103);

        // Generate bars
        var bars = new List<Bar>();
        foreach (int c in codewords)
            AddPattern(bars, Patterns[c]);

        // Stop pattern (special: 13 modules, last bar is extra wide)
        var stop = Patterns[StopPattern];
        for (int i = 0; i < stop.Length - 1; i++)
            bars.Add(new Bar { Width = stop[i], Black = i % 2 == 0 });
        // Last bar of stop is 2 modules
        bars.Add(new Bar { Width = 2, Black = true });

        return bars;
    }

    private static void AddPattern(List<Bar> bars, byte[] pattern)
    {
        for (int i = 0; i < pattern.Length; i++)
            bars.Add(new Bar { Width = pattern[i], Black = i % 2 == 0 });
    }
}

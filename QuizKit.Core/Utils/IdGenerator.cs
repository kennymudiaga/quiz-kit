namespace QuizKit.Core.Utils;

public static class IdGenerator
{
    static readonly char[] CharSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    static readonly DateTime Epoch = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    static readonly Random random = new();

    public static string GenerateId(int minLength = 8)
    {
        var prefix = new string(Enumerable.Range(0, 2).Select(x => CharSet[random.Next(CharSet.Length)]).ToArray());
        var date = DateTime.UtcNow;
        var elapsed = (long)(date - Epoch).TotalMilliseconds;
        var codeElapsed = Base36Encode(elapsed);
        int sumElapsed = 0;
        foreach (var c in codeElapsed)
        {
            sumElapsed += Array.IndexOf(CharSet, c);
        }
        var id = $"{prefix}{Base36Encode(sumElapsed)}{Base36Encode(date.Microsecond)}";

        var padding = minLength - id.Length;
        while (padding-- > 0)
        {
            id += CharSet[random.Next(CharSet.Length)];
        }

        return id;
    }

    private static string Base36Encode(long value) => Encode(value, CharSet);

    private static string Encode(long value, char[] charset)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "value must be positive");
        }
        var numberBase = charset.Length;
        var result = new Stack<char>();
        do
        {
            result.Push(charset[value % numberBase]);
            value /= numberBase;
        } while (value != 0);

        return new string(result.ToArray());
    }
}

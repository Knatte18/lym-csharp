using System.Globalization;
using System.Text.RegularExpressions;

namespace LymCsharp.Regression;

/// <summary>
/// Removes from output lines what changes between runs without a code change,
/// so a baseline compares equal across runs and machines.
/// Timestamps become <c>&lt;timestamp&gt;</c>, machine roots become <c>&lt;path&gt;</c>,
/// and every free-standing number is rewritten to 10 significant digits in invariant culture.
/// </summary>
public sealed partial class OutputScrubber
{
    private readonly Regex[] machineRootPatterns;

    /// <summary>Creates a scrubber that replaces the given directory paths; a filesystem root is ignored.</summary>
    public OutputScrubber(IEnumerable<string> machineRoots)
    {
        machineRootPatterns = machineRoots
            .Select(Path.TrimEndingDirectorySeparator)
            .Where(root => root.Length > 0 && Path.GetPathRoot(root) != root)
            .Distinct()
            .OrderByDescending(root => root.Length)
            .Select(root => new Regex(Regex.Escape(root) + @"(?![\w-])"))
            .ToArray();
    }

    /// <summary>
    /// Creates a scrubber for this machine's roots: the baseline directory,
    /// the current directory, the temp directory and the user's home directory.
    /// </summary>
    public static OutputScrubber ForMachine(string baselineDirectory) =>
        new([
            Path.GetFullPath(baselineDirectory),
            Environment.CurrentDirectory,
            Path.GetTempPath(),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ]);

    /// <summary>Returns the line with timestamps, machine roots and number formatting scrubbed.</summary>
    public string Scrub(string line)
    {
        var scrubbed = TimestampPattern().Replace(line, "<timestamp>");
        foreach (var rootPattern in machineRootPatterns)
        {
            scrubbed = rootPattern.Replace(scrubbed, "<path>");
        }

        return NumberPattern().Replace(
            scrubbed,
            match => double.Parse(match.Value, CultureInfo.InvariantCulture)
                .ToString("G10", CultureInfo.InvariantCulture));
    }

    [GeneratedRegex(@"\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?")]
    private static partial Regex TimestampPattern();

    // A number glued to a letter or a dot, such as the 3 in m3/s or a part of 1.2.3, is not a value.
    [GeneratedRegex(@"(?<![\w.])-?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?(?![\w.])")]
    private static partial Regex NumberPattern();
}

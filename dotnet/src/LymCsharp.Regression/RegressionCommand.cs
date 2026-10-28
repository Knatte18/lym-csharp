namespace LymCsharp.Regression;

/// <summary>
/// The harness's command line: <c>capture|compare &lt;baseline_dir&gt; [--skip-file &lt;path&gt;]</c>.
/// Exits 0 on success, 1 when compare finds drift, and 2 on a usage error.
/// </summary>
public static class RegressionCommand
{
    public const int Success = 0;
    public const int Drift = 1;
    public const int UsageError = 2;

    private const string Usage = "usage: capture|compare <baseline_dir> [--skip-file <path>]";

    /// <summary>
    /// Runs the mode named by <paramref name="args"/> over <paramref name="cases"/>, less those the skip file names,
    /// writing drift reports to <paramref name="output"/> and usage errors to <paramref name="error"/>.
    /// A skip file names one case per line; a missing skip file means no skips.
    /// </summary>
    public static int Run(string[] args, IReadOnlyList<RegressionCase> cases, TextWriter output, TextWriter error)
    {
        var argumentsValid = (args.Length == 2 || (args.Length == 4 && args[2] == "--skip-file"))
                             && args[0] is "capture" or "compare"
                             && !string.IsNullOrWhiteSpace(args[1]);
        if (!argumentsValid)
        {
            error.WriteLine(Usage);
            return UsageError;
        }

        var skipFile = args.Length == 4 ? args[3] : null;

        var skipped = skipFile is not null && File.Exists(skipFile)
            ? File.ReadAllLines(skipFile).Select(line => line.Trim()).Where(line => line.Length > 0).ToHashSet()
            : [];
        var unknown = skipped.Where(name => cases.All(c => c.Name != name)).ToList();
        if (unknown.Count > 0)
        {
            error.WriteLine($"unknown case in skip file: {string.Join(", ", unknown)}");
            return UsageError;
        }

        var harness = new BaselineHarness(args[1], cases.Where(c => !skipped.Contains(c.Name)).ToList());
        if (args[0] == "capture")
        {
            harness.Capture();
            return Success;
        }

        var deviations = harness.Compare();
        foreach (var deviation in deviations)
        {
            output.WriteLine($"drift: {deviation}");
        }

        return deviations.Count == 0 ? Success : Drift;
    }
}

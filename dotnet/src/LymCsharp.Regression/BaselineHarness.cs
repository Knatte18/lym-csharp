using System.Globalization;

namespace LymCsharp.Regression;

/// <summary>
/// The first line where a case's scrubbed output departs from its baseline.
/// A null line is absent: the file ended, or the baseline file does not exist.
/// </summary>
public sealed record CaseDeviation(string CaseName, int LineNumber, string? BaselineLine, string? CurrentLine)
{
    public override string ToString() =>
        $"{CaseName} line {LineNumber}: baseline {Show(BaselineLine)}, current {Show(CurrentLine)}";

    private static string Show(string? line) => line is null ? "<absent>" : $"\"{line}\"";
}

/// <summary>
/// Captures the scrubbed output of regression cases into a baseline directory, one <c>&lt;case&gt;.txt</c> per case,
/// and compares a later run against it.
/// Cases render under the invariant culture, so the current culture never reaches the output.
/// </summary>
public sealed class BaselineHarness(string baselineDirectory, IReadOnlyList<RegressionCase> cases)
{
    private readonly OutputScrubber scrubber = OutputScrubber.ForMachine(baselineDirectory);

    /// <summary>Writes every case's scrubbed output to the baseline directory, creating it when absent.</summary>
    public void Capture()
    {
        Directory.CreateDirectory(baselineDirectory);
        foreach (var regressionCase in cases)
        {
            File.WriteAllLines(BaselinePath(regressionCase), RenderScrubbed(regressionCase));
        }
    }

    /// <summary>
    /// Compares every case's scrubbed output with its baseline file
    /// and returns the first deviation of each drifted case; an empty list means nothing drifted.
    /// </summary>
    public IReadOnlyList<CaseDeviation> Compare()
    {
        var deviations = new List<CaseDeviation>();
        foreach (var regressionCase in cases)
        {
            var path = BaselinePath(regressionCase);
            var baseline = File.Exists(path) ? File.ReadAllLines(path) : [];
            var deviation = FirstDeviation(regressionCase.Name, baseline, RenderScrubbed(regressionCase));
            if (deviation is not null)
            {
                deviations.Add(deviation);
            }
        }

        return deviations;
    }

    private static CaseDeviation? FirstDeviation(
        string caseName, IReadOnlyList<string> baseline, IReadOnlyList<string> current)
    {
        for (var index = 0; index < Math.Max(baseline.Count, current.Count); index++)
        {
            var baselineLine = index < baseline.Count ? baseline[index] : null;
            var currentLine = index < current.Count ? current[index] : null;
            if (baselineLine != currentLine)
            {
                return new CaseDeviation(caseName, index + 1, baselineLine, currentLine);
            }
        }

        return null;
    }

    private string BaselinePath(RegressionCase regressionCase) =>
        Path.Combine(baselineDirectory, regressionCase.Name + ".txt");

    private IReadOnlyList<string> RenderScrubbed(RegressionCase regressionCase)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            return regressionCase.Render().Select(scrubber.Scrub).ToList();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}

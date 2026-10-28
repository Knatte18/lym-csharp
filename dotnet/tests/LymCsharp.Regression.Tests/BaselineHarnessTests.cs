using System.Globalization;
using LymCsharp.Regression;

namespace LymCsharp.Regression.Tests;

public class BaselineHarnessTests
{
    private string _workDirectory = null!;
    private string _baselineDirectory = null!;

    [SetUp]
    public void CreateWorkDirectory()
    {
        _workDirectory = Path.Combine(Path.GetTempPath(), "lym-regression-" + Guid.NewGuid().ToString("N"));
        _baselineDirectory = Path.Combine(_workDirectory, "nested", "baseline");
    }

    [TearDown]
    public void RemoveWorkDirectory()
    {
        if (Directory.Exists(_workDirectory))
        {
            Directory.Delete(_workDirectory, recursive: true);
        }
    }

    [Test]
    public void CaptureWritesOneScrubbedFilePerCase()
    {
        RegressionCase[] cases =
        [
            new("first", () => ["value = 1.2732395447351628", "at 2026-10-28T09:32:22Z"]),
            new("second", () => ["regime = Laminar"]),
        ];

        new BaselineHarness(_baselineDirectory, cases).Capture();

        Assert.Multiple(() =>
        {
            Assert.That(
                Directory.GetFiles(_baselineDirectory).Select(Path.GetFileName),
                Is.EquivalentTo(new[] { "first.txt", "second.txt" }));
            Assert.That(
                File.ReadAllLines(Path.Combine(_baselineDirectory, "first.txt")),
                Is.EqualTo(new[] { "value = 1.273239545", "at <timestamp>" }));
            Assert.That(
                File.ReadAllLines(Path.Combine(_baselineDirectory, "second.txt")),
                Is.EqualTo(new[] { "regime = Laminar" }));
        });
    }

    [Test]
    public void CompareAgainstFreshCaptureOfEveryCaseFindsNoDeviation()
    {
        var harness = new BaselineHarness(_baselineDirectory, RegressionCases.All);

        harness.Capture();

        Assert.That(harness.Compare(), Is.Empty);
    }

    [Test]
    [SetCulture("nb-NO")]
    public void CasesRenderInInvariantCultureWhateverTheCurrentCulture()
    {
        var norwegianDirectory = Path.Combine(_workDirectory, "norwegian");
        var invariantDirectory = Path.Combine(_workDirectory, "invariant");

        new BaselineHarness(norwegianDirectory, RegressionCases.All).Capture();
        var cultureOfFirstCapture = CultureInfo.CurrentCulture.Name;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        new BaselineHarness(invariantDirectory, RegressionCases.All).Capture();

        Assert.Multiple(() =>
        {
            Assert.That(cultureOfFirstCapture, Is.EqualTo("nb-NO"));
            foreach (var regressionCase in RegressionCases.All)
            {
                var fileName = regressionCase.Name + ".txt";
                Assert.That(
                    File.ReadAllLines(Path.Combine(norwegianDirectory, fileName)),
                    Is.EqualTo(File.ReadAllLines(Path.Combine(invariantDirectory, fileName))),
                    fileName);
            }
        });
    }

    [Test]
    public void CaptureReplacesTheBaselineDirectoryPathInACaseLine()
    {
        RegressionCase[] cases = [new("paths", () => [$"read {_baselineDirectory}{Path.DirectorySeparatorChar}out.txt"])];

        new BaselineHarness(_baselineDirectory, cases).Capture();

        Assert.That(
            File.ReadAllLines(Path.Combine(_baselineDirectory, "paths.txt")),
            Is.EqualTo(new[] { $"read <path>{Path.DirectorySeparatorChar}out.txt" }));
    }

    [Test]
    public void CompareReportsTheFirstDeviationOfEachDriftedCase()
    {
        string[] changed = ["a = 1", "b = 2", "c = 3"];
        string[] longer = ["a = 1"];
        string[] shorter = ["a = 1", "b = 2"];
        RegressionCase[] cases =
        [
            new("unchanged", () => ["a = 1"]),
            new("changed", () => changed),
            new("longer", () => longer),
            new("shorter", () => shorter),
            new("missing", () => ["a = 1"]),
        ];
        var harness = new BaselineHarness(_baselineDirectory, cases);
        harness.Capture();
        File.Delete(Path.Combine(_baselineDirectory, "missing.txt"));

        changed[1] = "b = 2.5";
        changed[2] = "c = 4";
        longer = ["a = 1", "b = 2"];
        shorter = ["a = 1"];

        Assert.That(harness.Compare(), Is.EqualTo(new[]
        {
            new CaseDeviation("changed", 2, "b = 2", "b = 2.5"),
            new CaseDeviation("longer", 2, null, "b = 2"),
            new CaseDeviation("shorter", 2, "b = 2", null),
            new CaseDeviation("missing", 1, null, "a = 1"),
        }));
    }
}

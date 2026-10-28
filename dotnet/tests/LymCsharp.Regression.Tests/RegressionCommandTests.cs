using LymCsharp.Regression;

namespace LymCsharp.Regression.Tests;

public class RegressionCommandTests
{
    private string _workDirectory = null!;
    private string _baselineDirectory = null!;
    private string _skipFile = null!;
    private string _valueOfFirst = null!;
    private RegressionCase[] _cases = null!;

    [SetUp]
    public void CreateWorkDirectory()
    {
        _workDirectory = Path.Combine(Path.GetTempPath(), "lym-regression-" + Guid.NewGuid().ToString("N"));
        _baselineDirectory = Path.Combine(_workDirectory, "baseline");
        _skipFile = Path.Combine(_workDirectory, "skip.txt");
        Directory.CreateDirectory(_workDirectory);
        _valueOfFirst = "value = 1";
        _cases =
        [
            new("first", () => [_valueOfFirst]),
            new("second", () => ["value = 2"]),
        ];
    }

    [TearDown]
    public void RemoveWorkDirectory()
    {
        Directory.Delete(_workDirectory, recursive: true);
    }

    [Test]
    public void CompareExitsZeroAndPrintsNothingWhenNothingDrifted()
    {
        Run("capture", _baselineDirectory);

        var (exitCode, output, _) = Run("compare", _baselineDirectory);

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(RegressionCommand.Success));
            Assert.That(output, Is.Empty);
        });
    }

    [Test]
    public void CompareExitsOneAndNamesTheCaseLineAndBothValuesOnDrift()
    {
        Run("capture", _baselineDirectory);
        _valueOfFirst = "value = 1.5";

        var (exitCode, output, _) = Run("compare", _baselineDirectory);

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(RegressionCommand.Drift));
            Assert.That(
                output,
                Is.EqualTo($"drift: first line 1: baseline \"value = 1\", current \"value = 1.5\"{Environment.NewLine}"));
        });
    }

    [Test]
    public void SkippedCaseIsNeitherCapturedNorCompared()
    {
        File.WriteAllLines(_skipFile, ["second", ""]);

        var (captureExitCode, _, _) = Run("capture", _baselineDirectory, "--skip-file", _skipFile);
        var capturedFiles = Directory.GetFiles(_baselineDirectory).Select(Path.GetFileName).ToList();
        var (compareExitCode, output, _) = Run("compare", _baselineDirectory, "--skip-file", _skipFile);

        Assert.Multiple(() =>
        {
            Assert.That(captureExitCode, Is.EqualTo(RegressionCommand.Success));
            Assert.That(capturedFiles, Is.EqualTo(new[] { "first.txt" }));
            Assert.That(compareExitCode, Is.EqualTo(RegressionCommand.Success));
            Assert.That(output, Is.Empty);
        });
    }

    [Test]
    public void MissingSkipFileSkipsNothing()
    {
        var (exitCode, _, _) = Run("capture", _baselineDirectory, "--skip-file", _skipFile);

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(RegressionCommand.Success));
            Assert.That(
                Directory.GetFiles(_baselineDirectory).Select(Path.GetFileName),
                Is.EquivalentTo(new[] { "first.txt", "second.txt" }));
        });
    }

    [Test]
    public void UnknownSkipNameIsAUsageError()
    {
        File.WriteAllLines(_skipFile, ["third"]);

        var (exitCode, _, error) = Run("capture", _baselineDirectory, "--skip-file", _skipFile);

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(RegressionCommand.UsageError));
            Assert.That(error, Is.EqualTo($"unknown case in skip file: third{Environment.NewLine}"));
            Assert.That(Directory.Exists(_baselineDirectory), Is.False);
        });
    }

    [TestCase("")]
    [TestCase("capture")]
    [TestCase("restore baseline")]
    [TestCase("compare baseline --skip skip.txt")]
    [TestCase("compare baseline extra")]
    public void MalformedArgumentsAreAUsageError(string commandLine)
    {
        var (exitCode, output, error) = Run(commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(RegressionCommand.UsageError));
            Assert.That(output, Is.Empty);
            Assert.That(
                error,
                Is.EqualTo($"usage: capture|compare <baseline_dir> [--skip-file <path>]{Environment.NewLine}"));
        });
    }

    [TestCase("capture")]
    [TestCase("compare")]
    public void EmptyBaselineDirectoryIsAUsageError(string mode)
    {
        var (exitCode, output, error) = Run(mode, "");

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(RegressionCommand.UsageError));
            Assert.That(output, Is.Empty);
            Assert.That(
                error,
                Is.EqualTo($"usage: capture|compare <baseline_dir> [--skip-file <path>]{Environment.NewLine}"));
        });
    }

    private (int ExitCode, string Output, string Error) Run(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = RegressionCommand.Run(args, _cases, output, error);
        return (exitCode, output.ToString(), error.ToString());
    }
}

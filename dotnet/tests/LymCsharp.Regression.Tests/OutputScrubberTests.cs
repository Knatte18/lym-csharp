using LymCsharp.Regression;

namespace LymCsharp.Regression.Tests;

public class OutputScrubberTests
{
    private static readonly OutputScrubber Scrubber = new(["/machine/work/", "/machine", "/"]);

    [TestCase("captured 2026-10-28T09:32:22Z", "captured <timestamp>")]
    [TestCase("at 2026-10-28 09:32:22.123+02:00 done", "at <timestamp> done")]
    [TestCase("read /machine/work/out/case.txt", "read <path>/out/case.txt")]
    [TestCase("read /machine/other", "read <path>/other")]
    [TestCase("read /machinery/other", "read /machinery/other")]
    [TestCase("read /machine/work", "read <path>")]
    [TestCase("failed in /machine: denied", "failed in <path>: denied")]
    [TestCase("x = 1.2732395447351628", "x = 1.273239545")]
    [TestCase("reynolds = 127323.95447351628", "reynolds = 127323.9545")]
    [TestCase("dp = 1e-5", "dp = 1E-05")]
    [TestCase("delta = -0.50", "delta = -0.5")]
    [TestCase("flow = 10 l/s, unit m3/s", "flow = 10 l/s, unit m3/s")]
    [TestCase("version 1.2.3", "version 1.2.3")]
    public void ScrubsTimestampsMachineRootsAndNumberFormatting(string line, string expected)
    {
        Assert.That(Scrubber.Scrub(line), Is.EqualTo(expected));
    }
}

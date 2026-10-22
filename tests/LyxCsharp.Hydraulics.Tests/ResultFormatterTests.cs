using LyxCsharp.Hydraulics;
using LyxCsharp.Reporting;
using LyxCsharp.Units;

namespace LyxCsharp.Hydraulics.Tests;

public class ResultFormatterTests
{
    private static readonly PipeFlowResult Sample =
        new(1.5, 50_000, FlowRegime.Turbulent, 0.02, 200_000);

    [Test]
    public void FormatsInPascals()
    {
        var text = ResultFormatter.Format(Sample, UnitCatalog.Pascal);

        Assert.That(text, Does.Contain("Turbulent flow").And.Contain("200000 Pa"));
    }

    [Test]
    public void FormatsInBar()
    {
        var text = ResultFormatter.Format(Sample, UnitCatalog.Bar);

        Assert.That(text, Does.Contain("2 bar"));
    }

    [Test]
    public void IncludesVelocityAndReynolds()
    {
        var text = ResultFormatter.Format(Sample, UnitCatalog.Pascal);

        Assert.That(text, Does.Contain("v = 1.5 m/s").And.Contain("Re = 50000"));
    }
}

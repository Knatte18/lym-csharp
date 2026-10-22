namespace LyxCsharp.Hydraulics;

/// <summary>Computes the Darcy friction factor.</summary>
public interface IFrictionModel
{
    double FrictionFactor(double reynolds, double relativeRoughness);
}

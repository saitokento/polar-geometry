namespace PolarGeometry
{
    public interface IPolarFunction
    {
        float Evaluate(float theta);

        float? ThetaSpan { get; }
    }
}
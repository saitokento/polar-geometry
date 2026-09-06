using UnityEngine;

namespace PolarGeometry
{
    public abstract class PolarFunction : MonoBehaviour, IPolarFunction
    {
        public abstract float Evaluate(float theta);
        public abstract float? ThetaSpan { get; }
        public abstract string Formula { get; }
    }
}
using UnityEngine;

namespace PolarGeometry
{
    public abstract class PolarFunction : MonoBehaviour, IPolarFunction
    {
        public abstract float Evaluate(float theta);
        public abstract float? Period { get; }
    }
}
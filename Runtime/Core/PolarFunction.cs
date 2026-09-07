using UnityEngine;

namespace PolarGeometry
{
    public abstract class PolarFunction : MonoBehaviour, IPolarFunction
    {
        public event System.Action Changed;
        public abstract float Evaluate(float theta);
        public abstract float? ThetaSpan { get; }
        public abstract string Formula { get; }

        protected void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
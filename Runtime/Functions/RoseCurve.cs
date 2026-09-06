using UnityEngine;

namespace PolarGeometry
{
    public class RoseCurve : PolarFunction
    {
        public override string Formula => "a * cos(k * θ)";

        [SerializeField]
        float a = 1f;

        [SerializeField]
        float k = 3;

        public override float Evaluate(float theta)
        {
            return a * Mathf.Cos(k * theta);
        }

        public override float? ThetaSpan
        {
            get
            {
                if (k == 0)
                {
                    return Mathf.PI * 2f;
                }

                if (k % 1f != 0f)
                {
                    return null;
                }

                return Mathf.Abs((int)k) % 2 == 0
                    ? Mathf.PI * 2f
                    : Mathf.PI;
            }

        }
    }
}
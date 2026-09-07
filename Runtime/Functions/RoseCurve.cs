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

        public float A
        {
            get => a;
            set
            {
                if (a == value)
                    return;

                a = value;

                NotifyChanged();
            }
        }

        public float K
        {
            get => k;
            set
            {
                if (k == value)
                    return;

                k = value;

                NotifyChanged();
            }
        }

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
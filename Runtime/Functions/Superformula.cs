using UnityEngine;

namespace PolarGeometry
{
    public class Superformula : PolarFunction
    {
        public override string Formula =>
            "(|cos(m * θ / 4) / a|^n2 + " +
            "|sin(m * θ / 4) / b|^n3)^(-1 / n1)";

        [SerializeField]
        private float a = 1f;

        [SerializeField]
        private float b = 1f;

        [SerializeField]
        private float m = 6f;

        [SerializeField]
        private float n1 = 1f;

        [SerializeField]
        private float n2 = 1f;

        [SerializeField]
        private float n3 = 1f;

        public float A
        {
            get => a;
            set => SetValue(
                ref a,
                value
            );
        }

        public float B
        {
            get => b;
            set => SetValue(
                ref b,
                value
            );
        }

        public float M
        {
            get => m;
            set => SetValue(
                ref m,
                value
            );
        }

        public float N1
        {
            get => n1;
            set => SetValue(
                ref n1,
                value
            );
        }

        public float N2
        {
            get => n2;
            set => SetValue(
                ref n2,
                value
            );
        }

        public float N3
        {
            get => n3;
            set => SetValue(
                ref n3,
                value
            );
        }

        public override float? ThetaSpan =>
            Mathf.PI * 2f;

        public override float Evaluate(
            float theta)
        {
            float angle =
                m * theta * 0.25f;

            float cosTerm =
                Mathf.Pow(
                    Mathf.Abs(
                        Mathf.Cos(angle) /
                        a
                    ),
                    n2
                );

            float sinTerm =
                Mathf.Pow(
                    Mathf.Abs(
                        Mathf.Sin(angle) /
                        b
                    ),
                    n3
                );

            return Mathf.Pow(
                cosTerm + sinTerm,
                -1f / n1
            );
        }

        public void SetParameters(
            float a,
            float b,
            float m,
            float n1,
            float n2,
            float n3)
        {
            bool changed =
                this.a != a ||
                this.b != b ||
                this.m != m ||
                this.n1 != n1 ||
                this.n2 != n2 ||
                this.n3 != n3;

            if (!changed)
                return;

            this.a = a;
            this.b = b;
            this.m = m;
            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;

            NotifyChanged();
        }

        private void SetValue(
            ref float field,
            float value)
        {
            if (field == value)
                return;

            field = value;

            NotifyChanged();
        }
    }
}
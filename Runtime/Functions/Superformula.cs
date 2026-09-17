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
            GetMinimumClosureSpan();

        public float? GetMinimumClosureSpan(
            int maxDenominator = 10000,
            float tolerance = 0.000001f)
        {
            if (maxDenominator < 1)
                return null;

            if (tolerance < 0f ||
                float.IsNaN(tolerance) ||
                float.IsInfinity(tolerance))
                return null;

            if (m == 0f)
                return Mathf.PI * 2f;

            long numerator;
            long denominator;

            if (!TryGetRational(
                Mathf.Abs(m),
                maxDenominator,
                tolerance,
                out numerator,
                out denominator))
                return null;

            return GetClosureSpan(
                numerator,
                denominator
            );
        }

        public float? GetMinimumClosureSpanForRationalM(
            long numerator,
            long denominator)
        {
            if (denominator == 0L ||
                numerator == long.MinValue ||
                denominator == long.MinValue)
                return null;

            numerator = System.Math.Abs(numerator);
            denominator = System.Math.Abs(denominator);

            if (numerator == 0L)
                return Mathf.PI * 2f;

            long divisor =
                GreatestCommonDivisor(
                    numerator,
                    denominator
                );

            return GetClosureSpan(
                numerator / divisor,
                denominator / divisor
            );
        }

        private float? GetClosureSpan(
            long numerator,
            long denominator)
        {

            bool hasHalfPeriod =
                a == b &&
                n2 == n3;

            double span;

            if (hasHalfPeriod)
            {
                span =
                    2.0 * Mathf.PI *
                    denominator;
            }
            else
            {
                long parityDivisor =
                    GreatestCommonDivisor(
                        numerator,
                        2L
                    );

                span =
                    4.0 * Mathf.PI *
                    denominator /
                    parityDivisor;
            }

            if (span > float.MaxValue)
                return null;

            return (float)span;
        }

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

        private static bool TryGetRational(
            float value,
            int maxDenominator,
            float tolerance,
            out long numerator,
            out long denominator)
        {
            numerator = 0L;
            denominator = 0L;

            if (float.IsNaN(value) ||
                float.IsInfinity(value))
                return false;

            double target = value;

            for (long candidateDenominator = 1L;
                candidateDenominator <= maxDenominator;
                candidateDenominator++)
            {
                double scaled =
                    target * candidateDenominator;

                if (scaled > long.MaxValue)
                    return false;

                long candidateNumerator =
                    (long)System.Math.Round(scaled);

                if (candidateNumerator == 0L)
                    continue;

                double error =
                    System.Math.Abs(
                        target -
                        (double)candidateNumerator /
                        candidateDenominator
                    );

                if (error > tolerance)
                    continue;

                long divisor =
                    GreatestCommonDivisor(
                        candidateNumerator,
                        candidateDenominator
                    );

                numerator =
                    candidateNumerator / divisor;

                denominator =
                    candidateDenominator / divisor;

                return true;
            }

            return false;
        }

        private static long GreatestCommonDivisor(
            long left,
            long right)
        {
            while (right != 0L)
            {
                long remainder =
                    left % right;

                left = right;
                right = remainder;
            }

            return left;
        }
    }
}

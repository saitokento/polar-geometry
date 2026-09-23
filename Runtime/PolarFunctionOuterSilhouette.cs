using System.Collections.Generic;
using UnityEngine;

namespace PolarGeometry
{
    public static class PolarFunctionOuterSilhouette
    {
        private const float FullTurn =
            2f * Mathf.PI;

        /// <summary>
        /// Samples the outer silhouette of a PolarFunction.
        ///
        /// When ThetaSpan exceeds 2PI, the largest
        /// radius at each angle is selected.
        ///
        /// Returns a single revolution of points.
        /// </summary>
        public static bool TrySample(
            PolarFunction function,
            int sampleCount,
            out List<Vector2> positions,
            float startTheta = 0f)
        {
            positions = null;

            if (function == null)
            {
                Debug.LogError(
                    "PolarFunction is null."
                );

                return false;
            }

            if (function.ThetaSpan is not float thetaSpan)
            {
                Debug.LogError(
                    "PolarFunction has no known ThetaSpan."
                );

                return false;
            }

            if (
                !IsFinite(thetaSpan) ||
                thetaSpan < FullTurn - 0.0001f
            )
            {
                Debug.LogError(
                    "ThetaSpan must be at least 2PI."
                );

                return false;
            }

            int count =
                Mathf.Max(16, sampleCount);

            int turnCount =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        thetaSpan / FullTurn
                    )
                );

            positions =
                new List<Vector2>(count);

            for (int i = 0; i < count; i++)
            {
                // Angle within one revolution
                float localTheta =
                    FullTurn * i / count;

                float maxRadius = 0f;

                // Compare every revolution
                for (int turn = 0; turn < turnCount; turn++)
                {
                    float relativeTheta =
                        localTheta +
                        FullTurn * turn;

                    if (relativeTheta >= thetaSpan)
                    {
                        break;
                    }

                    float theta =
                        startTheta + relativeTheta;

                    float radius =
                        function.Evaluate(theta);

                    if (
                        !IsFinite(radius) ||
                        radius < 0f
                    )
                    {
                        positions = null;

                        return false;
                    }

                    maxRadius =
                        Mathf.Max(
                            maxRadius,
                            radius
                        );
                }

                // Convert polar coordinates
                // to Cartesian coordinates
                float outputTheta =
                    startTheta + localTheta;

                float x =
                    maxRadius *
                    Mathf.Cos(outputTheta);

                float y =
                    maxRadius *
                    Mathf.Sin(outputTheta);

                if (
                    !IsFinite(x) ||
                    !IsFinite(y)
                )
                {
                    positions = null;

                    return false;
                }

                positions.Add(
                    new Vector2(x, y)
                );
            }

            return true;
        }

        private static bool IsFinite(float value)
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }
    }
}

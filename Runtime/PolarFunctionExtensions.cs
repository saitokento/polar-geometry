using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolarGeometry
{
    public static class PolarFunctionExtensions
    {
        public static List<Vector2> SamplePositions(
            this IPolarFunction function,
            float startTheta,
            float endTheta,
            float deltaTheta,
            bool includeEnd = true,
            float tolerance = 0f)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            if (deltaTheta <= 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTheta));

            if (tolerance < 0f)
                throw new ArgumentOutOfRangeException(nameof(tolerance));

            List<Vector2> positions = new List<Vector2>();

            float range = endTheta - startTheta;

            if (range == 0f)
            {
                if (includeEnd)
                {
                    positions.Add(
                        EvaluatePosition(function, startTheta)
                    );
                }

                return positions;
            }

            float direction = Mathf.Sign(range);
            float step = deltaTheta * direction;

            int stepCount =
                Mathf.FloorToInt(Mathf.Abs(range) / deltaTheta);

            for (int i = 0; i <= stepCount; i++)
            {
                float theta = startTheta + step * i;

                bool reachedEnd =
                    direction > 0f
                        ? theta >= endTheta
                        : theta <= endTheta;

                if (reachedEnd)
                {
                    if (includeEnd)
                    {
                        positions.Add(
                            EvaluatePosition(function, endTheta)
                        );
                    }

                    break;
                }

                positions.Add(
                    EvaluatePosition(function, theta)
                );
            }

            if (includeEnd)
            {
                float lastTheta =
                    startTheta + step * stepCount;

                bool reachedEnd =
                    direction > 0f
                        ? lastTheta >= endTheta
                        : lastTheta <= endTheta;

                if (!reachedEnd)
                {
                    positions.Add(
                        EvaluatePosition(function, endTheta)
                    );
                }
            }

            return SimplifyIfNeeded(
                positions,
                tolerance
            );
        }

        public static List<Vector2> SamplePositions(
            this IPolarFunction function,
            float startTheta,
            float deltaTheta,
            bool includeEnd = false,
            float tolerance = 0f)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            if (function.ThetaSpan is not float thetaSpan)
            {
                throw new InvalidOperationException(
                    "The polar function does not have a known theta span."
                );
            }

            return function.SamplePositions(
                startTheta,
                startTheta + thetaSpan,
                deltaTheta,
                includeEnd,
                tolerance
            );
        }

        public static List<Vector2> SamplePositionsAdaptive(
            this IPolarFunction function,
            float startTheta,
            float endTheta,
            float maxCoordinateDelta,
            float maxDeltaTheta,
            bool includeEnd = true,
            int maxDepth = 16,
            float tolerance = 0f)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            if (maxCoordinateDelta <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxCoordinateDelta)
                );
            }

            if (maxDeltaTheta <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxDeltaTheta)
                );
            }

            if (maxDepth < 0)
                throw new ArgumentOutOfRangeException(nameof(maxDepth));

            if (tolerance < 0f)
                throw new ArgumentOutOfRangeException(nameof(tolerance));

            List<Vector2> positions = new List<Vector2>();

            if (startTheta == endTheta)
            {
                if (includeEnd)
                {
                    positions.Add(
                        EvaluatePosition(function, startTheta)
                    );
                }

                return positions;
            }

            Vector2 startPosition =
                EvaluatePosition(function, startTheta);

            Vector2 endPosition =
                EvaluatePosition(function, endTheta);

            positions.Add(startPosition);

            SampleSegmentAdaptive(
                function,
                startTheta,
                startPosition,
                endTheta,
                endPosition,
                maxCoordinateDelta,
                maxDeltaTheta,
                maxDepth,
                0,
                positions
            );

            if (!includeEnd)
            {
                positions.RemoveAt(positions.Count - 1);
            }

            return SimplifyIfNeeded(
                positions,
                tolerance
            );
        }

        public static List<Vector2> SamplePositionsAdaptive(
            this IPolarFunction function,
            float startTheta,
            float maxCoordinateDelta,
            float maxDeltaTheta,
            bool includeEnd = false,
            int maxDepth = 16,
            float tolerance = 0f)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            if (function.ThetaSpan is not float thetaSpan)
            {
                throw new InvalidOperationException(
                    "The polar function does not have a known theta span."
                );
            }

            return function.SamplePositionsAdaptive(
                startTheta,
                startTheta + thetaSpan,
                maxCoordinateDelta,
                maxDeltaTheta,
                includeEnd,
                maxDepth,
                tolerance
            );
        }

        private static Vector2 EvaluatePosition(
            IPolarFunction function,
            float theta)
        {
            float r = function.Evaluate(theta);

            return new PolarCoordinate(r, theta)
                .ToCartesian();
        }

        private static void SampleSegmentAdaptive(
            IPolarFunction function,
            float theta0,
            Vector2 position0,
            float theta1,
            Vector2 position1,
            float maxCoordinateDelta,
            float maxDeltaTheta,
            int maxDepth,
            int depth,
            List<Vector2> positions)
        {
            float deltaX =
                Mathf.Abs(position1.x - position0.x);

            float deltaY =
                Mathf.Abs(position1.y - position0.y);

            float deltaTheta =
                Mathf.Abs(theta1 - theta0);

            bool shouldSubdivide =
                depth < maxDepth &&
                (
                    deltaX > maxCoordinateDelta ||
                    deltaY > maxCoordinateDelta ||
                    deltaTheta > maxDeltaTheta
                );

            if (!shouldSubdivide)
            {
                positions.Add(position1);
                return;
            }

            float thetaMid =
                (theta0 + theta1) * 0.5f;

            Vector2 positionMid =
                EvaluatePosition(function, thetaMid);

            SampleSegmentAdaptive(
                function,
                theta0,
                position0,
                thetaMid,
                positionMid,
                maxCoordinateDelta,
                maxDeltaTheta,
                maxDepth,
                depth + 1,
                positions
            );

            SampleSegmentAdaptive(
                function,
                thetaMid,
                positionMid,
                theta1,
                position1,
                maxCoordinateDelta,
                maxDeltaTheta,
                maxDepth,
                depth + 1,
                positions
            );
        }

        private static List<Vector2> SimplifyIfNeeded(
            List<Vector2> positions,
            float tolerance)
        {
            if (tolerance == 0f || positions.Count <= 2)
                return positions;

            List<Vector2> simplified =
                new List<Vector2>();

            LineUtility.Simplify(
                positions,
                tolerance,
                simplified
            );

            return simplified;
        }
    }
}
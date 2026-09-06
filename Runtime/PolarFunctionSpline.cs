using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PolarGeometry
{
    [RequireComponent(typeof(SplineContainer))]
    public class PolarFunctionSpline : MonoBehaviour
    {
        [SerializeField]
        private PolarFunction function;

        [Header("Range")]
        [SerializeField]
        private bool useFunctionThetaSpan = false;

        [SerializeField]
        private float startThetaDegrees = 0f;

        [SerializeField]
        private float endThetaDegrees = 360f;

        [Header("Sampling")]
        [SerializeField]
        private bool useAdaptiveSampling = false;

        [SerializeField]
        private float deltaThetaDegrees = 1f;

        [SerializeField]
        private float maxCoordinateDelta = 0.01f;

        [SerializeField]
        private float maxDeltaThetaDegrees = 10f;

        [SerializeField]
        private int maxDepth = 16;

        [SerializeField]
        private float tolerance = 0f;

        [Header("Spline")]
        [SerializeField]
        private bool loop = false;

        [SerializeField]
        private TangentMode tangentMode = TangentMode.Linear;

        private SplineContainer splineContainer;

        private void Awake()
        {
            splineContainer =
                GetComponent<SplineContainer>();

            if (function == null)
            {
                function = GetComponent<PolarFunction>();
            }
        }

        public void Generate()
        {
            if (function == null)
            {
                Debug.LogError(
                    "PolarFunction is not assigned.",
                    this
                );

                splineContainer.Spline =
                    new Spline();

                return;
            }

            float startTheta =
                startThetaDegrees * Mathf.Deg2Rad;

            float endTheta;

            if (useFunctionThetaSpan)
            {
                if (function.ThetaSpan is not float thetaSpan)
                {
                    Debug.LogError(
                        "The polar function does not have a known theta span.",
                        this
                    );

                    splineContainer.Spline =
                        new Spline();

                    return;
                }

                endTheta =
                    startTheta + thetaSpan;
            }
            else
            {
                endTheta =
                    endThetaDegrees * Mathf.Deg2Rad;
            }

            bool includeEnd =
                !(useFunctionThetaSpan && loop);

            List<Vector2> sampledPositions;

            if (useAdaptiveSampling)
            {
                float maxDeltaTheta =
                    maxDeltaThetaDegrees * Mathf.Deg2Rad;

                sampledPositions =
                    function.SamplePositionsAdaptive(
                        startTheta,
                        endTheta,
                        maxCoordinateDelta,
                        maxDeltaTheta,
                        includeEnd,
                        maxDepth,
                        tolerance: 0f
                    );
            }
            else
            {
                float deltaTheta =
                    deltaThetaDegrees * Mathf.Deg2Rad;

                sampledPositions =
                    function.SamplePositions(
                        startTheta,
                        endTheta,
                        deltaTheta,
                        includeEnd,
                        tolerance: 0f
                    );
            }


            List<float3> points =
                new List<float3>(sampledPositions.Count);

            for (int i = 0; i < sampledPositions.Count; i++)
            {
                Vector2 position =
                    sampledPositions[i];

                points.Add(
                    new float3(
                        position.x,
                        position.y,
                        0f
                    )
                );
            }

            Spline spline;

            if (tolerance > 0f)
            {
                bool success =
                    SplineUtility.FitSplineToPoints(
                        points,
                        tolerance,
                        loop,
                        out spline
                    );

                if (!success)
                {
                    Debug.LogWarning(
                        "Failed to fit spline to sampled positions.",
                        this
                    );

                    spline = CreateSplineFromPoints(
                        points
                    );
                }
            }
            else
            {
                spline =
                    CreateSplineFromPoints(
                        points
                    );
            }

            splineContainer.Spline =
                spline;

            spline.Closed = loop;

            splineContainer.Spline =
                spline;
        }

        private Spline CreateSplineFromPoints(
            List<float3> points)
        {
            Spline spline =
                new Spline();

            for (int i = 0; i < points.Count; i++)
            {
                spline.Add(
                    points[i],
                    tangentMode
                );
            }

            spline.Closed = loop;

            return spline;
        }
    }
}
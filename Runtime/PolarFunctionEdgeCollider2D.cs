using System.Collections.Generic;
using UnityEngine;

namespace PolarGeometry
{
    [RequireComponent(typeof(EdgeCollider2D))]
    public class PolarFunctionEdgeCollider2D : MonoBehaviour
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

        [Header("Shape")]
        [SerializeField]
        private bool loop = false;

        [SerializeField]
        private bool matchLineRendererWidth = false;

        private EdgeCollider2D edgeCollider;

        private void Awake()
        {
            edgeCollider = GetComponent<EdgeCollider2D>();

            if (function == null)
            {
                function = GetComponent<PolarFunction>();
            }
        }

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            if (function == null)
            {
                Debug.LogError(
                    "PolarFunction is not assigned.",
                    this
                );

                edgeCollider.points = new Vector2[0];
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

                    edgeCollider.points = new Vector2[0];
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
                        includeEnd: true,
                        maxDepth,
                        tolerance
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
                        includeEnd: true,
                        tolerance
                    );
            }

            if (loop && sampledPositions.Count > 1)
            {
                sampledPositions[^1] = sampledPositions[0];
            }

            edgeCollider.points =
                sampledPositions.ToArray();

            MatchLineRendererWidth();
        }

        private void MatchLineRendererWidth()
        {
            if (!matchLineRendererWidth)
                return;

            LineRenderer lineRenderer =
                GetComponent<LineRenderer>();

            if (lineRenderer == null)
            {
                Debug.LogWarning(
                    "LineRenderer was not found on the same GameObject.",
                    this
                );

                return;
            }

            edgeCollider.edgeRadius =
                lineRenderer.widthMultiplier * 0.5f;
        }
    }
}
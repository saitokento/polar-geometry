using System.Collections.Generic;
using UnityEngine;

namespace PolarGeometry
{
    [RequireComponent(typeof(LineRenderer))]
    public class PolarFunctionLineRenderer : MonoBehaviour
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

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();

            if (function == null)
            {
                function = GetComponent<PolarFunction>();
            }
        }

        private void Start()
        {
            Draw();
        }

        public void Draw()
        {
            if (function == null)
            {
                Debug.LogError(
                    "PolarFunction is not assigned.",
                    this
                );

                lineRenderer.positionCount = 0;
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

                    lineRenderer.positionCount = 0;
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
                !(useFunctionThetaSpan && lineRenderer.loop);

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
                        includeEnd,
                        tolerance
                    );
            }

            Vector3[] positions =
                new Vector3[sampledPositions.Count];

            for (int i = 0; i < sampledPositions.Count; i++)
            {
                Vector2 position = sampledPositions[i];

                positions[i] = new Vector3(
                    position.x,
                    position.y,
                    0f
                );
            }

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
    }
}
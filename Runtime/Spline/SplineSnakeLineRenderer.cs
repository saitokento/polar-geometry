using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PolarGeometry
{
    [RequireComponent(typeof(LineRenderer))]
    public class SplineSnakeLineRenderer : MonoBehaviour
    {
        [SerializeField]
        private SplineContainer splineContainer;

        [Header("Movement")]
        [SerializeField]
        [Min(0f)]
        private float speed = 1f;

        [SerializeField]
        [Min(0.0001f)]
        private float lineLength = 1f;

        [SerializeField]
        private bool loop = false;

        [Header("Sampling")]
        [SerializeField]
        [Min(0.0001f)]
        private float sampleDistance = 0.05f;

        private LineRenderer lineRenderer;

        private float traveledDistance;

        private void Awake()
        {
            lineRenderer =
                GetComponent<LineRenderer>();

            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
        }

        private void Start()
        {
            Restart();
        }

        private void Update()
        {
            if (splineContainer == null)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            Spline spline =
                splineContainer.Spline;

            if (!IsValidSpline(spline))
            {
                lineRenderer.positionCount = 0;
                return;
            }

            float splineLength =
                splineContainer.CalculateLength();

            if (splineLength <= 0f)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            traveledDistance +=
                speed * Time.deltaTime;

            float effectiveLineLength =
                Mathf.Min(
                    lineLength,
                    splineLength
                );

            if (loop && spline.Closed)
            {
                if (
                    traveledDistance >
                    splineLength + effectiveLineLength
                )
                {
                    traveledDistance =
                        effectiveLineLength +
                        Mathf.Repeat(
                            traveledDistance -
                            effectiveLineLength,
                            splineLength
                        );
                }
            }
            else
            {
                traveledDistance =
                    Mathf.Min(
                        traveledDistance,
                        splineLength
                    );
            }

            DrawSnake(
                spline,
                splineLength,
                effectiveLineLength
            );
        }

        public void Restart()
        {
            traveledDistance = 0f;

            if (splineContainer == null)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            Spline spline =
                splineContainer.Spline;

            if (!IsValidSpline(spline))
            {
                lineRenderer.positionCount = 0;
                return;
            }

            float splineLength =
                splineContainer.CalculateLength();

            DrawSnake(
                spline,
                splineLength,
                Mathf.Min(
                    lineLength,
                    splineLength
                )
            );
        }

        private void DrawSnake(
            Spline spline,
            float splineLength,
            float effectiveLineLength)
        {
            float headDistance =
                traveledDistance;

            float tailDistance =
                Mathf.Max(
                    0f,
                    headDistance -
                    effectiveLineLength
                );

            float visibleLength =
                headDistance -
                tailDistance;

            if (visibleLength <= 0f)
            {
                SetSinglePosition(
                    spline,
                    0f
                );

                return;
            }

            int segmentCount =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        visibleLength /
                        sampleDistance
                    )
                );

            int positionCount =
                segmentCount + 1;

            Vector3[] positions =
                new Vector3[positionCount];

            for (int i = 0; i < positionCount; i++)
            {
                float ratio =
                    (float)i /
                    segmentCount;

                float distance =
                    Mathf.Lerp(
                        tailDistance,
                        headDistance,
                        ratio
                    );

                if (loop && spline.Closed)
                {
                    distance =
                        Mathf.Repeat(
                            distance,
                            splineLength
                        );
                }
                else
                {
                    distance =
                        Mathf.Clamp(
                            distance,
                            0f,
                            splineLength
                        );
                }

                float t =
                    SplineUtility.GetNormalizedInterpolation(
                        spline,
                        distance,
                        PathIndexUnit.Distance
                    );

                float3 position =
                    splineContainer.EvaluatePosition(
                        spline,
                        t
                    );

                positions[i] =
                    new Vector3(
                        position.x,
                        position.y,
                        position.z
                    );
            }

            lineRenderer.positionCount =
                positions.Length;

            lineRenderer.SetPositions(
                positions
            );
        }

        private void SetSinglePosition(
            Spline spline,
            float distance)
        {
            float t =
                SplineUtility.GetNormalizedInterpolation(
                    spline,
                    distance,
                    PathIndexUnit.Distance
                );

            float3 position =
                splineContainer.EvaluatePosition(
                    spline,
                    t
                );

            lineRenderer.positionCount = 1;

            lineRenderer.SetPosition(
                0,
                new Vector3(
                    position.x,
                    position.y,
                    position.z
                )
            );
        }

        private static bool IsValidSpline(
            Spline spline)
        {
            return spline != null &&
                   spline.Count >= 2;
        }
    }
}
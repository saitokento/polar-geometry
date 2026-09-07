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

        private Spline runtimeSpline;

        private float splineLength;
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
            if (!IsValidSpline(runtimeSpline))
            {
                lineRenderer.positionCount = 0;
                return;
            }

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

            if (loop && runtimeSpline.Closed)
            {
                if (
                    traveledDistance >
                    splineLength +
                    effectiveLineLength
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
                effectiveLineLength
            );
        }

        public void Restart()
        {
            traveledDistance = 0f;

            if (splineContainer == null)
            {
                runtimeSpline = null;
                splineLength = 0f;

                lineRenderer.positionCount = 0;
                return;
            }

            Spline sourceSpline =
                splineContainer.Spline;

            if (!IsValidSpline(sourceSpline))
            {
                runtimeSpline = null;
                splineLength = 0f;

                lineRenderer.positionCount = 0;
                return;
            }

            runtimeSpline =
                new Spline(sourceSpline);

            splineLength =
                runtimeSpline.GetLength();

            if (splineLength <= 0f)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            DrawSnake(
                Mathf.Min(
                    lineLength,
                    splineLength
                )
            );
        }

        private void DrawSnake(
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
                SetSinglePosition(0f);
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

                if (
                    loop &&
                    runtimeSpline.Closed
                )
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

                positions[i] =
                    EvaluateWorldPosition(
                        distance
                    );
            }

            lineRenderer.positionCount =
                positions.Length;

            lineRenderer.SetPositions(
                positions
            );
        }

        private void SetSinglePosition(
            float distance)
        {
            lineRenderer.positionCount = 1;

            lineRenderer.SetPosition(
                0,
                EvaluateWorldPosition(
                    distance
                )
            );
        }

        private Vector3 EvaluateWorldPosition(
            float distance)
        {
            float t =
                SplineUtility.GetNormalizedInterpolation(
                    runtimeSpline,
                    distance,
                    PathIndexUnit.Distance
                );

            float3 localPosition =
                runtimeSpline.EvaluatePosition(t);

            return splineContainer.transform.TransformPoint(
                new Vector3(
                    localPosition.x,
                    localPosition.y,
                    localPosition.z
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
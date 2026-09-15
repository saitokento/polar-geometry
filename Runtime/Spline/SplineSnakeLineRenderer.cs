using System;
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

        [Header("Collider")]
        [SerializeField]
        private bool enableCollider = false;

        [SerializeField]
        private bool matchLineRendererWidth = true;

        private LineRenderer lineRenderer;
        private EdgeCollider2D edgeCollider;

        // 開始時に取得したSplineのコピー。
        // 元のSplineContainer.Splineが変更されても追従しない。
        private Spline runtimeSpline;

        private float splineLength;
        private float traveledDistance;

        private bool missingColliderWarningLogged;

        private void Awake()
        {
            lineRenderer =
                GetComponent<LineRenderer>();

            edgeCollider =
                GetComponent<EdgeCollider2D>();

            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;

            UpdateColliderState();
        }

        private void Start()
        {
            Restart();
        }

        private void Update()
        {
            UpdateColliderState();

            // Start時点ではPolarFunctionSplineが
            // まだ生成されていない可能性がある。
            // 有効なSplineを取得できるまで待つ。
            if (!IsValidSpline(runtimeSpline))
            {
                if (!TryInitializeRuntimeSpline())
                {
                    ClearOutput();
                    return;
                }

                DrawSnake(
                    Mathf.Min(
                        lineLength,
                        splineLength
                    )
                );
            }

            if (splineLength <= 0f)
            {
                ClearOutput();
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
            runtimeSpline = null;
            splineLength = 0f;

            ClearOutput();

            // ここで取得できなくても、
            // Update()で取得可能になるまで再試行する。
            if (!TryInitializeRuntimeSpline())
                return;

            DrawSnake(
                Mathf.Min(
                    lineLength,
                    splineLength
                )
            );
        }

        private bool TryInitializeRuntimeSpline()
        {
            if (splineContainer == null)
                return false;

            Spline sourceSpline =
                splineContainer.Spline;

            if (!IsValidSpline(sourceSpline))
                return false;

            // 現在のSplineをコピーする。
            // 以降のPolarFunctionSplineの変更には追従しない。
            runtimeSpline =
                new Spline(sourceSpline);

            splineLength =
                runtimeSpline.GetLength();

            if (splineLength <= 0f)
            {
                runtimeSpline = null;
                splineLength = 0f;

                return false;
            }

            traveledDistance = 0f;

            return true;
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

            for (
                int i = 0;
                i < positionCount;
                i++)
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

            UpdateCollider(
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

            // EdgeCollider2Dは2点以上必要。
            ClearColliderPoints();
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

        private void UpdateCollider(
            Vector3[] worldPositions)
        {
            if (!enableCollider)
            {
                ClearColliderPoints();
                return;
            }

            if (edgeCollider == null)
            {
                WarnMissingCollider();
                return;
            }

            if (worldPositions.Length < 2)
            {
                ClearColliderPoints();
                return;
            }

            Vector2[] colliderPoints =
                new Vector2[
                    worldPositions.Length
                ];

            for (
                int i = 0;
                i < worldPositions.Length;
                i++)
            {
                // LineRendererはworld spaceなので、
                // EdgeCollider2D用にこのGameObjectの
                // local spaceへ変換する。
                Vector3 localPosition =
                    transform.InverseTransformPoint(
                        worldPositions[i]
                    );

                colliderPoints[i] =
                    new Vector2(
                        localPosition.x,
                        localPosition.y
                    );
            }

            edgeCollider.points =
                colliderPoints;

            if (matchLineRendererWidth)
            {
                edgeCollider.edgeRadius =
                    lineRenderer.widthMultiplier *
                    0.5f;
            }
        }

        private void UpdateColliderState()
        {
            if (edgeCollider == null)
            {
                if (enableCollider)
                {
                    WarnMissingCollider();
                }

                return;
            }

            edgeCollider.enabled =
                enableCollider;

            if (!enableCollider)
            {
                ClearColliderPoints();
            }
        }

        private void ClearOutput()
        {
            lineRenderer.positionCount = 0;

            ClearColliderPoints();
        }

        private void ClearColliderPoints()
        {
            if (edgeCollider == null)
                return;

            edgeCollider.points =
                Array.Empty<Vector2>();
        }

        private void WarnMissingCollider()
        {
            if (missingColliderWarningLogged)
                return;

            Debug.LogWarning(
                "Enable Collider is enabled, but EdgeCollider2D is not attached.",
                this
            );

            missingColliderWarningLogged = true;
        }

        private static bool IsValidSpline(
            Spline spline)
        {
            return spline != null &&
                   spline.Count >= 2;
        }
    }
}

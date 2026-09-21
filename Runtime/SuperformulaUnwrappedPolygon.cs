using System.Collections.Generic;
using UnityEngine;

namespace PolarGeometry
{
    /// <summary>
    /// Unwraps a Superformula shape along +X while using its radius as Y.
    ///
    /// Continuous mapping:
    ///     y(theta) = r(theta)
    ///     dx       = 0.5 * r(theta) * dtheta
    ///
    /// Therefore:
    ///     integral y dx = 0.5 * integral r(theta)^2 dtheta,
    /// so the area of the original polar shape is preserved.
    ///
    /// For a circle r(theta) = R, the result is a rectangle
    /// with height R and width PI * R.
    /// </summary>
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(MeshFilter))]
    public class SuperformulaUnwrappedPolygon : MonoBehaviour
    {
        [SerializeField]
        private Superformula function;

        [Header("Range")]
        [SerializeField]
        private bool useFunctionThetaSpan = true;

        [SerializeField]
        private float startThetaDegrees = 0f;

        [SerializeField]
        private float endThetaDegrees = 360f;

        [Header("Sampling")]
        [SerializeField]
        private bool useAdaptiveSampling = true;

        [SerializeField]
        [Min(0.001f)]
        private float deltaThetaDegrees = 1f;

        [Tooltip("Adaptive sampling: refine when the Cartesian distance between interval endpoints exceeds this value. Set <= 0 to disable this criterion.")]
        [SerializeField]
        private float maxCoordinateDelta = 0.01f;

        [Tooltip("Adaptive sampling: maximum angular interval. Set <= 0 to disable this criterion.")]
        [SerializeField]
        private float maxDeltaThetaDegrees = 10f;

        [Tooltip("Adaptive sampling recursion limit.")]
        [SerializeField]
        [Range(1, 24)]
        private int maxDepth = 16;

        [Tooltip("Adaptive sampling: midpoint-to-chord error in the original polar curve. Set <= 0 to disable this criterion.")]
        [SerializeField]
        private float tolerance = 0.01f;

        [Header("Output")]
        [SerializeField]
        private bool enableCollider = true;

        [Tooltip("If enabled, shifts the generated strip so its center lies on local X = 0.")]
        [SerializeField]
        private bool centerHorizontally = false;

        [Header("Update")]
        [SerializeField]
        private bool autoUpdate = true;

        private PolygonCollider2D polygonCollider;
        private MeshFilter meshFilter;
        private Mesh generatedMesh;

        public float GeneratedWidth { get; private set; }
        public float GeneratedArea { get; private set; }

        private readonly struct PolarSample
        {
            public readonly float Theta;
            public readonly float Radius;

            public PolarSample(float theta, float radius)
            {
                Theta = theta;
                Radius = radius;
            }
        }

        private void Awake()
        {
            polygonCollider = GetComponent<PolygonCollider2D>();
            meshFilter = GetComponent<MeshFilter>();

            if (function == null)
            {
                function = GetComponent<Superformula>();
            }
        }

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            EnsureComponents();

            if (!TrySample(out List<PolarSample> samples))
            {
                Clear();
                return;
            }

            List<Vector2> polygon = BuildUnwrappedPolygon(
                samples,
                out float width,
                out float area
            );

            if (polygon.Count < 4)
            {
                Debug.LogError(
                    "At least four points are required to create the unwrapped polygon.",
                    this
                );

                Clear();
                return;
            }

            GeneratedWidth = width;
            GeneratedArea = area;

            polygonCollider.pathCount = 1;
            polygonCollider.SetPath(0, polygon);

            GenerateMesh();

            polygonCollider.enabled = enableCollider;
        }

        public void Clear()
        {
            EnsureComponents();

            GeneratedWidth = 0f;
            GeneratedArea = 0f;

            if (polygonCollider != null)
            {
                polygonCollider.pathCount = 0;
                polygonCollider.enabled = enableCollider;
            }

            ClearMesh();
        }

        private void EnsureComponents()
        {
            if (polygonCollider == null)
            {
                polygonCollider = GetComponent<PolygonCollider2D>();
            }

            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (function == null)
            {
                function = GetComponent<Superformula>();
            }
        }

        private bool TrySample(out List<PolarSample> samples)
        {
            samples = null;

            if (function == null)
            {
                Debug.LogError(
                    "Superformula is not assigned.",
                    this
                );

                return false;
            }

            if (!TryGetThetaRange(out float startTheta, out float endTheta))
            {
                return false;
            }

            if (useAdaptiveSampling)
            {
                return TrySampleAdaptive(
                    startTheta,
                    endTheta,
                    out samples
                );
            }

            return TrySampleUniform(
                startTheta,
                endTheta,
                out samples
            );
        }

        private bool TryGetThetaRange(
            out float startTheta,
            out float endTheta)
        {
            startTheta = startThetaDegrees * Mathf.Deg2Rad;

            if (useFunctionThetaSpan)
            {
                if (function.ThetaSpan is not float thetaSpan)
                {
                    Debug.LogError(
                        "The Superformula does not have a known theta span.",
                        this
                    );

                    endTheta = 0f;
                    return false;
                }

                endTheta = startTheta + thetaSpan;
            }
            else
            {
                endTheta = endThetaDegrees * Mathf.Deg2Rad;
            }

            if (!IsFinite(startTheta) ||
                !IsFinite(endTheta) ||
                endTheta <= startTheta)
            {
                Debug.LogError(
                    "Invalid theta range. endTheta must be greater than startTheta.",
                    this
                );

                return false;
            }

            return true;
        }

        private bool TrySampleUniform(
            float startTheta,
            float endTheta,
            out List<PolarSample> samples)
        {
            samples = new List<PolarSample>();

            float requestedStep =
                Mathf.Max(0.001f, deltaThetaDegrees) * Mathf.Deg2Rad;

            float span = endTheta - startTheta;
            int segmentCount = Mathf.Max(
                1,
                Mathf.CeilToInt(span / requestedStep)
            );

            samples.Capacity = segmentCount + 1;

            for (int i = 0; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                float theta = Mathf.Lerp(startTheta, endTheta, t);

                if (!TryEvaluateRadius(theta, out float radius))
                {
                    return false;
                }

                samples.Add(new PolarSample(theta, radius));
            }

            return samples.Count >= 2;
        }

        private bool TrySampleAdaptive(
            float startTheta,
            float endTheta,
            out List<PolarSample> samples)
        {
            samples = new List<PolarSample>();

            if (!TryEvaluateRadius(startTheta, out float startRadius))
            {
                return false;
            }

            samples.Add(new PolarSample(startTheta, startRadius));

            List<float> boundaries =
                BuildCriticalAngleBoundaries(startTheta, endTheta);

            for (int i = 0; i < boundaries.Count - 1; i++)
            {
                float a = boundaries[i];
                float b = boundaries[i + 1];

                float radiusA;

                if (i == 0)
                {
                    radiusA = startRadius;
                }
                else
                {
                    radiusA = samples[samples.Count - 1].Radius;
                }

                if (!TryEvaluateRadius(b, out float radiusB))
                {
                    return false;
                }

                if (!TrySubdivideAdaptive(
                    a,
                    radiusA,
                    b,
                    radiusB,
                    0,
                    samples))
                {
                    return false;
                }
            }

            return samples.Count >= 2;
        }

        private List<float> BuildCriticalAngleBoundaries(
            float startTheta,
            float endTheta)
        {
            List<float> boundaries = new List<float>
            {
                startTheta
            };

            float m = function.M;

            if (m != 0f && IsFinite(m))
            {
                double criticalStep =
                    2.0 * System.Math.PI /
                    System.Math.Abs((double)m);

                long firstIndex =
                    (long)System.Math.Floor(
                        startTheta / criticalStep
                    ) + 1L;

                for (long index = firstIndex; ; index++)
                {
                    double criticalThetaDouble =
                        index * criticalStep;

                    if (criticalThetaDouble >= endTheta)
                    {
                        break;
                    }

                    float criticalTheta =
                        (float)criticalThetaDouble;

                    if (criticalTheta > startTheta)
                    {
                        boundaries.Add(criticalTheta);
                    }

                    if (index == long.MaxValue)
                    {
                        break;
                    }
                }
            }

            boundaries.Add(endTheta);
            return boundaries;
        }

        private bool TrySubdivideAdaptive(
            float thetaA,
            float radiusA,
            float thetaB,
            float radiusB,
            int depth,
            List<PolarSample> output)
        {
            float thetaMid = 0.5f * (thetaA + thetaB);

            if (!TryEvaluateRadius(thetaMid, out float radiusMid))
            {
                return false;
            }

            bool refine =
                depth < maxDepth &&
                NeedsSubdivision(
                    thetaA,
                    radiusA,
                    thetaMid,
                    radiusMid,
                    thetaB,
                    radiusB
                );

            if (refine)
            {
                if (!TrySubdivideAdaptive(
                    thetaA,
                    radiusA,
                    thetaMid,
                    radiusMid,
                    depth + 1,
                    output))
                {
                    return false;
                }

                return TrySubdivideAdaptive(
                    thetaMid,
                    radiusMid,
                    thetaB,
                    radiusB,
                    depth + 1,
                    output
                );
            }

            output.Add(new PolarSample(thetaB, radiusB));
            return true;
        }

        private bool NeedsSubdivision(
            float thetaA,
            float radiusA,
            float thetaMid,
            float radiusMid,
            float thetaB,
            float radiusB)
        {
            float deltaTheta = thetaB - thetaA;
            float maxDeltaTheta =
                maxDeltaThetaDegrees * Mathf.Deg2Rad;

            if (maxDeltaTheta > 0f &&
                deltaTheta > maxDeltaTheta)
            {
                return true;
            }

            Vector2 pointA = PolarToCartesian(thetaA, radiusA);
            Vector2 pointMid = PolarToCartesian(thetaMid, radiusMid);
            Vector2 pointB = PolarToCartesian(thetaB, radiusB);

            if (maxCoordinateDelta > 0f &&
                Vector2.Distance(pointA, pointB) > maxCoordinateDelta)
            {
                return true;
            }

            if (tolerance > 0f)
            {
                Vector2 chordMid = 0.5f * (pointA + pointB);

                if (Vector2.Distance(pointMid, chordMid) > tolerance)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryEvaluateRadius(
            float theta,
            out float radius)
        {
            radius = function.Evaluate(theta);

            if (!IsFinite(radius) || radius < 0f)
            {
                Debug.LogError(
                    $"Superformula returned an invalid radius at theta={theta} rad: {radius}",
                    this
                );

                return false;
            }

            return true;
        }

        private static Vector2 PolarToCartesian(
            float theta,
            float radius)
        {
            return new Vector2(
                radius * Mathf.Cos(theta),
                radius * Mathf.Sin(theta)
            );
        }

        private List<Vector2> BuildUnwrappedPolygon(
            List<PolarSample> samples,
            out float width,
            out float area)
        {
            List<Vector2> points =
                new List<Vector2>(samples.Count + 2);

            width = 0f;
            area = 0f;

            // Left baseline -> left top -> top contour -> right baseline.
            points.Add(Vector2.zero);
            points.Add(new Vector2(0f, samples[0].Radius));

            for (int i = 1; i < samples.Count; i++)
            {
                PolarSample previous = samples[i - 1];
                PolarSample current = samples[i];

                float deltaTheta =
                    current.Theta - previous.Theta;

                float r0 = previous.Radius;
                float r1 = current.Radius;

                // Trapezoidal approximation of the original polar area:
                // dA ~= 1/4 * (r0^2 + r1^2) * dtheta
                float segmentArea =
                    0.25f *
                    (r0 * r0 + r1 * r1) *
                    deltaTheta;

                // Choose dx so the trapezoid under the unwrapped top edge
                // has exactly the same numerical area:
                // 0.5 * (r0 + r1) * dx = segmentArea.
                float radiusSum = r0 + r1;
                float deltaX =
                    radiusSum > Mathf.Epsilon
                        ? (2f * segmentArea) / radiusSum
                        : 0f;

                width += deltaX;
                area += segmentArea;

                points.Add(
                    new Vector2(width, r1)
                );
            }

            points.Add(new Vector2(width, 0f));

            if (centerHorizontally)
            {
                float offset = 0.5f * width;

                for (int i = 0; i < points.Count; i++)
                {
                    Vector2 point = points[i];
                    point.x -= offset;
                    points[i] = point;
                }
            }

            return points;
        }

        private void GenerateMesh()
        {
            ClearMesh();

            generatedMesh = polygonCollider.CreateMesh(
                useBodyPosition: false,
                useBodyRotation: false
            );

            if (generatedMesh == null)
            {
                Debug.LogError(
                    "Failed to create mesh from PolygonCollider2D.",
                    this
                );

                return;
            }

            if (polygonCollider.attachedRigidbody == null)
            {
                ConvertMeshToLocalSpace(generatedMesh);
            }

            generatedMesh.name =
                $"{name} Superformula Unwrapped Mesh";

            meshFilter.sharedMesh = generatedMesh;
        }

        private void ConvertMeshToLocalSpace(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Transform meshTransform = meshFilter.transform;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] =
                    meshTransform.InverseTransformPoint(vertices[i]);
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
        }

        private void ClearMesh()
        {
            if (meshFilter != null &&
                meshFilter.sharedMesh == generatedMesh)
            {
                meshFilter.sharedMesh = null;
            }

            if (generatedMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }

            generatedMesh = null;
        }

        private void OnEnable()
        {
            SubscribeFunction();
        }

        private void OnDisable()
        {
            UnsubscribeFunction();
        }

        private void OnDestroy()
        {
            ClearMesh();
        }

        private void SubscribeFunction()
        {
            if (function != null && autoUpdate)
            {
                function.Changed -= Generate;
                function.Changed += Generate;
            }
        }

        private void UnsubscribeFunction()
        {
            if (function != null)
            {
                function.Changed -= Generate;
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}

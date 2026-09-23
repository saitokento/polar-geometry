using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PolarGeometry
{
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PolarFunctionGate : MonoBehaviour
    {
        [Header("Function")]
        [SerializeField] private PolarFunction function;

        [Header("Wall")]
        [SerializeField] private Vector2 wallSize = new Vector2(10f, 10f);
        [SerializeField, Min(0.001f)] private float holeScale = 1f;

        [Header("Mesh")]
        [SerializeField] private bool generate3D = false;
        [SerializeField, Min(0.001f)] private float thickness = 0.5f;

        [Header("Sampling")]
        [SerializeField, Min(16)] private int sampleCount = 360;

        [Header("Output")]
        [SerializeField] private bool enableCollider = true;

        [Header("Update")]
        [SerializeField] private bool autoUpdate = true;

        private PolygonCollider2D polygonCollider;
        private MeshFilter meshFilter;
        private Mesh generatedMesh;

        // Read-only values used by ShapeGateGameManager.
        public PolarFunction Function => function;
        public float HoleScale => holeScale;
        public int SampleCount => sampleCount;
        public bool IsGenerated =>
            generatedMesh != null &&
            meshFilter != null &&
            meshFilter.sharedMesh == generatedMesh;

        private void Awake()
        {
            polygonCollider = GetComponent<PolygonCollider2D>();
            meshFilter = GetComponent<MeshFilter>();

            if (function == null)
                function = GetComponent<PolarFunction>();
        }

        private void Start() => Generate();
        private void OnEnable() => SubscribeFunction();
        private void OnDisable() => UnsubscribeFunction();
        private void OnDestroy() => ClearMesh();

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (function == null ||
                !IsFinite(wallSize.x) || !IsFinite(wallSize.y) ||
                wallSize.x <= 0f || wallSize.y <= 0f ||
                !IsFinite(holeScale) || holeScale <= 0f ||
                (generate3D && (!IsFinite(thickness) || thickness <= 0f)))
            {
                Debug.LogError("Invalid function, wall size, hole scale or thickness.", this);
                Clear();
                return;
            }

            if (!PolarFunctionOuterSilhouette.TrySample(
                    function, sampleCount, out List<Vector2> holePositions) ||
                holePositions == null || holePositions.Count < 3)
            {
                Debug.LogError("Failed to generate hole silhouette.", this);
                Clear();
                return;
            }

            float halfWidth = wallSize.x * 0.5f;
            float halfHeight = wallSize.y * 0.5f;

            Vector2[] wallPoints =
            {
                new Vector2(-halfWidth, -halfHeight),
                new Vector2( halfWidth, -halfHeight),
                new Vector2( halfWidth,  halfHeight),
                new Vector2(-halfWidth,  halfHeight)
            };

            Vector2[] holePoints = new Vector2[holePositions.Count];
            const float margin = 0.001f;

            for (int i = 0; i < holePoints.Length; i++)
            {
                Vector2 p = holePositions[i] * holeScale;
                if (!IsFinite(p.x) || !IsFinite(p.y) ||
                    Mathf.Abs(p.x) >= halfWidth - margin ||
                    Mathf.Abs(p.y) >= halfHeight - margin)
                {
                    Debug.LogError("Hole contains invalid coordinates or exceeds wall bounds.", this);
                    Clear();
                    return;
                }
                holePoints[i] = p;
            }

            // With solid material on the left of both loops:
            // outer = CCW, hole = CW.
            if (SignedArea(wallPoints) < 0f)
                System.Array.Reverse(wallPoints);
            if (SignedArea(holePoints) > 0f)
                System.Array.Reverse(holePoints);

            polygonCollider.pathCount = 2;
            polygonCollider.SetPath(0, wallPoints);
            polygonCollider.SetPath(1, holePoints);

            GenerateMesh(wallPoints, holePoints);
            polygonCollider.enabled = enableCollider;
        }

        private void GenerateMesh(Vector2[] wallPoints, Vector2[] holePoints)
        {
            ClearMesh();

            Mesh flatMesh = polygonCollider.CreateMesh(
                useBodyPosition: false,
                useBodyRotation: false);

            if (flatMesh == null)
            {
                Debug.LogError("Failed to create gate mesh from PolygonCollider2D.", this);
                return;
            }

            // Preserve the coordinate conversion of the original 2D gate.
            if (polygonCollider.attachedRigidbody == null)
                ConvertMeshToLocalSpace(flatMesh);

            if (generate3D)
            {
                generatedMesh = BuildExtrudedMesh(
                    flatMesh, wallPoints, holePoints, thickness);
                DestroyOwnedMesh(flatMesh); // Temporary 2D mesh is not displayed.
            }
            else
            {
                generatedMesh = flatMesh;
            }

            if (generatedMesh == null)
            {
                Debug.LogError("Failed to build extruded gate mesh.", this);
                return;
            }

            generatedMesh.name = $"{name} Polar Function Gate Mesh";
            meshFilter.sharedMesh = generatedMesh;
        }

        private Mesh BuildExtrudedMesh(
            Mesh flatMesh,
            Vector2[] wallPoints,
            Vector2[] holePoints,
            float depth)
        {
            Vector3[] flatVertices = flatMesh.vertices;
            int[] flatTriangles = flatMesh.triangles;
            if (flatVertices.Length < 3 || flatTriangles.Length < 3)
                return null;

            float halfDepth = depth * 0.5f;
            int faceVertexCount = flatVertices.Length;

            var vertices = new List<Vector3>(
                faceVertexCount * 2 + (wallPoints.Length + holePoints.Length) * 4);
            var normals = new List<Vector3>(vertices.Capacity);
            var uv = new List<Vector2>(vertices.Capacity);
            var triangles = new List<int>(
                flatTriangles.Length * 2 +
                (wallPoints.Length + holePoints.Length) * 6);

            // Front points toward local -Z; back points toward local +Z.
            for (int side = 0; side < 2; side++)
            {
                float z = side == 0 ? -halfDepth : halfDepth;
                Vector3 normal = side == 0 ? Vector3.back : Vector3.forward;

                for (int i = 0; i < faceVertexCount; i++)
                {
                    Vector3 p = flatVertices[i];
                    vertices.Add(new Vector3(p.x, p.y, z));
                    normals.Add(normal);
                    uv.Add(new Vector2(
                        p.x / wallSize.x + 0.5f,
                        p.y / wallSize.y + 0.5f));
                }
            }

            for (int i = 0; i < flatTriangles.Length; i += 3)
            {
                int a = flatTriangles[i];
                int b = flatTriangles[i + 1];
                int c = flatTriangles[i + 2];

                Vector3 ab = flatVertices[b] - flatVertices[a];
                Vector3 ac = flatVertices[c] - flatVertices[a];
                float signedZ = Vector3.Cross(ab, ac).z;

                if (Mathf.Abs(signedZ) < 1e-10f)
                    continue;

                // Orient each triangle explicitly rather than assuming
                // the winding returned by Collider2D.CreateMesh().
                if (signedZ < 0f)
                {
                    AddTriangle(triangles, a, b, c);
                    AddTriangle(triangles,
                        a + faceVertexCount,
                        c + faceVertexCount,
                        b + faceVertexCount);
                }
                else
                {
                    AddTriangle(triangles, a, c, b);
                    AddTriangle(triangles,
                        a + faceVertexCount,
                        b + faceVertexCount,
                        c + faceVertexCount);
                }
            }

            // Both contours have the solid area on their left.
            // Therefore their right-hand normals point outside the solid,
            // including into the hole for the inner contour.
            AddSideLoop(wallPoints, halfDepth, vertices, normals, uv, triangles);
            AddSideLoop(holePoints, halfDepth, vertices, normals, uv, triangles);

            Mesh mesh = new Mesh();
            if (vertices.Count > 65535)
                mesh.indexFormat = IndexFormat.UInt32;

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddSideLoop(
            Vector2[] loop,
            float halfDepth,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uv,
            List<int> triangles)
        {
            float perimeter = 0f;
            for (int i = 0; i < loop.Length; i++)
                perimeter += Vector2.Distance(loop[i], loop[(i + 1) % loop.Length]);

            if (perimeter <= 0f)
                return;

            float travelled = 0f;
            for (int i = 0; i < loop.Length; i++)
            {
                Vector2 a = loop[i];
                Vector2 b = loop[(i + 1) % loop.Length];
                Vector2 edge = b - a;
                float length = edge.magnitude;
                if (length <= 1e-8f)
                    continue;

                Vector3 normal = new Vector3(edge.y, -edge.x, 0f).normalized;
                float u0 = travelled / perimeter;
                float u1 = (travelled + length) / perimeter;
                int first = vertices.Count;

                vertices.Add(new Vector3(a.x, a.y, -halfDepth));
                vertices.Add(new Vector3(b.x, b.y, -halfDepth));
                vertices.Add(new Vector3(b.x, b.y, halfDepth));
                vertices.Add(new Vector3(a.x, a.y, halfDepth));

                for (int j = 0; j < 4; j++)
                    normals.Add(normal);

                uv.Add(new Vector2(u0, 0f));
                uv.Add(new Vector2(u1, 0f));
                uv.Add(new Vector2(u1, 1f));
                uv.Add(new Vector2(u0, 1f));

                AddTriangle(triangles, first, first + 1, first + 2);
                AddTriangle(triangles, first, first + 2, first + 3);
                travelled += length;
            }
        }

        private static void AddTriangle(List<int> triangles, int a, int b, int c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        private void ConvertMeshToLocalSpace(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Transform meshTransform = meshFilter.transform;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = meshTransform.InverseTransformPoint(vertices[i]);
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
        }

        private static float SignedArea(Vector2[] points)
        {
            double area = 0.0;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[(i + 1) % points.Length];
                area += (double)a.x * b.y - (double)b.x * a.y;
            }
            return (float)(area * 0.5);
        }

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        public void Clear()
        {
            if (polygonCollider != null)
                polygonCollider.pathCount = 0;
            ClearMesh();
        }

        private void ClearMesh()
        {
            if (meshFilter != null && meshFilter.sharedMesh == generatedMesh)
                meshFilter.sharedMesh = null;
            DestroyOwnedMesh(generatedMesh);
            generatedMesh = null;
        }

        private static void DestroyOwnedMesh(Mesh mesh)
        {
            if (mesh == null)
                return;
            if (Application.isPlaying)
                Destroy(mesh);
            else
                DestroyImmediate(mesh);
        }

        private void SubscribeFunction()
        {
            if (function != null && autoUpdate)
                function.Changed += Generate;
        }

        private void UnsubscribeFunction()
        {
            if (function != null)
                function.Changed -= Generate;
        }
    }
}

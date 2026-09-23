using System.Collections.Generic;
using UnityEngine;
using Fs.Liquid2D;

namespace PolarGeometry
{
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(MeshFilter))]
    public class PolarFunctionPolygon : MonoBehaviour
    {
        [SerializeField]
        private PolarFunction function;

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
        private float deltaThetaDegrees = 1f;

        [SerializeField]
        private float maxCoordinateDelta = 0.01f;

        [SerializeField]
        private float maxDeltaThetaDegrees = 10f;

        [SerializeField]
        private int maxDepth = 16;

        [SerializeField]
        private float tolerance = 0.01f;

        [Header("Output")]
        [SerializeField]
        private bool enableCollider = true;

        [Header("Update")]
        [SerializeField]
        private bool autoUpdate = true;

        [Header("Silhouette")]
        [SerializeField]
        private bool useOuterSilhouette = false;

        [SerializeField]
        [Min(16)]
        private int silhouetteSampleCount = 360;

        private PolygonCollider2D polygonCollider;
        private Liquid2DPolygonCollider liquid2DPolygonCollider;
        private MeshFilter meshFilter;

        private Mesh generatedMesh;

        private void Awake()
        {
            polygonCollider =
                GetComponent<PolygonCollider2D>();

            liquid2DPolygonCollider =
                GetComponent<Liquid2DPolygonCollider>();

            meshFilter =
                GetComponent<MeshFilter>();

            if (function == null)
            {
                function =
                    GetComponent<PolarFunction>();
            }
        }

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            if (!TrySamplePositions(
                out List<Vector2> positions))
            {
                Clear();
                return;
            }

            polygonCollider.pathCount = 1;

            polygonCollider.SetPath(
                0,
                positions
            );

            // if (liquid2DPolygonCollider != null)
            // {
            //     liquid2DPolygonCollider.SetPoints(positions);
            // }

            GenerateMesh();

            polygonCollider.enabled =
                enableCollider;
        }

        public void Clear()
        {
            if (polygonCollider != null)
            {
                polygonCollider.pathCount = 0;
                polygonCollider.enabled =
                    enableCollider;
            }

            // if (liquid2DPolygonCollider != null)
            // {
            //     liquid2DPolygonCollider.ClearPoints();
            // }

            ClearMesh();
        }

        private bool TrySamplePositions(
            out List<Vector2> positions)
        {
            positions = null;

            if (function == null)
            {
                Debug.LogError(
                    "PolarFunction is not assigned.",
                    this
                );

                return false;
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

                    return false;
                }

                endTheta =
                    startTheta + thetaSpan;
            }
            else
            {
                endTheta =
                    endThetaDegrees * Mathf.Deg2Rad;
            }


            // Generate outer silhouette when ThetaSpan
            // exceeds one revolution.
            if (
                useOuterSilhouette &&
                useFunctionThetaSpan &&
                function.ThetaSpan is float span &&
                span > 2f * Mathf.PI + 0.0001f
            )
            {
                bool success =
                    PolarFunctionOuterSilhouette.TrySample(
                        function,
                        silhouetteSampleCount,
                        out positions,
                        startTheta
                    );

                return
                    success &&
                    positions != null &&
                    positions.Count >= 3;
            }

            bool includeEnd =
                !useFunctionThetaSpan;

            if (useAdaptiveSampling)
            {
                float maxDeltaTheta =
                    maxDeltaThetaDegrees * Mathf.Deg2Rad;

                if (function is Superformula superformula)
                {
                    positions =
                        superformula.SamplePositionsAdaptiveWithCriticalAngles(
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
                    positions =
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
            }
            else
            {
                float deltaTheta =
                    deltaThetaDegrees * Mathf.Deg2Rad;

                positions =
                    function.SamplePositions(
                        startTheta,
                        endTheta,
                        deltaTheta,
                        includeEnd,
                        tolerance
                    );
            }

            if (positions.Count < 3)
            {
                Debug.LogError(
                    "At least three positions are required to create a polygon.",
                    this
                );

                return false;
            }

            return true;
        }

        private void GenerateMesh()
        {
            ClearMesh();

            generatedMesh =
                polygonCollider.CreateMesh(
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
                ConvertMeshToLocalSpace(
                    generatedMesh
                );
            }

            generatedMesh.name =
                $"{name} Polar Function Polygon Mesh";

            meshFilter.sharedMesh =
                generatedMesh;
        }

        private void ConvertMeshToLocalSpace(
            Mesh mesh)
        {
            Vector3[] vertices =
                mesh.vertices;

            Transform meshTransform =
                meshFilter.transform;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] =
                    meshTransform.InverseTransformPoint(
                        vertices[i]
                    );
            }

            mesh.vertices =
                vertices;

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
                return;

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

        private void OnDestroy()
        {
            ClearMesh();
        }

        private void OnEnable()
        {
            SubscribeFunction();
        }

        private void OnDisable()
        {
            UnsubscribeFunction();
        }

        private void SubscribeFunction()
        {
            if (function != null && autoUpdate)
            {
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
    }
}

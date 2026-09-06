using System.Collections.Generic;
using UnityEngine;

namespace PolarGeometry
{
    public class PolarFunctionSpawner : MonoBehaviour
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

        [Header("Spawn")]
        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private Transform spawnParent;

        [SerializeField]
        [Min(0.0001f)]
        private float deltaThetaDegrees = 10f;

        [SerializeField]
        private bool alignToPath = false;

        private readonly List<GameObject> spawnedObjects =
            new List<GameObject>();

        private void Awake()
        {
            if (function == null)
            {
                function =
                    GetComponent<PolarFunction>();
            }
        }

        private void Start()
        {
            Spawn();
        }

        public void Spawn()
        {
            Clear();

            if (function == null)
            {
                Debug.LogError(
                    "PolarFunction is not assigned.",
                    this
                );

                return;
            }

            if (prefab == null)
            {
                Debug.LogError(
                    "Prefab is not assigned.",
                    this
                );

                return;
            }

            float deltaTheta =
                deltaThetaDegrees * Mathf.Deg2Rad;

            if (deltaTheta <= 0f)
            {
                Debug.LogError(
                    "Delta Theta must be greater than zero.",
                    this
                );

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

            float range =
                endTheta - startTheta;

            if (range == 0f)
            {
                SpawnAtTheta(
                    startTheta,
                    startTheta,
                    endTheta,
                    deltaTheta
                );

                return;
            }

            float direction =
                Mathf.Sign(range);

            float rangeLength =
                Mathf.Abs(range);

            int count =
                Mathf.CeilToInt(
                    rangeLength / deltaTheta
                );

            for (int i = 0; i < count; i++)
            {
                float theta =
                    startTheta +
                    direction *
                    deltaTheta *
                    i;

                SpawnAtTheta(
                    theta,
                    startTheta,
                    endTheta,
                    deltaTheta
                );
            }
        }

        public void Clear()
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i] != null)
                {
                    Destroy(
                        spawnedObjects[i]
                    );
                }
            }

            spawnedObjects.Clear();
        }

        private void SpawnAtTheta(
            float theta,
            float startTheta,
            float endTheta,
            float deltaTheta)
        {
            Vector2 position =
                EvaluatePosition(theta);

            Transform parent =
                spawnParent != null
                    ? spawnParent
                    : transform;

            GameObject instance =
                Instantiate(
                    prefab,
                    parent,
                    false
                );

            instance.transform.localPosition =
                new Vector3(
                    position.x,
                    position.y,
                    0f
                );

            if (alignToPath)
            {
                Vector2 tangent =
                    EvaluateTangent(
                        theta,
                        startTheta,
                        endTheta,
                        deltaTheta
                    );

                if (tangent.sqrMagnitude > 0f)
                {
                    float angle =
                        Mathf.Atan2(
                            tangent.y,
                            tangent.x
                        ) * Mathf.Rad2Deg;

                    instance.transform.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            angle
                        );
                }
            }

            spawnedObjects.Add(instance);
        }

        private Vector2 EvaluatePosition(
            float theta)
        {
            float r =
                function.Evaluate(theta);

            return new PolarCoordinate(
                r,
                theta
            ).ToCartesian();
        }

        private Vector2 EvaluateTangent(
            float theta,
            float startTheta,
            float endTheta,
            float deltaTheta)
        {
            float direction =
                Mathf.Sign(
                    endTheta - startTheta
                );

            if (direction == 0f)
                return Vector2.zero;

            float tangentDeltaTheta =
                Mathf.Min(
                    deltaTheta * 0.5f,
                    0.001f
                );

            float minTheta =
                Mathf.Min(
                    startTheta,
                    endTheta
                );

            float maxTheta =
                Mathf.Max(
                    startTheta,
                    endTheta
                );

            float thetaBefore =
                Mathf.Clamp(
                    theta -
                    direction *
                    tangentDeltaTheta,
                    minTheta,
                    maxTheta
                );

            float thetaAfter =
                Mathf.Clamp(
                    theta +
                    direction *
                    tangentDeltaTheta,
                    minTheta,
                    maxTheta
                );

            Vector2 before =
                EvaluatePosition(thetaBefore);

            Vector2 after =
                EvaluatePosition(thetaAfter);

            return after - before;
        }
    }
}
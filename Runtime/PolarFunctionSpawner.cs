using System.Collections.Generic;
using UnityEngine;

namespace PolarGeometry
{
    public class PolarFunctionSpawner : MonoBehaviour
    {
        public enum SpawnMode
        {
            Immediate,
            Progressive
        }

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
        private SpawnMode spawnMode =
            SpawnMode.Immediate;

        [SerializeField]
        [Min(0.0001f)]
        private float deltaThetaDegrees = 10f;

        [SerializeField]
        private bool alignToPath = false;

        [Header("Progressive")]
        [SerializeField]
        [Min(0f)]
        private float angularSpeedDegreesPerSecond = 90f;

        [Header("Update")]
        [SerializeField]
        private bool autoUpdate = false;

        private readonly List<GameObject> spawnedObjects =
            new List<GameObject>();

        private float startTheta;
        private float endTheta;
        private float thetaRange;
        private float thetaDirection;
        private float deltaTheta;

        private float thetaProgress;
        private int nextSpawnIndex;
        private int spawnCount;

        private bool spawning;

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

        private void Update()
        {
            if (!spawning)
                return;

            UpdateProgressiveSpawn(
                Time.deltaTime
            );
        }

        public void Spawn()
        {
            Clear();

            if (!InitializeSpawn())
                return;

            if (spawnMode == SpawnMode.Immediate)
            {
                SpawnImmediate();
            }
            else
            {
                StartProgressiveSpawn();
            }
        }

        public void Clear()
        {
            spawning = false;

            for (int i = 0;
                i < spawnedObjects.Count;
                i++)
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

        private bool InitializeSpawn()
        {
            if (function == null)
            {
                Debug.LogError(
                    "PolarFunction is not assigned.",
                    this
                );

                return false;
            }

            if (prefab == null)
            {
                Debug.LogError(
                    "Prefab is not assigned.",
                    this
                );

                return false;
            }

            deltaTheta =
                deltaThetaDegrees * Mathf.Deg2Rad;

            if (deltaTheta <= 0f)
            {
                Debug.LogError(
                    "Delta Theta must be greater than zero.",
                    this
                );

                return false;
            }

            startTheta =
                startThetaDegrees * Mathf.Deg2Rad;

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

            float range =
                endTheta - startTheta;

            thetaRange =
                Mathf.Abs(range);

            thetaDirection =
                Mathf.Sign(range);

            if (thetaRange == 0f)
            {
                spawnCount = 1;
            }
            else
            {
                spawnCount =
                    Mathf.CeilToInt(
                        thetaRange /
                        deltaTheta
                    );
            }

            return true;
        }

        private void SpawnImmediate()
        {
            for (int i = 0;
                i < spawnCount;
                i++)
            {
                float theta =
                    GetThetaAtIndex(i);

                SpawnAtTheta(theta);
            }
        }

        private void StartProgressiveSpawn()
        {
            thetaProgress = 0f;
            nextSpawnIndex = 0;

            if (spawnCount <= 0)
                return;

            // 始点は開始直後に生成する。
            SpawnAtTheta(
                GetThetaAtIndex(0)
            );

            nextSpawnIndex = 1;

            spawning =
                nextSpawnIndex < spawnCount;
        }

        private void UpdateProgressiveSpawn(
            float deltaTime)
        {
            float angularSpeed =
                angularSpeedDegreesPerSecond *
                Mathf.Deg2Rad;

            if (angularSpeed <= 0f)
                return;

            thetaProgress +=
                angularSpeed * deltaTime;

            thetaProgress =
                Mathf.Min(
                    thetaProgress,
                    thetaRange
                );

            // 1フレームで複数の生成地点を
            // 通過する可能性があるのでwhile。
            while (
                nextSpawnIndex < spawnCount &&
                nextSpawnIndex * deltaTheta
                    <= thetaProgress
            )
            {
                SpawnAtTheta(
                    GetThetaAtIndex(
                        nextSpawnIndex
                    )
                );

                nextSpawnIndex++;
            }

            if (nextSpawnIndex >= spawnCount)
            {
                spawning = false;
            }
        }

        private float GetThetaAtIndex(
            int index)
        {
            if (thetaRange == 0f)
                return startTheta;

            return startTheta +
                thetaDirection *
                deltaTheta *
                index;
        }

        private void SpawnAtTheta(
            float theta)
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
                    EvaluateTangent(theta);

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
            float theta)
        {
            if (thetaDirection == 0f)
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
                    thetaDirection *
                    tangentDeltaTheta,
                    minTheta,
                    maxTheta
                );

            float thetaAfter =
                Mathf.Clamp(
                    theta +
                    thetaDirection *
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
            if (function != null &&
                autoUpdate)
            {
                function.Changed +=
                    OnFunctionChanged;
            }
        }

        private void UnsubscribeFunction()
        {
            if (function != null)
            {
                function.Changed -=
                    OnFunctionChanged;
            }
        }

        private void OnFunctionChanged()
        {
            Spawn();
        }
    }
}

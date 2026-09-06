using System.Collections.Generic;
using UnityEngine;

namespace PolarGeometry
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PolarFunctionThetaRigidbody2DMover
        : MonoBehaviour
    {
        public enum MovementMode
        {
            Continuous,
            Discrete
        }

        [SerializeField]
        private PolarFunction function;

        [SerializeField]
        private MovementMode movementMode =
            MovementMode.Continuous;

        [Header("Range")]
        [SerializeField]
        private bool useFunctionThetaSpan = false;

        [SerializeField]
        private float startThetaDegrees = 0f;

        [SerializeField]
        private float endThetaDegrees = 360f;

        [Header("Movement")]
        [SerializeField]
        [Min(0f)]
        private float angularSpeedDegreesPerSecond = 90f;

        [SerializeField]
        private bool loop = false;

        [Header("Discrete")]
        [SerializeField]
        [Min(0.0001f)]
        private float deltaThetaDegrees = 1f;

        private readonly List<Vector2> sampledPositions =
            new List<Vector2>();

        private Rigidbody2D rb;

        private float startTheta;
        private float endTheta;
        private float thetaRange;
        private float thetaDirection;
        private float thetaProgress;
        private float deltaTheta;

        private bool initialized;

        private void Awake()
        {
            rb =
                GetComponent<Rigidbody2D>();

            if (function == null)
            {
                function =
                    GetComponent<PolarFunction>();
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            if (!initialized)
                return;

            AdvanceTheta(Time.fixedDeltaTime);
            ApplyCurrentPosition();
        }

        public void Initialize()
        {
            initialized = false;

            if (function == null)
            {
                Debug.LogError(
                    "PolarFunction is not assigned.",
                    this
                );

                return;
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

            thetaRange =
                Mathf.Abs(range);

            thetaDirection =
                Mathf.Sign(range);

            thetaProgress = 0f;

            if (movementMode == MovementMode.Discrete)
            {
                if (!InitializeDiscretePositions())
                    return;
            }

            initialized = true;

            ApplyCurrentPosition();
        }

        private bool InitializeDiscretePositions()
        {
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

            bool includeEnd =
                !(useFunctionThetaSpan && loop);

            sampledPositions.Clear();

            sampledPositions.AddRange(
                function.SamplePositions(
                    startTheta,
                    endTheta,
                    deltaTheta,
                    includeEnd
                )
            );

            return sampledPositions.Count > 0;
        }

        private void AdvanceTheta(float deltaTime)
        {
            if (thetaRange <= 0f)
                return;

            float angularSpeed =
                angularSpeedDegreesPerSecond *
                Mathf.Deg2Rad;

            thetaProgress +=
                angularSpeed * deltaTime;

            if (loop)
            {
                thetaProgress =
                    Mathf.Repeat(
                        thetaProgress,
                        thetaRange
                    );
            }
            else
            {
                thetaProgress =
                    Mathf.Clamp(
                        thetaProgress,
                        0f,
                        thetaRange
                    );
            }
        }

        private void ApplyCurrentPosition()
        {
            Vector2 localPosition =
                movementMode == MovementMode.Continuous
                    ? GetContinuousPosition()
                    : GetDiscretePosition();

            Vector2 worldPosition =
                LocalToWorldPosition(localPosition);

            rb.MovePosition(
                worldPosition
            );
        }

        private Vector2 GetContinuousPosition()
        {
            float theta =
                GetCurrentTheta();

            float r =
                function.Evaluate(theta);

            return new PolarCoordinate(
                r,
                theta
            ).ToCartesian();
        }

        private Vector2 GetDiscretePosition()
        {
            if (sampledPositions.Count == 0)
                return Vector2.zero;

            if (!loop &&
                thetaProgress >= thetaRange)
            {
                return sampledPositions[
                    sampledPositions.Count - 1
                ];
            }

            int index =
                Mathf.FloorToInt(
                    thetaProgress /
                    deltaTheta
                );

            index =
                Mathf.Clamp(
                    index,
                    0,
                    sampledPositions.Count - 1
                );

            return sampledPositions[index];
        }

        private float GetCurrentTheta()
        {
            return startTheta +
                thetaDirection *
                thetaProgress;
        }

        private Vector2 LocalToWorldPosition(
            Vector2 localPosition)
        {
            if (transform.parent == null)
                return localPosition;

            Vector3 worldPosition =
                transform.parent.TransformPoint(
                    new Vector3(
                        localPosition.x,
                        localPosition.y,
                        transform.localPosition.z
                    )
                );

            return new Vector2(
                worldPosition.x,
                worldPosition.y
            );
        }
    }
}
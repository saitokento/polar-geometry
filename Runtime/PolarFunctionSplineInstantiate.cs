using UnityEngine;
using UnityEngine.Splines;

namespace PolarGeometry
{
    [RequireComponent(typeof(SplineInstantiate))]
    public class PolarFunctionSplineInstantiate : MonoBehaviour
    {
        [SerializeField]
        private PolarFunctionSpline splineSource;

        private SplineInstantiate splineInstantiate;

        private void Awake()
        {
            splineInstantiate =
                GetComponent<SplineInstantiate>();

            if (splineSource == null)
            {
                splineSource =
                    GetComponentInParent<PolarFunctionSpline>();
            }
        }

        private void Start()
        {
            Spawn();
        }

        public void Spawn()
        {
            if (splineSource == null)
            {
                Debug.LogError(
                    "PolarFunctionSpline is not assigned.",
                    this
                );

                return;
            }

            splineSource.Generate();

            SplineContainer splineContainer =
                splineSource.GetComponent<SplineContainer>();

            if (splineContainer == null ||
                splineContainer.Spline == null ||
                splineContainer.Spline.Count < 2)
            {
                Debug.LogError(
                    "A valid spline could not be generated.",
                    this
                );

                return;
            }

            splineInstantiate.Container =
                splineContainer;

            splineInstantiate.SetDirty();
            splineInstantiate.UpdateInstances();
        }

        public void Clear()
        {
            splineInstantiate.Clear();
        }
    }
}
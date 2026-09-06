using UnityEngine;
using UnityEngine.Splines;

namespace PolarGeometry
{
    [RequireComponent(typeof(SplineAnimate))]
    public class PolarFunctionSplineAnimate : MonoBehaviour
    {
        [SerializeField]
        private PolarFunctionSpline splineSource;

        private SplineAnimate splineAnimate;

        private void Awake()
        {
            splineAnimate =
                GetComponent<SplineAnimate>();

            splineAnimate.enabled = false;

            if (splineSource == null)
            {
                splineSource =
                    GetComponentInParent<PolarFunctionSpline>();
            }
        }

        private void Start()
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

            splineAnimate.Container =
                splineContainer;

            splineAnimate.enabled = true;
        }
    }
}
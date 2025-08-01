using UnityEngine;

namespace Astrovisio
{
    public class DataRendererAxisController : MonoBehaviour
    {
        [SerializeField] private Canvas xAxeCanvas;
        [SerializeField] private Canvas yAxeCanvas;
        [SerializeField] private Canvas zAxeCanvas;
        [SerializeField] private Canvas refCanvas;

        private Camera cameraToLookAt;

        private void Start()
        {
            if (cameraToLookAt == null || !cameraToLookAt.isActiveAndEnabled)
            {
                cameraToLookAt = Camera.main;
                if (cameraToLookAt == null || !cameraToLookAt.isActiveAndEnabled)
                {
                    return;
                }
            }
        }

        private void Update()
        {
            if (cameraToLookAt == null)
            {
                return;
            }

            BillboardToCamera(xAxeCanvas);
            BillboardToCamera(yAxeCanvas);
            BillboardToCamera(zAxeCanvas);
            BillboardToCamera(refCanvas);
        }

        private void BillboardToCamera(Canvas canvas)
        {
            if (canvas == null)
                return;

            Transform t = canvas.transform;
            t.LookAt(
                t.position + cameraToLookAt.transform.rotation * Vector3.forward,
                cameraToLookAt.transform.rotation * Vector3.up
            );
        }

    }
    
}

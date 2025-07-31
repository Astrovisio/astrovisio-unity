using UnityEngine;

namespace Astrovisio
{

    public class Gizmo : MonoBehaviour
    {
        [SerializeField] private Canvas xPosCanvas;
        [SerializeField] private Canvas yPosCanvas;
        [SerializeField] private Canvas zPosCanvas;
        [SerializeField] private Canvas xNegCanvas;
        [SerializeField] private Canvas yNegCanvas;
        [SerializeField] private Canvas zNegCanvas;

        [SerializeField] private Camera gizmoCamera;

        private void Update()
        {
            if (gizmoCamera == null)
            {
                return;
            }

            BillboardToCamera(xPosCanvas);
            BillboardToCamera(yPosCanvas);
            BillboardToCamera(zPosCanvas);
            BillboardToCamera(xNegCanvas);
            BillboardToCamera(yNegCanvas);
            BillboardToCamera(zNegCanvas);
        }

        private void BillboardToCamera(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            Transform t = canvas.transform;

            t.LookAt(
                t.position + gizmoCamera.transform.rotation * Vector3.forward,
                gizmoCamera.transform.rotation * Vector3.up
            );

        }

    }

}
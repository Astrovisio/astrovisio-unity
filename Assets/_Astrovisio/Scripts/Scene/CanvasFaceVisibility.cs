using UnityEngine;

namespace Astrovisio
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasFaceVisibility : MonoBehaviour
    {
        private Canvas canvas;
        [SerializeField] private Camera cam;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        private void Update()
        {
            if (cam == null)
            {
                return;
            }

            Vector3 toCam = cam.transform.position - canvas.transform.position;

            bool isBack = Vector3.Dot(canvas.transform.forward, toCam) > 0;

            if (isBack)
            {
                transform.Rotate(0f, 180f, 0f, Space.Self);
            }
        }

    }
    
}

using UnityEngine;

namespace Astrovisio
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private bool m_FlipForward = false;

        private Camera m_Camera;

        private void Awake()
        {
            m_Camera = Camera.main;
        }

        private void Update()
        {
            if (m_Camera == null)
            {
                UpdateCamera();
                if (m_Camera == null)
                    return;
            }

            Vector3 direction = transform.position - m_Camera.transform.position;

            if (m_FlipForward)
            {
                direction = -direction;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }

        private void UpdateCamera()
        {
            m_Camera = Camera.main;
        }

    }

}

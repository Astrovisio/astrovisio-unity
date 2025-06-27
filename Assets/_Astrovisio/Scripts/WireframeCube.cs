using UnityEngine;

namespace Astrovisio
{
    public class WireframeCube : MonoBehaviour
    {

        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float size = 1f;
        [SerializeField] private float lineWidthFactor = 0.02f;

        private void Start()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = 16;
            lineRenderer.loop = false;
            lineRenderer.useWorldSpace = false;
            
            float lineWidth = Mathf.Max(0.001f, size * lineWidthFactor);
            lineRenderer.widthMultiplier = lineWidth;

            Vector3[] corners = new Vector3[]
            {
                // Base
                new Vector3(-1, -1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, -1, 1),
                new Vector3(-1, -1, 1),
                new Vector3(-1, -1, -1),

                // Vertical lines
                new Vector3(-1, 1, -1),
                new Vector3(1, 1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, 1, -1),
                new Vector3(1, 1, 1),
                new Vector3(1, -1, 1),
                new Vector3(1, 1, 1),
                new Vector3(-1, 1, 1),
                new Vector3(-1, -1, 1),
                new Vector3(-1, 1, 1),
                new Vector3(-1, 1, -1)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] *= size * 0.5f;
            }

            lineRenderer.positionCount = corners.Length;
            lineRenderer.SetPositions(corners);

        }

    }

}

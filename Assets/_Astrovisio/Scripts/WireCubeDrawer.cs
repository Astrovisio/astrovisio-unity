using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WireCubeDrawer : MonoBehaviour
{
    [SerializeField] private Vector3 min;
    [SerializeField] private Vector3 max;
    private Material lineMaterial;

    private void OnRenderObject()
    {
        if (!lineMaterial)
        {
            Shader shader = Shader.Find("Unlit/Color");
            lineMaterial = new Material(shader);
            lineMaterial.color = Color.green;
        }

        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);

        Vector3[] p = new Vector3[8];
        p[0] = new Vector3(min.x, min.y, min.z);
        p[1] = new Vector3(max.x, min.y, min.z);
        p[2] = new Vector3(max.x, min.y, max.z);
        p[3] = new Vector3(min.x, min.y, max.z);
        p[4] = new Vector3(min.x, max.y, min.z);
        p[5] = new Vector3(max.x, max.y, min.z);
        p[6] = new Vector3(max.x, max.y, max.z);
        p[7] = new Vector3(min.x, max.y, max.z);

        void DrawEdge(int a, int b)
        {
            GL.Vertex(p[a]);
            GL.Vertex(p[b]);
        }

        // Bottom face
        DrawEdge(0, 1);
        DrawEdge(1, 2);
        DrawEdge(2, 3);
        DrawEdge(3, 0);

        // Top face
        DrawEdge(4, 5);
        DrawEdge(5, 6);
        DrawEdge(6, 7);
        DrawEdge(7, 4);

        // Sides
        DrawEdge(0, 4);
        DrawEdge(1, 5);
        DrawEdge(2, 6);
        DrawEdge(3, 7);

        GL.End();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3[] p = new Vector3[8];
        p[0] = new Vector3(min.x, min.y, min.z);
        p[1] = new Vector3(max.x, min.y, min.z);
        p[2] = new Vector3(max.x, min.y, max.z);
        p[3] = new Vector3(min.x, min.y, max.z);
        p[4] = new Vector3(min.x, max.y, min.z);
        p[5] = new Vector3(max.x, max.y, min.z);
        p[6] = new Vector3(max.x, max.y, max.z);
        p[7] = new Vector3(min.x, max.y, max.z);

        void DrawEdge(int a, int b)
        {
            Gizmos.DrawLine(transform.TransformPoint(p[a]), transform.TransformPoint(p[b]));
        }

        // BOTTOM
        DrawEdge(0, 1);
        DrawEdge(1, 2);
        DrawEdge(2, 3);
        DrawEdge(3, 0);

        // TOP
        DrawEdge(4, 5);
        DrawEdge(5, 6);
        DrawEdge(6, 7);
        DrawEdge(7, 4);
        
        // VERTICALS
        DrawEdge(0, 4);
        DrawEdge(1, 5);
        DrawEdge(2, 6);
        DrawEdge(3, 7);
    }


}
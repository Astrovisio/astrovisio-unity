using UnityEngine;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;

public class KDTreeComponent : MonoBehaviour
{
    [Header("Transforms")]
    public Transform pointCloudTransform;
    public Transform controllerTransform;

    [Header("Mapping Ranges")]
    public Vector2 xRange = new Vector2(-10f, 10f);
    public Vector2 yRange = new Vector2(-10f, 10f);
    public Vector2 zRange = new Vector2(-10f, 10f);
    public Vector2 normalizedRange = new Vector2(-1f, 1f);


    private float gizmoRadius = 0.02f;
    private KDTreeManager manager;
    private float[][] data;
    private (int, float)? nearest;
    private bool running = false;

    [Header("Debug Sphere (in game)")]
    public Color sphereColor = Color.red;
    public float sphereRadius = 0.05f;
    private GameObject debugSphere;

    public async void Initialize(float[][] pointData, Vector3 pivot = new Vector3())
    {
        data = pointData;
        _ = await Task.Run(() => manager = new KDTreeManager(data, pivot));

        if (!Application.isPlaying)
        {
            return;
        }

        // Crea la sfera di debug se non esiste
        if (debugSphere == null)
        {
            debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.name = "DebugNearestPointSphere";
            debugSphere.transform.localScale = Vector3.one * sphereRadius * 2f; // diametro = raggio*2
            var rend = debugSphere.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.color = sphereColor;

            // Rimuovo collider per evitare problemi fisici se non serve
            Destroy(debugSphere.GetComponent<Collider>());
        }
    }

    private async void Update()
    {
        if (manager == null || running || controllerTransform == null || pointCloudTransform == null)
        {
            return;
        }

        running = true;
        Vector3 controllerWorld = controllerTransform.position;
        Vector3 controllerLocal = pointCloudTransform.InverseTransformPoint(controllerWorld);

        float x = RemapInverse(controllerLocal.x, normalizedRange, xRange);
        float y = RemapInverse(controllerLocal.y, normalizedRange, yRange);
        float z = RemapInverse(controllerLocal.z, normalizedRange, zRange);
        Vector3 queryPoint = new Vector3(x, y, z);

        nearest = await Task.Run(() => manager.FindNearest(queryPoint));

        var lastNearestIndex = nearest.Value.Item1;
        // Sposta la sfera debug nel mondo
        if (debugSphere != null && lastNearestIndex >= 0)
        {
            Vector3 pointOriginal = new Vector3(
                data[0][lastNearestIndex],
                data[1][lastNearestIndex],
                data[2][lastNearestIndex]
            );

            pointOriginal.x = RemapInverse(pointOriginal.x, xRange, normalizedRange);
            pointOriginal.y = RemapInverse(pointOriginal.y, yRange, normalizedRange);
            pointOriginal.z = RemapInverse(pointOriginal.z, zRange, normalizedRange);

            Vector3 worldPos = pointCloudTransform.TransformPoint(pointOriginal);
            debugSphere.transform.position = worldPos;
        }

        running = false;
    }

    float RemapInverse(float val, Vector2 from, Vector2 to)
    {
        float t = Mathf.InverseLerp(from.x, from.y, val);
        return Mathf.Lerp(to.x, to.y, t);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || nearest == null) return;
        var lastNearestIndex = nearest.Value.Item1;

        // Ottieni la posizione originale del punto nel sistema dati
        Vector3 pointOriginal = new Vector3(
            data[0][lastNearestIndex],
            data[1][lastNearestIndex],
            data[2][lastNearestIndex]
        );

        pointOriginal.x = RemapInverse(pointOriginal.x, xRange, normalizedRange);
        pointOriginal.y = RemapInverse(pointOriginal.y, yRange, normalizedRange);
        pointOriginal.z = RemapInverse(pointOriginal.z, zRange, normalizedRange);

        // Trasformalo nello spazio locale del pointCloudTransform, poi in mondo
        Vector3 worldPos = pointCloudTransform.TransformPoint(pointOriginal);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldPos, gizmoRadius);
    }
}
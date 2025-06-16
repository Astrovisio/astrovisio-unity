using UnityEngine;
using System.Threading.Tasks;

public class KDTreeComponent : MonoBehaviour
{
    [Header("Settings")]
    public bool realtime = true;

    [Header("Transforms")]
    public Transform pointCloudTransform;
    public Transform controllerTransform;

    [Header("Mapping Ranges")]
    public Vector2 xRange = new Vector2(-10f, 10f);
    public Vector2 yRange = new Vector2(-10f, 10f);
    public Vector2 zRange = new Vector2(-10f, 10f);
    public Vector2 xTargetRange = new Vector2(-10f, 10f);
    public Vector2 yTargetRange = new Vector2(-10f, 10f);
    public Vector2 zTargetRange = new Vector2(-10f, 10f);


    private KDTreeManager manager;
    private float[][] data;
    private (int, float)? nearest; // (Index, SquaredDistance)
    private bool running = false;

    [Header("Debug Sphere (in game)")]
    public bool debugSphereEnabled = true;
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
            debugSphere.SetActive(false);
            debugSphere.name = "DebugNearestPointSphere";
            debugSphere.transform.localScale = Vector3.one * sphereRadius * 2f; // diametro = raggio*2
            var rend = debugSphere.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.color = sphereColor;

            // Rimuovo collider per evitare problemi fisici se non serve
            Destroy(debugSphere.GetComponent<Collider>());
        }
    }

    public (int, float)? getLastNearest()
    {
        return nearest;
    }

    public Vector3 getNearestWorldSpaceCoordinates(int? index)
    {
        int lastNearestIndex = index != null ? (int)index : nearest.Value.Item1;
        Vector3 pointOriginal = new Vector3(
            data[0][lastNearestIndex],
            data[1][lastNearestIndex],
            data[2][lastNearestIndex]
        );

        pointOriginal.x = RemapInverse(pointOriginal.x, xRange, xTargetRange);
        pointOriginal.y = RemapInverse(pointOriginal.y, yRange, yTargetRange);
        pointOriginal.z = RemapInverse(pointOriginal.z, zRange, zTargetRange);

        Vector3 worldPos = pointCloudTransform.TransformPoint(pointOriginal);
        return worldPos;
    }

    public async Task<(int, float)> ComputeNearestPoint(Vector3 point)
    {
        Vector3 controllerLocal = pointCloudTransform.InverseTransformPoint(point);

        float x = RemapInverse(controllerLocal.x, xTargetRange, xRange);
        float y = RemapInverse(controllerLocal.y, yTargetRange, yRange);
        float z = RemapInverse(controllerLocal.z, zTargetRange, zRange);
        Vector3 queryPoint = new Vector3(x, y, z);

        (int, float) nearest = await Task.Run(() => manager.FindNearest(queryPoint));

        return nearest;
    }

    [ContextMenu("ComputeNearestPoint")]
    public async Task<(int, float)?> ComputeNearestPoint()
    {
        nearest = await ComputeNearestPoint(controllerTransform.position);
        return nearest;
    }

    private async void Update()
    {
        if (manager == null || running || controllerTransform == null || pointCloudTransform == null)
        {
            return;
        }


        if (realtime)
        {
            running = true;
            nearest = await ComputeNearestPoint(controllerTransform.position);
            running = false;
        }

        // Sposta la sfera debug nel mondo
        if (nearest != null && debugSphereEnabled && debugSphere != null)
        {
            if (!debugSphere.activeSelf)
            {
                debugSphere.SetActive(true);
            }
            debugSphere.transform.position = getNearestWorldSpaceCoordinates(nearest.Value.Item1);
        }
        else if (!debugSphereEnabled)
        {
            if (debugSphere.activeSelf)
            {
                debugSphere.SetActive(false);
            }
        }


    }

    private float RemapInverse(float val, Vector2 from, Vector2 to)
    {
        float t = Mathf.InverseLerp(from.x, from.y, val);
        return Mathf.Lerp(to.x, to.y, t);
    }

}
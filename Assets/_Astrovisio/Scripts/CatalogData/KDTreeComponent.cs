using UnityEngine;
using System.Threading.Tasks;

public readonly struct PointDistance
{
    public readonly int index;
    public readonly float squaredDistance;

    public PointDistance(int index, float squaredDistance)
    {
        this.index = index;
        this.squaredDistance = squaredDistance;
    }
}

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
    private PointDistance? nearest;
    private bool running = false;

    [Header("Debug Sphere (in game)")]
    public bool debugSphereEnabled = true;
    public Material sphereMaterial;
    public float sphereRadius = 0.05f;
    private GameObject debugSphere;

    public async void Initialize(float[][] pointData, Vector3 pivot, int[] xyz = null)
    {
        data = pointData;
        xyz ??= new int[] { 0, 1, 2 };
        _ = await Task.Run(() => manager = new KDTreeManager(data, pivot, xyz));

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
            rend.material = sphereMaterial;
            // Rimuovo collider per evitare problemi fisici se non serve
            Destroy(debugSphere.GetComponent<Collider>());
        }
    }

    public PointDistance? GetLastNearest()
    {
        return nearest;
    }

    public float[] GetDataInfo(int index)
    {
        if (data == null || data.Length == 0)
        {
            return null;
        }

        int rowCount = data.Length;
        float[] result = new float[rowCount];

        for (int i = 0; i < rowCount; i++)
        {
            result[i] = data[i][index];
        }

        return result;
    }


    public Vector3 GetNearestWorldSpaceCoordinates(int? index)
    {
        int lastNearestIndex = index != null ? (int)index : nearest.Value.index;
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

    public async Task<PointDistance> ComputeNearestPoint(Vector3 point)
    {
        Vector3 controllerLocal = pointCloudTransform.InverseTransformPoint(point);

        float x = RemapInverse(controllerLocal.x, xTargetRange, xRange);
        float y = RemapInverse(controllerLocal.y, yTargetRange, yRange);
        float z = RemapInverse(controllerLocal.z, zTargetRange, zRange);
        Vector3 queryPoint = new Vector3(x, y, z);

        (int index, float distanceSquared) tuple = await Task.Run(() => manager.FindNearest(queryPoint));
        PointDistance nearest = new PointDistance(tuple.index, tuple.distanceSquared);

        return nearest;
    }

    [ContextMenu("ComputeNearestPoint")]
    public async Task<PointDistance?> ComputeNearestPoint()
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
            debugSphere.transform.position = GetNearestWorldSpaceCoordinates(nearest.Value.index);
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

    private void OnDestroy()
    {
        Destroy(debugSphere);
    }

}

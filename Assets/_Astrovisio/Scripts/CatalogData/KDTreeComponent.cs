using UnityEngine;
using System.Threading.Tasks;
using CatalogData;
using System;

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

    [Header("Mapping Scales")]
    public ScalingType XScale = ScalingType.Linear;
    public ScalingType YScale = ScalingType.Linear;
    public ScalingType ZScale = ScalingType.Linear;

    private int[] xyz = new int[] { 0, 1, 2 };

    private KDTreeManager manager;
    private float[][] data;
    private PointDistance? nearest;
    private bool running = false;

    [Header("SphereDataInspector")]
    [SerializeField] private SphereDataInspector sphereDataInspectorPrefab;
     private SphereDataInspector sphereDataInspector;


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

        HandleSphereDataInspector();
    }

    private void HandleSphereDataInspector()
    {
        // Debug.Log($"{nearest != null} - {sphereDataInspector}");
        if (nearest != null && sphereDataInspector != null)
        {
            sphereDataInspector.transform.position = GetNearestWorldSpaceCoordinates(nearest.Value.index);
        }
    }

    public async void Initialize(float[][] pointData, Vector3 pivot, int[] xyz)
    {
        data = pointData;
        this.xyz = xyz;
        _ = await Task.Run(() => manager = new KDTreeManager(data, pivot, this.xyz));

        if (!Application.isPlaying)
        {
            return;
        }

        if (sphereDataInspector == null)
        {
            sphereDataInspector = Instantiate(sphereDataInspectorPrefab);
            sphereDataInspector.SetActiveState(false);
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

    public void SetDataInspectorVisibility(bool state)
    {
        if (sphereDataInspector != null)
        {
            sphereDataInspector.SetActiveState(state);
        }
    }

    public bool GetDataInspectorVisibility()
    {
        if (sphereDataInspector != null)
        {
            MeshRenderer meshRenderer = sphereDataInspector.GetComponent<MeshRenderer>();
            return meshRenderer.enabled;
        }
        return false;
    }

    public bool ToggleDataInspectorVisibility()
    {
        if (sphereDataInspector != null)
        {
            MeshRenderer meshRenderer = sphereDataInspector.GetComponent<MeshRenderer>();
            bool currentState = meshRenderer.enabled;
            meshRenderer.enabled = !currentState;
            return meshRenderer.enabled;
            // Debug.Log($"Toggled object '{debugSphere.name}' to {(debugSphere.activeSelf ? "visible" : "hidden")}");
        }
        return false;
    }

    public Vector3 GetNearestWorldSpaceCoordinates(int? index)
    {
        if (pointCloudTransform == null)
        {
            return new Vector3();
        }

        int lastNearestIndex = index != null ? (int)index : nearest.Value.index;

        if (lastNearestIndex > 0)
        {
            Vector3 pointOriginal = new Vector3(
                data[xyz[0]][lastNearestIndex],
                data[xyz[1]][lastNearestIndex],
                data[xyz[2]][lastNearestIndex]
            );

            switch (XScale)
            {
                case ScalingType.Sqrt:
                    pointOriginal.x = signed_sqrt(pointOriginal.x, 1);
                    break;
                default:
                    break;
            }

            switch (YScale)
            {
                case ScalingType.Sqrt:
                    pointOriginal.y = signed_sqrt(pointOriginal.y, 1);
                    break;
                default:
                    break;
            }

            switch (ZScale)
            {
                case ScalingType.Sqrt:
                    pointOriginal.z = signed_sqrt(pointOriginal.z, 1);
                    break;
                default:
                    break;
            }

            pointOriginal.x = RemapInverse(pointOriginal.x, xRange, xTargetRange);
            pointOriginal.y = RemapInverse(pointOriginal.y, yRange, yTargetRange);
            pointOriginal.z = RemapInverse(pointOriginal.z, zRange, zTargetRange);

            Vector3 worldPos = pointCloudTransform.TransformPoint(pointOriginal);
            return worldPos;
        }

        return new Vector3();

    }

    public async Task<PointDistance> ComputeNearestPoint(Vector3 point)
    {
        Vector3 controllerLocal = pointCloudTransform.InverseTransformPoint(point);

        switch (XScale)
        {
            case ScalingType.Sqrt:
                controllerLocal.x = inverse_signed_sqrt(controllerLocal.x, 1);
                break;
            default:
                break;
        }

        switch (YScale)
        {
            case ScalingType.Sqrt:
                controllerLocal.y = inverse_signed_sqrt(controllerLocal.y, 1);
                break;
            default:
                break;
        }

        switch (ZScale)
        {
            case ScalingType.Sqrt:
                controllerLocal.z = inverse_signed_sqrt(controllerLocal.z, 1);
                break;
            default:
                break;
        }

        float x = RemapInverse(controllerLocal.x, xTargetRange, xRange);
        float y = RemapInverse(controllerLocal.y, yTargetRange, yRange);
        float z = RemapInverse(controllerLocal.z, zTargetRange, zRange);

        Vector3 queryPoint = new Vector3(x, y, z);

        (int index, float distanceSquared) tuple = await Task.Run(() => manager.FindNearest(queryPoint));
        PointDistance nearest = new PointDistance(tuple.index, tuple.distanceSquared);

        return nearest;
    }

    private float RemapInverse(float val, Vector2 from, Vector2 to)
    {
        float t = Mathf.InverseLerp(from.x, from.y, val);
        return Mathf.Lerp(to.x, to.y, t);
    }

    private float inverse_signed_sqrt(float y, float scale)
    {
        return Math.Sign(y) * scale * (y * y);
    }

    private float signed_sqrt(float x, float scale)
    {
        return Math.Sign(x) * (float)Math.Sqrt(Math.Abs(x) / scale);
    }

    private void OnDestroy()
    {
        DestroyImmediate(sphereDataInspector);
    }

}

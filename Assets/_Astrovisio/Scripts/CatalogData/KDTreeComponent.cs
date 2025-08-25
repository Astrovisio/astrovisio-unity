using UnityEngine;
using System.Threading.Tasks;
using CatalogData;
using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct PointDistance
{
    public readonly int index;
    public readonly float squaredDistance;

    public PointDistance(int index, float squaredDistance)
    {
        this.index = index;
        this.squaredDistance = squaredDistance;
    }

    public static bool operator ==(PointDistance a, PointDistance b)
    {
        return a.index == b.index && a.squaredDistance == b.squaredDistance;
    }

    public static bool operator !=(PointDistance a, PointDistance b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return obj is PointDistance other && this == other;
    }

    public override int GetHashCode()
    {
        return (index, squaredDistance).GetHashCode();
    }
}

public class KDTreeComponent : MonoBehaviour
{

    [Header("Settings")]
    public bool realtime = true;

    [Header("Area Selection Settings")]
    public AreaSelectionMode selectionMode = AreaSelectionMode.SinglePoint;
    public float selectionRadius = 0.05f; // For sphere selection
    public float selectionCubeHalfSize = 0.05f; // For cube selection
    public AggregationMode aggregationMode = AggregationMode.Average;
    public bool showSelectionGizmo = true;

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
    private AreaSelectionResult areaSelectionResult;
    private bool running = false;

    [Header("DataInspectors")]
    [SerializeField] private DataInspector pointDataInspectorPrefab;
    [SerializeField] private DataInspector areaSphereDataInspectorPrefab;
    [SerializeField] private DataInspector areaBoxDataInspectorPrefab;
    private DataInspector pointDataInspector;
    private DataInspector areaSphereDataInspector;
    private DataInspector areaBoxDataInspector;
    private DataInspector currentDataInspector;

    // Cached transform values for async operations
    private Vector3 cachedWorldScale;
    private bool needsScaleUpdate = true;

    [ContextMenu("ComputeNearestPoint")]
    public async Task<PointDistance?> ComputeNearestPoint()
    {
        nearest = await ComputeNearestPoint(controllerTransform.position);
        return nearest;
    }

    [ContextMenu("ComputeAreaSelection")]
    public async Task<AreaSelectionResult> ComputeAreaSelection()
    {
        areaSelectionResult = await ComputeAreaSelection(controllerTransform.position);
        return areaSelectionResult;
    }

    private async void Update()
    {
        if (manager == null || running || controllerTransform == null || pointCloudTransform == null)
        {
            return;
        }

        // Update cached scale values if needed
        if (needsScaleUpdate || transform.hasChanged)
        {
            cachedWorldScale = pointCloudTransform.lossyScale;
            needsScaleUpdate = false;
        }

        if (realtime)
        {
            running = true;

            if (selectionMode == AreaSelectionMode.SinglePoint)
            {
                nearest = await ComputeNearestPoint(controllerTransform.position);
            }
            else
            {
                areaSelectionResult = await ComputeAreaSelection(controllerTransform.position);
                // Debug.Log(TransformRadiusToDataSpace(selectionRadius) + ", " + areaSelectionResult.SelectedIndices.Count);
                // if (areaSelectionResult.Count > 0)
                // {
                //     Debug.Log(string.Join(", ", areaSelectionResult.AggregatedValues));
                // }

            }

            running = false;
        }

        HandleSphereDataInspector();
        UpdateSelectionVisualizer();
    }

    private void HandleSphereDataInspector()
    {
        if (selectionMode == AreaSelectionMode.SinglePoint)
        {
            if (nearest != null && pointDataInspector != null)
            {
                pointDataInspector.transform.position = GetNearestWorldSpaceCoordinates(nearest.Value.index);
            }
        }
        else
        {
            // For area selection, position at the center of mass of selected points
            if (areaSelectionResult != null && areaSelectionResult.Count > 0 && areaSphereDataInspector != null && areaBoxDataInspector != null)
            {
                areaSphereDataInspector.transform.position = GetAreaCenterWorldSpace();
                areaBoxDataInspector.transform.position = GetAreaCenterWorldSpace();
            }
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

        if (pointDataInspector == null)
        {
            pointDataInspector = Instantiate(pointDataInspectorPrefab);
            pointDataInspector.SetActiveState(false);
            currentDataInspector = pointDataInspector;
        }

        if (areaSphereDataInspector == null)
        {
            areaSphereDataInspector = Instantiate(areaSphereDataInspectorPrefab);
            areaSphereDataInspector.SetActiveState(false);
        }

        if (areaBoxDataInspector == null)
        {
            areaBoxDataInspector = Instantiate(areaBoxDataInspectorPrefab);
            areaBoxDataInspector.SetActiveState(false);
        }

    }

    private void UpdateSelectionVisualizer()
    {
        if (!showSelectionGizmo || currentDataInspector == null) return;

        currentDataInspector.SetActiveState(false);

        if (selectionMode == AreaSelectionMode.SinglePoint)
        {
            currentDataInspector = pointDataInspector;
            currentDataInspector.SetActiveState(true);
        }
        else
        {

            if (selectionMode == AreaSelectionMode.Sphere)
            {
                areaSphereDataInspector.transform.localScale = Vector3.one * (selectionRadius * 2);
                currentDataInspector = areaSphereDataInspector;
            }
            else if (selectionMode == AreaSelectionMode.Cube)
            {

                areaBoxDataInspector.transform.localScale = Vector3.one * (selectionCubeHalfSize * 2);
                currentDataInspector = areaBoxDataInspector;

            }

            currentDataInspector.SetActiveState(true);
            currentDataInspector.transform.position = controllerTransform.position;
        }
    }

    public PointDistance? GetLastNearest()
    {
        return nearest;
    }

    public AreaSelectionResult GetLastAreaSelection()
    {
        return areaSelectionResult;
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

    // NEW: Get aggregated data for area selection
    public float[] GetAreaSelectionDataInfo()
    {
        if (areaSelectionResult == null || areaSelectionResult.Count == 0)
        {
            return null;
        }

        return areaSelectionResult.AggregatedValues;
    }

    private float[] AggregateData(List<int> indices)
    {
        if (data == null || data.Length == 0 || indices.Count == 0)
        {
            return null;
        }

        int rowCount = data.Length;
        float[] result = new float[rowCount];

        for (int row = 0; row < rowCount; row++)
        {
            var values = indices.Select(idx => data[row][idx]).ToList();

            switch (aggregationMode)
            {
                case AggregationMode.Average:
                    result[row] = values.Average();
                    break;
                case AggregationMode.Sum:
                    result[row] = values.Sum();
                    break;
                case AggregationMode.Min:
                    result[row] = values.Min();
                    break;
                case AggregationMode.Max:
                    result[row] = values.Max();
                    break;
                case AggregationMode.Median:
                    values.Sort();
                    int mid = values.Count / 2;
                    result[row] = values.Count % 2 == 0 ?
                        (values[mid - 1] + values[mid]) / 2f : values[mid];
                    break;
            }
        }

        return result;
    }

    public void SetDataInspectorVisibility(bool state)
    {
        if (currentDataInspector != null)
        {
            currentDataInspector.SetActiveState(state);
        }
    }

    public bool GetDataInspectorVisibility()
    {
        if (currentDataInspector != null)
        {
            MeshRenderer meshRenderer = currentDataInspector.GetComponent<MeshRenderer>();
            return meshRenderer.enabled;
        }
        return false;
    }

    public bool ToggleDataInspectorVisibility()
    {
        if (currentDataInspector != null)
        {
            MeshRenderer meshRenderer = currentDataInspector.GetComponent<MeshRenderer>();
            bool currentState = meshRenderer.enabled;
            meshRenderer.enabled = !currentState;
            return meshRenderer.enabled;
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

        if (lastNearestIndex >= 0)
        {
            Vector3 pointOriginal = new Vector3(
                data[xyz[0]][lastNearestIndex],
                data[xyz[1]][lastNearestIndex],
                data[xyz[2]][lastNearestIndex]
            );

            pointOriginal = ApplyScaling(pointOriginal);
            pointOriginal = RemapPoint(pointOriginal);

            Vector3 worldPos = pointCloudTransform.TransformPoint(pointOriginal);
            return worldPos;
        }

        return new Vector3();
    }

    private Vector3 GetAreaCenterWorldSpace()
    {
        if (areaSelectionResult == null || areaSelectionResult.Count == 0 || pointCloudTransform == null)
        {
            return Vector3.zero;
        }

        // Calculate center of mass
        Vector3 centerOfMass = Vector3.zero;
        foreach (int idx in areaSelectionResult.SelectedIndices)
        {
            Vector3 point = new Vector3(
                data[xyz[0]][idx],
                data[xyz[1]][idx],
                data[xyz[2]][idx]
            );
            centerOfMass += point;
        }
        centerOfMass /= areaSelectionResult.Count;

        centerOfMass = ApplyScaling(centerOfMass);
        centerOfMass = RemapPoint(centerOfMass);

        return pointCloudTransform.TransformPoint(centerOfMass);
    }

    private Vector3 ApplyScaling(Vector3 point)
    {
        switch (XScale)
        {
            case ScalingType.Sqrt:
                point.x = signed_sqrt(point.x, 1);
                break;
        }

        switch (YScale)
        {
            case ScalingType.Sqrt:
                point.y = signed_sqrt(point.y, 1);
                break;
        }

        switch (ZScale)
        {
            case ScalingType.Sqrt:
                point.z = signed_sqrt(point.z, 1);
                break;
        }

        return point;
    }

    private Vector3 RemapPoint(Vector3 point)
    {
        point.x = RemapInverseUnclamped(point.x, xRange, xTargetRange);
        point.y = RemapInverseUnclamped(point.y, yRange, yTargetRange);
        point.z = RemapInverseUnclamped(point.z, zRange, zTargetRange);
        return point;
    }

    public async Task<PointDistance> ComputeNearestPoint(Vector3 point)
    {
        Vector3 queryPoint = TransformWorldToDataSpace(point);

        (int index, float distanceSquared) tuple = await Task.Run(() => manager.FindNearest(queryPoint));
        PointDistance nearest = new PointDistance(tuple.index, tuple.distanceSquared);

        return nearest;
    }

    public async Task<AreaSelectionResult> ComputeAreaSelection(Vector3 worldPoint)
    {
        Vector3 queryPoint = TransformWorldToDataSpace(worldPoint);

        Debug.Log(queryPoint);

        // Calculate data space radius/size before entering the async task
        float dataSpaceRadius = 0f;
        float dataSpaceHalfSize = 0f;

        switch (selectionMode)
        {
            case AreaSelectionMode.Sphere:
                dataSpaceRadius = TransformRadiusToDataSpace(selectionRadius);
                break;
            case AreaSelectionMode.Cube:
                dataSpaceHalfSize = TransformRadiusToDataSpace(selectionCubeHalfSize);
                break;
        }

        List<int> indices = null;

        // Copy values for use in async context
        var mode = selectionMode;
        var radius = dataSpaceRadius;
        var halfSize = dataSpaceHalfSize;

        await Task.Run(() =>
        {
            switch (mode)
            {
                case AreaSelectionMode.Sphere:
                    indices = manager.FindPointsInSphere(queryPoint, radius);
                    break;

                case AreaSelectionMode.Cube:
                    indices = manager.FindPointsInCube(queryPoint, halfSize);
                    break;

                default:
                    indices = new List<int>();
                    break;
            }
        });

        var result = new AreaSelectionResult
        {
            SelectedIndices = indices,
            CenterPoint = queryPoint,
            SelectionRadius = selectionMode == AreaSelectionMode.Sphere ? selectionRadius : selectionCubeHalfSize
        };

        if (indices.Count > 0)
        {
            result.AggregatedValues = AggregateData(indices);
        }

        return result;
    }

    private Vector3 TransformWorldToDataSpace(Vector3 worldPoint)
    {
        Vector3 controllerLocal = pointCloudTransform.InverseTransformPoint(worldPoint);

        switch (XScale)
        {
            case ScalingType.Sqrt:
                controllerLocal.x = inverse_signed_sqrt(controllerLocal.x, 1);
                break;
        }

        switch (YScale)
        {
            case ScalingType.Sqrt:
                controllerLocal.y = inverse_signed_sqrt(controllerLocal.y, 1);
                break;
        }

        switch (ZScale)
        {
            case ScalingType.Sqrt:
                controllerLocal.z = inverse_signed_sqrt(controllerLocal.z, 1);
                break;
        }

        float x = RemapInverseUnclamped(controllerLocal.x, xTargetRange, xRange);
        float y = RemapInverseUnclamped(controllerLocal.y, yTargetRange, yRange);
        float z = RemapInverseUnclamped(controllerLocal.z, zTargetRange, zRange);

        return new Vector3(x, y, z);
    }

    private float TransformRadiusToDataSpace(float worldRadius)
    {
        // Use cached scale or get it if on main thread
        Vector3 worldScale = cachedWorldScale;
        if (worldScale == Vector3.zero && pointCloudTransform != null)
        {
            worldScale = pointCloudTransform.lossyScale;
        }

        float avgScale = (worldScale.x + worldScale.y + worldScale.z) / 3f;
        float localRadius = worldRadius / avgScale;

        // Apply inverse remapping (assuming uniform scaling for simplicity)
        float xScale = (xRange.y - xRange.x) / (xTargetRange.y - xTargetRange.x);
        float yScale = (yRange.y - yRange.x) / (yTargetRange.y - yTargetRange.x);
        float zScale = (zRange.y - zRange.x) / (zTargetRange.y - zTargetRange.x);
        float avgDataScale = (xScale + yScale + zScale) / 3f;

        return localRadius * avgDataScale;
    }

    public float InverseLerpUnclamped(float a, float b, float value)
    {
        // Gestione del caso degenere dove a == b
        if (Mathf.Approximately(a, b))
        {
            return 0f; // Oppure potresti voler restituire float.NaN o gestire diversamente
        }

        return (value - a) / (b - a);
    }

    private float RemapInverse(float val, Vector2 from, Vector2 to)
    {
        float t = Mathf.InverseLerp(from.x, from.y, val);
        return Mathf.Lerp(to.x, to.y, t);
    }

    private float RemapInverseUnclamped(float val, Vector2 from, Vector2 to)
    {
        float t = InverseLerpUnclamped(from.x, from.y, val);
        return Mathf.LerpUnclamped(to.x, to.y, t);
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
        if (pointDataInspector != null)
        {
            DestroyImmediate(pointDataInspector);
        }
        if (areaSphereDataInspector != null)
        {
            DestroyImmediate(areaSphereDataInspector);
        }
        if (areaBoxDataInspector != null)
        {
            DestroyImmediate(areaBoxDataInspector);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controllerTransform == null) return;

        Gizmos.color = new Color(0, 1, 0, 0.3f);

        switch (selectionMode)
        {
            case AreaSelectionMode.Sphere:
                Gizmos.DrawWireSphere(controllerTransform.position, selectionRadius);
                break;

            case AreaSelectionMode.Cube:
                Gizmos.DrawWireCube(controllerTransform.position, Vector3.one * (selectionCubeHalfSize * 2));
                break;
        }
    }
}
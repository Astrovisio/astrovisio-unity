using UnityEngine;
using System.Threading.Tasks;
using CatalogData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

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

    // Events
    public event Action OnInitializationPerformed;
    public event Action<float[]> OnSelectionPerformed;

    [Header("Settings")]
    public bool realtime = false;

    [Header("Area Selection Settings")]
    public SelectionMode selectionMode = SelectionMode.Sphere;
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
    private SelectionResult areaSelectionResult;
    private bool running = false;
    private bool computingAreaSelection = false;

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

    [Header("Input Actions")]
    public InputActionProperty selectAction;

    private GameObject currentDataSelectionGameObject;
    private AstrovisioDataSetRenderer astrovisioDatasetRenderer;


    [ContextMenu("ComputeNearestPoint")]
    public async Task<PointDistance?> ComputeNearestPoint()
    {
        nearest = await ComputeNearestPoint(controllerTransform.position);
        return nearest;
    }

    [ContextMenu("ComputeAreaSelection")]
    public async Task<SelectionResult> ComputeAreaSelection()
    {
        areaSelectionResult = await ComputeAreaSelection(controllerTransform.position);
        return areaSelectionResult;
    }

    public bool IsComputingAreaSelection()
    {
        return computingAreaSelection;
    }

    private void OnEnable()
    {
        selectAction.action.Enable();
    }

    private void OnDisable()
    {
        selectAction.action.Disable();
    }

    private void Awake()
    {
        astrovisioDatasetRenderer = GetComponent<AstrovisioDataSetRenderer>();
    }

    private void Start()
    {
        selectAction.action.performed += OnSpacebarPressed;
    }

    private void OnSpacebarPressed(InputAction.CallbackContext context)
    {
        _ = PerformSelection();
    }

    private GameObject CloneAndAttach(GameObject original, Transform newParent)
    {

        if (currentDataSelectionGameObject != null)
        {
            Destroy(currentDataSelectionGameObject);
        }

        if (original == null)
        {
            Debug.LogError("Original GameObject is null");
            return null;
        }

        if (newParent == null)
        {
            Debug.LogError("New parent Transform is null");
            return null;
        }

        // Clona l'oggetto
        GameObject cloned = Instantiate(original);

        // Imposta il parent
        cloned.transform.SetParent(newParent, true);

        currentDataSelectionGameObject = cloned;

        return cloned;
    }

    private async Task<SelectionResult> ComputeSelection()
    {
        SelectionResult selectionResult = null;

        switch (selectionMode)
        {
            case SelectionMode.SinglePoint:
                nearest = await ComputeNearestPoint(controllerTransform.position);
                List<int> indices = new List<int>(new int[1] { nearest.Value.index });
                selectionResult = new SelectionResult
                {
                    SelectedIndices = indices,
                    CenterPoint = GetNearestWorldSpaceCoordinates(nearest.Value.index),
                    SelectionRadius = selectionMode == SelectionMode.Sphere ? selectionRadius : (selectionMode == SelectionMode.Cube ? selectionCubeHalfSize : 0),
                    AggregatedValues = AggregateData(indices),
                    SelectionMode = selectionMode
                };
                int[] visibilityArray = new int[data[0].Length];
                for (int i = 0; i < indices.Count; i++)
                {
                    visibilityArray[indices[i]] = 1;
                }
                selectionResult.SelectedArray = visibilityArray;
                break;
            case SelectionMode.Sphere:
            case SelectionMode.Cube:
                selectionResult = await ComputeAreaSelection(controllerTransform.position);
                break;
        }
        return selectionResult;
    }

    public async Task<SelectionResult> PerformSelection()
    {
        Vector3 positionAtAction = controllerTransform.position + Vector3.zero;
        // Quaternion rotationAtAction = controllerTransform.rotation * Quaternion.identity;
        areaSelectionResult = await ComputeSelection();

        // Update Visible Items Array in shader
        astrovisioDatasetRenderer.UpdateDataVisibility(areaSelectionResult.SelectedArray);

        GameObject cloned;
        switch (selectionMode)
        {
            case SelectionMode.SinglePoint:
                cloned = CloneAndAttach(pointDataInspector.gameObject, gameObject.transform);
                cloned.transform.position = areaSelectionResult.CenterPoint;
                //cloned.transform.rotation = rotationAtAction;
                break;
            case SelectionMode.Sphere:
                cloned = CloneAndAttach(areaSphereDataInspector.gameObject, gameObject.transform);
                cloned.transform.position = positionAtAction + Vector3.zero;
                //cloned.transform.rotation = rotationAtAction;
                break;
            case SelectionMode.Cube:
                cloned = CloneAndAttach(areaBoxDataInspector.gameObject, gameObject.transform);
                cloned.transform.position = positionAtAction + Vector3.zero;
                //cloned.transform.rotation = rotationAtAction;
                break;
        }

        if (areaSelectionResult.SelectedIndices.Count > 0)
        {
            Debug.Log(string.Join(", ", areaSelectionResult.AggregatedValues));
        }

        OnSelectionPerformed?.Invoke(areaSelectionResult.AggregatedValues);

        return areaSelectionResult;
    }

    private void Update()
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

        HandleDataInspectorPosition();
        UpdateSelectionVisualizer();
    }

    public void setControllerTransform(Transform controllerTransform)
    {
        this.controllerTransform = controllerTransform;
        areaSphereDataInspector.transform.position = this.controllerTransform.transform.position;
        areaSphereDataInspector.transform.rotation = this.controllerTransform.transform.rotation;
        areaBoxDataInspector.transform.position = this.controllerTransform.transform.position;
        areaBoxDataInspector.transform.rotation = this.controllerTransform.transform.rotation;
    }

    private void HandleDataInspectorPosition()
    {
        switch (selectionMode)
        {
            case SelectionMode.SinglePoint:
                if (pointDataInspector != null)
                {
                    pointDataInspector.transform.position = controllerTransform.transform.position;
                    pointDataInspector.transform.rotation = controllerTransform.transform.rotation;
                }
                break;
            case SelectionMode.Sphere:
                if (areaSphereDataInspector != null)
                {
                    areaSphereDataInspector.transform.position = controllerTransform.transform.position;
                    areaSphereDataInspector.transform.rotation = controllerTransform.transform.rotation;
                }
                break;
            case SelectionMode.Cube:
                if (areaBoxDataInspector != null)
                {
                    areaBoxDataInspector.transform.position = controllerTransform.transform.position;
                    areaBoxDataInspector.transform.rotation = controllerTransform.transform.rotation;
                }
                break;
        }
    }

    public async Task Initialize(float[][] pointData, Vector3 pivot, int[] xyz)
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

        OnInitializationPerformed?.Invoke();
    }

    private void UpdateSelectionVisualizer()
    {

        if (currentDataInspector)
        {
            currentDataInspector.SetActiveState(false);
            currentDataInspector = null;
        }

        if (currentDataSelectionGameObject != null)
        {
            currentDataSelectionGameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        if (!showSelectionGizmo) return;

        if (selectionMode == SelectionMode.SinglePoint && pointDataInspector != null)
        {
            pointDataInspector.transform.rotation = transform.rotation;
            currentDataInspector = pointDataInspector;
        }

        if (selectionMode == SelectionMode.Sphere && areaSphereDataInspector != null)
        {
            areaSphereDataInspector.transform.localScale = Vector3.one * (selectionRadius * 2);
            areaSphereDataInspector.transform.rotation = transform.rotation;
            currentDataInspector = areaSphereDataInspector;
        }

        if (selectionMode == SelectionMode.Cube && areaBoxDataInspector != null)
        {
            areaBoxDataInspector.transform.localScale = Vector3.one * (selectionCubeHalfSize * 2);
            areaBoxDataInspector.transform.rotation = transform.rotation;
            currentDataInspector = areaBoxDataInspector;
        }


        if (currentDataInspector != null)
        {

            currentDataInspector.SetActiveState(true);


            if (!astrovisioDatasetRenderer.DataMapping.isolateSelection && currentDataSelectionGameObject != null)
            {
                currentDataSelectionGameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public PointDistance? GetLastNearest()
    {
        return nearest;
    }

    public SelectionResult GetLastAreaSelection()
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

        float scale = 1.0f; // Matching shader scale

        switch (XScale)
        {
            case ScalingType.Sqrt:
                point.x = signed_sqrt(point.x, scale);
                break;
            case ScalingType.Log:
                point.x = signed_log10(point.x, scale);
                break;
        }

        switch (YScale)
        {
            case ScalingType.Sqrt:
                point.y = signed_sqrt(point.y, scale);
                break;
            case ScalingType.Log:
                point.y = signed_log10(point.y, scale);
                break;
        }

        switch (ZScale)
        {
            case ScalingType.Sqrt:
                point.z = signed_sqrt(point.z, scale);
                break;
            case ScalingType.Log:
                point.z = signed_log10(point.z, scale);
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

    public async Task<SelectionResult> ComputeAreaSelection(Vector3 worldPoint)
    {
        int dataCount = data[0].Length;
        computingAreaSelection = true;
        Vector3 queryPoint = TransformWorldToDataSpace(worldPoint);

        // Calculate data space radius/size before entering the async task
        Vector3 dataSpaceSize = Vector3.zero;

        switch (selectionMode)
        {
            case SelectionMode.Sphere:
            case SelectionMode.Cube:
                dataSpaceSize = TransformRadiusToDataSpace(
                    selectionMode == SelectionMode.Sphere ? selectionRadius : selectionCubeHalfSize
                );
                break;
        }

        List<int> indices = null;

        // Copy values for use in async context
        var mode = selectionMode;
        var size = dataSpaceSize;

        SelectionResult result = null;

        await Task.Run(() =>
        {
            switch (mode)
            {
                case SelectionMode.Sphere:
                    indices = manager.FindPointsInEllipsoid(queryPoint, size);
                    break;

                case SelectionMode.Cube:
                    indices = manager.FindPointsInBox(queryPoint, size);
                    break;

                default:
                    indices = new List<int>();
                    break;
            }

            int[] visibilityArray = new int[dataCount];
            for (int i = 0; i < indices.Count; i++)
            {
                visibilityArray[indices[i]] = 1;
            }

            result = new SelectionResult
            {
                SelectedIndices = indices,
                SelectedArray = visibilityArray,
                CenterPoint = queryPoint,
                SelectionRadius = selectionMode == SelectionMode.Sphere ? selectionRadius : selectionCubeHalfSize,
                SelectionMode = selectionMode
            };

            if (indices.Count > 0)
            {
                result.AggregatedValues = AggregateData(indices);
            }
        });
        computingAreaSelection = false;
        return result;
    }

    private Vector3 TransformWorldToDataSpace(Vector3 worldPoint)
    {
        // Transform from world space to local space of the point cloud
        Vector3 localPoint = pointCloudTransform.InverseTransformPoint(worldPoint);

        float scale = 1.0f;
        Vector3 result = new Vector3();

        // X axis
        result.x = TransformAxisToDataSpace(localPoint.x, xRange, xTargetRange, XScale, scale);

        // Y axis
        result.y = TransformAxisToDataSpace(localPoint.y, yRange, yTargetRange, YScale, scale);

        // Z axis
        result.z = TransformAxisToDataSpace(localPoint.z, zRange, zTargetRange, ZScale, scale);

        return result;
    }

    // New helper method to handle axis transformation with zero-crossing ranges
    private float TransformAxisToDataSpace(float localValue, Vector2 dataRange, Vector2 targetRange, ScalingType scalingType, float scale)
    {
        if (scalingType == ScalingType.Linear)
        {
            return RemapInverseUnclamped(localValue, targetRange, dataRange);
        }

        // Check if the range crosses zero
        bool crossesZero = dataRange.x < 0 && dataRange.y > 0;

        if (crossesZero && (scalingType == ScalingType.Log || scalingType == ScalingType.Sqrt))
        {
            // Handle zero-crossing ranges specially
            // We need to find where zero maps to in the target range

            // Scale the data range boundaries
            float scaledMin = ApplyScalingFunction(dataRange.x, scalingType, scale);
            float scaledMax = ApplyScalingFunction(dataRange.y, scalingType, scale);
            float scaledZero = ApplyScalingFunction(0f, scalingType, scale); // This should be 0

            // Find where zero maps to in target space
            float zeroInTarget = Mathf.Lerp(targetRange.x, targetRange.y,
                Mathf.InverseLerp(scaledMin, scaledMax, scaledZero));

            // Determine which side of zero we're on in target space
            if (localValue < zeroInTarget)
            {
                // We're in the negative part
                // Map from [targetRange.x, zeroInTarget] to [dataRange.x, 0]
                float t = Mathf.InverseLerp(targetRange.x, zeroInTarget, localValue);
                float scaledValue = Mathf.Lerp(scaledMin, scaledZero, t);
                return ApplyInverseScalingFunction(scaledValue, scalingType, scale);
            }
            else
            {
                // We're in the positive part
                // Map from [zeroInTarget, targetRange.y] to [0, dataRange.y]
                float t = Mathf.InverseLerp(zeroInTarget, targetRange.y, localValue);
                float scaledValue = Mathf.Lerp(scaledZero, scaledMax, t);
                return ApplyInverseScalingFunction(scaledValue, scalingType, scale);
            }
        }
        else
        {
            // Standard case: range doesn't cross zero or using squared scaling
            float scaledMin = ApplyScalingFunction(dataRange.x, scalingType, scale);
            float scaledMax = ApplyScalingFunction(dataRange.y, scalingType, scale);
            float unmappedScaled = RemapInverseUnclamped(localValue, targetRange, new Vector2(scaledMin, scaledMax));
            return ApplyInverseScalingFunction(unmappedScaled, scalingType, scale);
        }
    }

    // Helper function to apply scaling
    private float ApplyScalingFunction(float value, ScalingType scalingType, float scale)
    {
        switch (scalingType)
        {
            case ScalingType.Log:
                return signed_log10(value, scale);
            case ScalingType.Sqrt:
                return signed_sqrt(value, scale);
            default:
                return value;
        }
    }

    // Helper function to apply inverse scaling
    private float ApplyInverseScalingFunction(float value, ScalingType scalingType, float scale)
    {
        switch (scalingType)
        {
            case ScalingType.Log:
                return inverse_signed_log10(value, scale);
            case ScalingType.Sqrt:
                return inverse_signed_sqrt(value, scale);
            default:
                return value;
        }
    }

    private Vector3 TransformRadiusToDataSpace(float worldRadius)
    {
        Vector3 center = TransformWorldToDataSpace(controllerTransform.position);

        // For more accuracy, sample more points around the sphere
        List<Vector3> samplePoints = new List<Vector3>();

        // Add axis-aligned points
        samplePoints.Add(controllerTransform.position + Vector3.right * worldRadius);
        samplePoints.Add(controllerTransform.position - Vector3.right * worldRadius);
        samplePoints.Add(controllerTransform.position + Vector3.up * worldRadius);
        samplePoints.Add(controllerTransform.position - Vector3.up * worldRadius);
        samplePoints.Add(controllerTransform.position + Vector3.forward * worldRadius);
        samplePoints.Add(controllerTransform.position - Vector3.forward * worldRadius);

        // Add diagonal points for better approximation
        samplePoints.Add(controllerTransform.position + (Vector3.right + Vector3.up).normalized * worldRadius);
        samplePoints.Add(controllerTransform.position + (Vector3.right - Vector3.up).normalized * worldRadius);
        samplePoints.Add(controllerTransform.position + (Vector3.forward + Vector3.up).normalized * worldRadius);
        samplePoints.Add(controllerTransform.position + (Vector3.forward - Vector3.up).normalized * worldRadius);

        float maxRadiusX = 0, maxRadiusY = 0, maxRadiusZ = 0;

        foreach (var point in samplePoints)
        {
            Vector3 transformedPoint = TransformWorldToDataSpace(point);
            maxRadiusX = Mathf.Max(maxRadiusX, Mathf.Abs(transformedPoint.x - center.x));
            maxRadiusY = Mathf.Max(maxRadiusY, Mathf.Abs(transformedPoint.y - center.y));
            maxRadiusZ = Mathf.Max(maxRadiusZ, Mathf.Abs(transformedPoint.z - center.z));
        }

        return new Vector3(maxRadiusX, maxRadiusY, maxRadiusZ);
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

    private float signed_log10(float x, float scale)
    {
        return Math.Sign(x) * (float)Math.Log10(1 + Math.Abs(x) / scale);
    }

    private float inverse_signed_log10(float y, float scale)
    {
        return Math.Sign(y) * scale * ((float)Math.Pow(10, Math.Abs(y)) - 1);
    }

    private float signed_sqrt(float x, float scale)
    {
        return Math.Sign(x) * (float)Math.Sqrt(Math.Abs(x) / scale);
    }

    private float inverse_signed_sqrt(float y, float scale)
    {
        float absY = Math.Abs(y);
        return Math.Sign(y) * scale * absY * absY;
    }

    private void OnDestroy()
    {
        // Debug.Log("OnDestroy");
        if (pointDataInspector != null)
        {
            Destroy(pointDataInspector.gameObject);
        }
        if (areaSphereDataInspector != null)
        {
            Destroy(areaSphereDataInspector.gameObject);
        }
        if (areaBoxDataInspector != null)
        {
            Destroy(areaBoxDataInspector.gameObject);
        }

        // Rimuovi il listener per evitare memory leak
        selectAction.action.performed -= OnSpacebarPressed;
    }

    private void OnDrawGizmosSelected()
    {
        if (controllerTransform == null) return;

        Gizmos.color = new Color(0, 1, 0, 0.3f);

        switch (selectionMode)
        {
            case SelectionMode.Sphere:
                Gizmos.DrawWireSphere(controllerTransform.position, selectionRadius);
                break;

            case SelectionMode.Cube:
                Gizmos.DrawWireCube(controllerTransform.position, Vector3.one * (selectionCubeHalfSize * 2));
                break;
        }
    }
}
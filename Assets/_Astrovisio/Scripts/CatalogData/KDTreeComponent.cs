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

    [Header("Area Selection Settings")]
    public SelectionMode selectionMode = SelectionMode.Sphere;
    public float selectionRadius = 0.05f; // For sphere selection
    public float selectionCubeHalfSize = 0.05f; // For cube selection
    public AggregationMode aggregationMode = AggregationMode.Average;
    public bool showSelectionGizmo = true;

    [Header("Transforms")]
    public Transform pointCloudTransform;
    public Transform controllerTransform;

    // private int[] xyz = new int[] { 0, 1, 2 };

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
    private Mapping mapping;


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
        Debug.Log("PerformSelection");

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

    public async Task Initialize(float[][] pointData, Vector3 pivot)
    {
        mapping = astrovisioDatasetRenderer.DataMapping.Mapping;
        data = pointData;
        int[] xyz = new int[] {
            mapping.X.SourceIndex,
            mapping.Y.SourceIndex,
            mapping.Z.SourceIndex
        };

        _ = await Task.Run(() => manager = new KDTreeManager(data, pivot, xyz));

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
                data[mapping.X.SourceIndex][lastNearestIndex],
                data[mapping.Y.SourceIndex][lastNearestIndex],
                data[mapping.Z.SourceIndex][lastNearestIndex]
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
                data[mapping.X.SourceIndex][idx],
                data[mapping.Y.SourceIndex][idx],
                data[mapping.Z.SourceIndex][idx]
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

        Mapping mapping = astrovisioDatasetRenderer.DataMapping.Mapping;

        switch (mapping.X.ScalingType)
        {
            case ScalingType.Sqrt:
                point.x = signed_sqrt(point.x);
                break;
            case ScalingType.Log:
                point.x = signed_log10(point.x);
                break;
        }

        switch (mapping.Y.ScalingType)
        {
            case ScalingType.Sqrt:
                point.y = signed_sqrt(point.y);
                break;
            case ScalingType.Log:
                point.y = signed_log10(point.y);
                break;
        }

        switch (mapping.Z.ScalingType)
        {
            case ScalingType.Sqrt:
                point.z = signed_sqrt(point.z);
                break;
            case ScalingType.Log:
                point.z = signed_log10(point.z);
                break;
        }

        return point;
    }

    private Vector3 RemapPoint(Vector3 point)
    {
        Mapping mapping = astrovisioDatasetRenderer.DataMapping.Mapping;

        point.x = RemapInverseUnclamped(
            point.x,
            new Vector2(mapping.X.DataMinVal, mapping.X.DataMaxVal),
            mapping.X.InverseMapping ? new Vector2(mapping.X.TargetMaxVal, mapping.X.TargetMinVal) : new Vector2(mapping.X.TargetMinVal, mapping.X.TargetMaxVal)
            );
        point.y = RemapInverseUnclamped(
            point.y,
            new Vector2(mapping.Y.DataMinVal, mapping.Y.DataMaxVal),
            mapping.Y.InverseMapping ? new Vector2(mapping.Y.TargetMaxVal, mapping.Y.TargetMinVal) : new Vector2(mapping.Y.TargetMinVal, mapping.Y.TargetMaxVal)
        );
        point.z = RemapInverseUnclamped(
            point.z,
            new Vector2(mapping.Z.DataMinVal, mapping.Z.DataMaxVal),
            mapping.Z.InverseMapping ? new Vector2(mapping.Z.TargetMaxVal, mapping.Z.TargetMinVal) : new Vector2(mapping.Z.TargetMinVal, mapping.Z.TargetMaxVal)
        );
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

        Vector3 result = new Vector3();

        // X axis
        result.x = TransformAxisToDataSpace(localPoint.x, astrovisioDatasetRenderer.DataMapping.Mapping.X);

        // Y axis
        result.y = TransformAxisToDataSpace(localPoint.y, astrovisioDatasetRenderer.DataMapping.Mapping.Y);

        // Z axis
        result.z = TransformAxisToDataSpace(localPoint.z, astrovisioDatasetRenderer.DataMapping.Mapping.Z);

        return result;
    }

    // New helper method to handle axis transformation with zero-crossing ranges
    private float TransformAxisToDataSpace(float localValue, MapFloatEntry entry)
    {

        Vector2 dataRange = new Vector2(entry.DataMinVal, entry.DataMaxVal);
        Vector2 targetRange = entry.InverseMapping ? new Vector2(entry.TargetMaxVal, entry.TargetMinVal) : new Vector2(entry.TargetMinVal, entry.TargetMaxVal);

        switch (entry.ScalingType)
        {
            case ScalingType.Linear:
                return RemapInverseUnclamped(localValue, targetRange, dataRange);

            case ScalingType.Log:
            case ScalingType.Sqrt:
                float scaledMin = ApplyScalingFunction(dataRange.x, entry.ScalingType);
                float scaledMax = ApplyScalingFunction(dataRange.y, entry.ScalingType);
                float unmappedScaled = RemapUnclamped(localValue, targetRange, new Vector2(scaledMin, scaledMax));
                return ApplyInverseScalingFunction(unmappedScaled, entry.ScalingType);

            default:
                return 0;
        }

    }

    // Helper function to apply scaling
    private float ApplyScalingFunction(float value, ScalingType scalingType)
    {
        switch (scalingType)
        {
            case ScalingType.Log:
                return signed_log10(value);
            case ScalingType.Sqrt:
                return signed_sqrt(value);
            default:
                return value;
        }
    }

    // Helper function to apply inverse scaling
    private float ApplyInverseScalingFunction(float value, ScalingType scalingType)
    {
        switch (scalingType)
        {
            case ScalingType.Log:
                return inverse_signed_log10(value);
            case ScalingType.Sqrt:
                return inverse_signed_sqrt(value);
            default:
                return value;
        }
    }

    private Vector3 TransformRadiusToDataSpace(float worldRadius)
    {
        Vector3 center = TransformWorldToDataSpace(controllerTransform.position);

        // Genera punti uniformemente distribuiti su una sfera nel world space
        Vector3[] spherePoints = GenerateSphereSamplePoints(controllerTransform.position, worldRadius);

        float maxRadiusX = 0, maxRadiusY = 0, maxRadiusZ = 0;

        // Trasforma ogni punto della sfera e calcola il bounding ellipsoid nel data space
        foreach (var worldPoint in spherePoints)
        {
            Vector3 transformedPoint = TransformWorldToDataSpace(worldPoint);
            Vector3 diff = transformedPoint - center;

            maxRadiusX = Mathf.Max(maxRadiusX, Mathf.Abs(diff.x));
            maxRadiusY = Mathf.Max(maxRadiusY, Mathf.Abs(diff.y));
            maxRadiusZ = Mathf.Max(maxRadiusZ, Mathf.Abs(diff.z));
        }

        return new Vector3(maxRadiusX, maxRadiusY, maxRadiusZ);
    }

    private Vector3[] GenerateSphereSamplePoints(Vector3 center, float radius)
    {
        var points = new List<Vector3>();

        // Aumentato significativamente il numero di campioni per maggiore precisione
        int fibonacciSamples = 64;

        // Genera punti uniformemente distribuiti su una sfera usando la spirale di Fibonacci
        for (int i = 0; i < fibonacciSamples; i++)
        {
            float y = 1 - 2f * i / (fibonacciSamples - 1f); // y va da 1 a -1
            float radiusAtY = Mathf.Sqrt(1 - y * y);

            float theta = Mathf.PI * (1 + Mathf.Sqrt(5)) * i; // Angolo dorato

            float x = Mathf.Cos(theta) * radiusAtY;
            float z = Mathf.Sin(theta) * radiusAtY;

            Vector3 direction = new Vector3(x, y, z);
            points.Add(center + direction * radius);
        }

        // Aggiungi punti cardinali primari
        points.Add(center + Vector3.right * radius);
        points.Add(center - Vector3.right * radius);
        points.Add(center + Vector3.up * radius);
        points.Add(center - Vector3.up * radius);
        points.Add(center + Vector3.forward * radius);
        points.Add(center - Vector3.forward * radius);

        // Aggiungi punti diagonali critici (questi sono importanti per rotazioni a 30°, 45°, 60°)
        // Diagonali principali sul piano XZ (critiche per rotazioni Y)
        points.Add(center + new Vector3(1, 0, 1).normalized * radius);    // 45°
        points.Add(center + new Vector3(1, 0, -1).normalized * radius);   // -45°
        points.Add(center + new Vector3(-1, 0, 1).normalized * radius);   // 135°
        points.Add(center + new Vector3(-1, 0, -1).normalized * radius);  // -135°

        // Aggiungi punti a 30° e 60° sul piano XZ (critici per l'errore residuo)
        for (int i = 0; i < 12; i++) // Ogni 30°
        {
            float angle = i * 30f * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            points.Add(center + direction * radius);
        }

        // Aggiungi punti a elevazioni intermedie (15°, 30°, 45°, 60°, 75°)
        float[] elevations = { 15f, 30f, 45f, 60f, 75f };
        foreach (float elevation in elevations)
        {
            float elevRad = elevation * Mathf.Deg2Rad;
            float y = Mathf.Sin(elevRad);
            float radiusAtElev = Mathf.Cos(elevRad);

            // 8 direzioni azimutali per ogni elevazione
            for (int i = 0; i < 8; i++)
            {
                float azimuth = i * 45f * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(
                    Mathf.Cos(azimuth) * radiusAtElev,
                    y,
                    Mathf.Sin(azimuth) * radiusAtElev
                );
                points.Add(center + direction * radius);

                // Aggiungi anche l'elevazione negativa
                direction.y = -y;
                points.Add(center + direction * radius);
            }
        }

        return points.ToArray();
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

    private float RemapUnclamped(float value, Vector2 fromRange, Vector2 toRange)
    {
        // Evita divisione per zero
        if (Mathf.Approximately(fromRange.y, fromRange.x))
        {
            return toRange.x;
        }

        float t = (value - fromRange.x) / (fromRange.y - fromRange.x);
        return toRange.x + t * (toRange.y - toRange.x);
    }

    private float signed_log10(float x, float scale = 1f)
    {
        return Math.Sign(x) * (float)Math.Log10(1 + Math.Abs(x) / scale);
    }

    private float inverse_signed_log10(float y, float scale = 1f)
    {
        return Math.Sign(y) * scale * ((float)Math.Pow(10, Math.Abs(y)) - 1);
    }

    private float signed_sqrt(float x, float scale = 1f)
    {
        return Math.Sign(x) * (float)Math.Sqrt(Math.Abs(x) / scale);
    }

    private float inverse_signed_sqrt(float y, float scale = 1f)
    {
        return Math.Sign(y) * scale * (y * y);
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
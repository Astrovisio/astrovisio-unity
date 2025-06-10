using UnityEngine;
using System.Threading.Tasks;

public class KDTreeComponent : MonoBehaviour
{
    [Header("Transforms")]
    public Transform pointCloudTransform;
    public Transform controllerTransform;

    [Header("Query Settings")]
    public float queryInterval = 0.05f;

    [Header("Mapping Ranges (visivi)")]
    public Vector2 xRange = new Vector2(-10f, 10f);
    public Vector2 yRange = new Vector2(-10f, 10f);
    public Vector2 zRange = new Vector2(-10f, 10f);
    public Vector2 normalizedRange = new Vector2(-1f, 1f);

    [Header("Debug Gizmo")]
    public Color gizmoColor = Color.red;
    public float gizmoRadius = 0.05f;

    [Header("Debug Sphere (in game)")]
    public Color sphereColor = Color.red;
    public float sphereRadius = 0.05f;
    private GameObject debugSphere;

    private float[][] pointData;
    private KDTree kdTree;
    private float lastQueryTime = -1f;
    private bool isTreeReady = false;

    private int lastNearestIndex = -1;

    private bool isQueryRunning = false;


    public async void InitializeAsync(float[][] data)
    {
        pointData = data;
        isTreeReady = false;

        kdTree = await Task.Run(() => new KDTree(pointData));
        isTreeReady = true;

        Debug.Log("KDTree costruito nello spazio originale dei dati.");

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

    private async void Start()
    {
        // Avvia la query asincrona infinita se il KDTree è già pronto
        // oppure appena è pronto dopo InitializeAsync
        await WaitUntilTreeReady();
        RunQueryLoop();
    }

    // void Update()
    // {
    //     if (!isTreeReady || controllerTransform == null || pointCloudTransform == null)
    //         return;

    //     if (Time.unscaledTime - lastQueryTime < queryInterval)
    //         return;

    //     lastQueryTime = Time.unscaledTime;

    //     // Vector3 controllerWorld = controllerTransform.position;
    //     Vector3 controllerWorld = controllerTransform.position + controllerTransform.forward * 0.1f;
    //     Vector3 controllerLocal = pointCloudTransform.InverseTransformPoint(controllerWorld);

    //     float x = RemapInverse(controllerLocal.x, normalizedRange, xRange);
    //     float y = RemapInverse(controllerLocal.y, normalizedRange, yRange);
    //     float z = RemapInverse(controllerLocal.z, normalizedRange, zRange);
    //     Vector3 queryPoint = new Vector3(x, y, z);

    //     var (nearestIndex, distSqr) = kdTree.FindNearest(queryPoint);
    //     lastNearestIndex = nearestIndex;

    //     float value = pointData[3][nearestIndex];
    //     Debug.Log($"Punto più vicino: indice={nearestIndex}, valore={value}, distanza²={distSqr}");

    //     // Sposta la sfera debug nel mondo
    //     if (debugSphere != null && lastNearestIndex >= 0)
    //     {
    //         Vector3 pointOriginal = new Vector3(
    //             pointData[0][lastNearestIndex],
    //             pointData[1][lastNearestIndex],
    //             pointData[2][lastNearestIndex]
    //         );

    //         Vector3 worldPos = pointCloudTransform.TransformPoint(pointOriginal);
    //         debugSphere.transform.position = worldPos;
    //     }
    // }

    private async Task WaitUntilTreeReady()
    {
        while (!isTreeReady)
            await Task.Yield();
    }

    private void RunQueryLoop()
    {
        if (isQueryRunning) return;

        isQueryRunning = true;
        _ = QueryLoop();
    }

    private async Task QueryLoop()
    {
        while (enabled && isTreeReady && controllerTransform != null && pointCloudTransform != null)
        {
            await Task.Yield(); // evitare blocco main thread

            Vector3 controllerWorld = controllerTransform.position;
            // Vector3 controllerWorld = controllerTransform.position + controllerTransform.forward * 0.3f;
            Vector3 controllerLocal = pointCloudTransform.InverseTransformPoint(controllerWorld);

            float x = RemapInverse(controllerLocal.x, normalizedRange, xRange);
            float y = RemapInverse(controllerLocal.y, normalizedRange, yRange);
            float z = RemapInverse(controllerLocal.z, normalizedRange, zRange);
            Vector3 queryPoint = new Vector3(x, y, z);

            var (nearestIndex, distSqr) = kdTree.FindNearest(queryPoint);
            lastNearestIndex = nearestIndex;

            float value = pointData[3][nearestIndex];
            // Debug.Log($"Punto più vicino: indice={nearestIndex}, valore={value}, distanza²={distSqr}");

            // Sposta la sfera debug nel mondo
            if (debugSphere != null && lastNearestIndex >= 0)
            {
                Vector3 pointOriginal = new Vector3(
                    pointData[0][lastNearestIndex],
                    pointData[1][lastNearestIndex],
                    pointData[2][lastNearestIndex]
                );

                Vector3 worldPos = pointCloudTransform.TransformPoint(pointOriginal);
                debugSphere.transform.position = worldPos;
            }

            // Attendi un minimo prima della prossima iterazione
            await Task.Delay((int)(queryInterval * 1000)); // puoi regolare qui se serve rate limit
        }

        isQueryRunning = false;
    }

    float RemapInverse(float val, Vector2 from, Vector2 to)
    {
        float t = Mathf.InverseLerp(from.x, from.y, val);
        return Mathf.Lerp(to.x, to.y, t);
    }

    public void SetRanges(Vector2 x, Vector2 y, Vector2 z)
    {
        xRange = x;
        yRange = y;
        zRange = z;
    }

    private void OnDrawGizmos()
    {
        if (!isTreeReady || lastNearestIndex < 0 || pointData == null)
            return;

        // Ottieni la posizione originale del punto nel sistema dati
        Vector3 pointOriginal = new Vector3(
            pointData[0][lastNearestIndex],
            pointData[1][lastNearestIndex],
            pointData[2][lastNearestIndex]
        );

        // Trasformalo nello spazio locale del pointCloudTransform, poi in mondo
        Vector3 worldPos = pointCloudTransform.TransformPoint(pointOriginal);

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(worldPos, gizmoRadius);
    }
}
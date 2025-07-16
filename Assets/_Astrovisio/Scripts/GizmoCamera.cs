using UnityEngine;

// [ExecuteAlways] // Facoltativo: per vedere l'effetto anche in editor
public class GizmoCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float distance = 5.0f;
    public float rotationSpeed = 5.0f;
    public Vector2 verticalLimits = new Vector2(-90f, 90f);

    private Vector2 orbitAngles = new Vector2(30f, 45f);

    private void Start()
    {
        if (target == null)
        {
            GameObject go = new GameObject("OrbitTarget");
            go.transform.position = Vector3.zero;
            target = go.transform;
        }
    }

    private void LateUpdate()
    {
        // Rotazione con tasto sinistro
        if (Input.GetMouseButton(0))
        {
            orbitAngles.y += Input.GetAxis("Mouse X") * rotationSpeed;
            orbitAngles.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
            orbitAngles.x = Mathf.Clamp(orbitAngles.x, verticalLimits.x, verticalLimits.y);
        }

        Quaternion rotation = Quaternion.Euler(orbitAngles.x, orbitAngles.y, 0);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        transform.position = target.position + offset;
        transform.rotation = rotation;
    }

    public Vector2 GetOrbitAngles() => orbitAngles;
    public Quaternion GetRotation() => Quaternion.Euler(orbitAngles);

}

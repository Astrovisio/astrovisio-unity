using UnityEngine;

public class GizmoCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;


    private float fixedDistance;
    private OrbitCameraController orbitCamera;


    private void Start()
    {
        orbitCamera = FindFirstObjectByType<OrbitCameraController>();
        if (orbitCamera == null)
        {
            Debug.LogError("OrbitCameraController not found.");
            return;
        }

        if (target != null && orbitCamera.target != null)
        {
            Vector3 initialDir = orbitCamera.transform.position - orbitCamera.target.position;
            fixedDistance = initialDir.magnitude;
        }
    }

    private void LateUpdate()
    {
        if (orbitCamera != null && target != null && orbitCamera.target != null)
        {
            Vector3 dir = (orbitCamera.transform.position - orbitCamera.target.position).normalized;
            transform.position = target.position + dir * fixedDistance;
            transform.rotation = orbitCamera.transform.rotation;
        }
    }


}

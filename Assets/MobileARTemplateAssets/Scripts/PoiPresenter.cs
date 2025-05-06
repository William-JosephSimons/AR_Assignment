using UnityEngine;
using TMPro;

public class PoiPresenter : MonoBehaviour {
    public Canvas infoCanvas;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI hoursText;


 private Transform cam;
    private PoiData data;
    private Bounds bounds;

    [Header("Visibility Settings")]
    public float maxDistance = 10f;
    // Angle threshold (in degrees) within which the POI becomes visible
    public float angleThreshold = 18f;

    [Header("Perimeter Positioning")]
    public float margin = 0.2f;         // meters out from asset edge
    public float smoothSpeed = 5f;      // interpolation speed

    private Vector3 targetPosition;

    void Awake() {
        cam = Camera.main.transform;
        infoCanvas?.gameObject.SetActive(false);

        // Calculate asset bounds once
        var colliders = GetComponentsInChildren<Collider>();
        bounds = new Bounds(transform.position, Vector3.zero);
        foreach (var col in colliders) bounds.Encapsulate(col.bounds);
    }

    public void Initialize(PoiData poiData) {
        data = poiData;
        nameText.text = data.name;
        descriptionText.text = data.description;
        hoursText.text = data.hours;
    }

   void Update() {
        Vector3 toPoi = transform.position - cam.position;
        float distance = toPoi.magnitude;
        float angle = Vector3.Angle(cam.forward, toPoi);
        bool withinView = (angle <= angleThreshold) && (distance <= maxDistance);

        // Toggle panel visibility
        if (infoCanvas != null && infoCanvas.gameObject.activeSelf != withinView) {
            infoCanvas.gameObject.SetActive(withinView);
        }

        if (withinView) {
            // Direction from POI to camera (for panel positioning)
            // Flatten direction on XZ plane
            Vector3 dirToCamera = cam.position - transform.position;
            dirToCamera.y = 0;
            dirToCamera.Normalize();

            // Find the perimeter point on the bounding box
            float tX = bounds.extents.x / Mathf.Abs(dirToCamera.x);
            float tZ = bounds.extents.z / Mathf.Abs(dirToCamera.z);
            float t = Mathf.Min(tX, tZ);
            Vector3 surfacePoint = bounds.center + dirToCamera * t;

            // Offset outwards and set fixed height
            targetPosition = surfacePoint + dirToCamera * margin;

            // Smoothly move the panel there
            infoCanvas.transform.position = Vector3.Lerp(
                infoCanvas.transform.position,
                targetPosition,
                Time.deltaTime * smoothSpeed
            );

            // Always face the camera
            infoCanvas.transform.LookAt(cam);
            infoCanvas.transform.Rotate(0, 180, 0);
        }
    }
}

using UnityEngine;
using TMPro;

public class PoiPresenter : MonoBehaviour {
    public Canvas infoCanvas;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI hoursText;

    private Transform cam;
    private PoiData data;
    private readonly float maxDistance = 10f;
    // Angle threshold (in degrees) within which the POI becomes visible
    private readonly float angleThreshold = 15f;

    void Awake() {
        cam = Camera.main.transform;
        infoCanvas?.gameObject.SetActive(false);
    }

    public void Initialize(PoiData poiData) {
        data = poiData;
        nameText.text = data.name;
        descriptionText.text = data.description;
        hoursText.text = data.hours;
    }

    void Update() {
        Vector3 dirToPoi = transform.position - cam.position;
        float distance = dirToPoi.magnitude;
        float angle = Vector3.Angle(cam.forward, dirToPoi);
        bool withinAngle = angle <= angleThreshold;
        bool withinDistance = distance <= maxDistance;
        bool show = withinAngle && withinDistance;

        // Toggle canvas visibility
        if (infoCanvas != null && infoCanvas.gameObject.activeSelf != show) {
            infoCanvas.gameObject.SetActive(show);
        }

        // If visible, face the camera
        if (show) {
            infoCanvas.transform.LookAt(cam);
            infoCanvas.transform.Rotate(0, 180, 0);
        }
    }
}

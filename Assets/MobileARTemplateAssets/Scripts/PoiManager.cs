using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PoiData {
    public string name;
    public string description;
    public string hours;
    public Vector3 position; // Phone relative positions from JSON
}

[System.Serializable]
public class PoiDataWrapper {
    public PoiData[] pois;
}

public class PoiManager : MonoBehaviour {
    public GameObject poiPrefab;
    private readonly List<GameObject> spawned = new();

    void Start() {
        if (poiPrefab == null) {
             Debug.LogError("PoiManager: Poi Prefab is not assigned in the Inspector!");
             return;
        }

        // Load JSON text from Resources
        TextAsset textAsset = Resources.Load<TextAsset>("pois");
        if (textAsset == null) {
            Debug.LogError("PoiManager: Failed to load pois.json from Resources.");
            return;
        }
        // Parse JSON
        PoiDataWrapper wrapper = JsonUtility.FromJson<PoiDataWrapper>(textAsset.text);
        if (wrapper == null || wrapper.pois == null) {
            Debug.LogError("PoiManager: Failed to parse pois.json");
            return;
        }

        Debug.Log($"PoiManager: Spawning {wrapper.pois.Length} POIs in positions.");

        foreach (var poi in wrapper.pois) {
            // Instantiate at the position defined in the JSON
            var go = Instantiate(poiPrefab, poi.position, Quaternion.identity);
            go.name = poi.name;

            // Populate fields
            var presenter = go.GetComponent<PoiPresenter>();
            if (presenter != null) {
                presenter.Initialize(poi);
            } else {
                Debug.LogError($"PoiManager: PoiPrefab is missing PoiPresenter component on POI '{poi.name}'");
            }

            spawned.Add(go);
        }
    }
}
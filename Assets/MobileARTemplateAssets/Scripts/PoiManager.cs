using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PoiData {
    public string name;
    public string description;
    public string hours;
    public Vector3 position;
}

[System.Serializable]
public class PoiDataWrapper {
    public PoiData[] pois;
}

public class PoiManager : MonoBehaviour {
    public GameObject poiPrefab;
    private List<GameObject> spawned = new List<GameObject>();

    void Start() {
        // Load JSON text from Resources
        TextAsset textAsset = Resources.Load<TextAsset>("pois");
        if (textAsset == null) {
            Debug.LogError("Failed to load pois.json");
            return;
        }
        // Parse JSON
        PoiDataWrapper wrapper = JsonUtility.FromJson<PoiDataWrapper>(textAsset.text);
        if (wrapper == null || wrapper.pois == null) {
            Debug.LogError("Failed to parse pois.json");
            return;
        }

        foreach (var poi in wrapper.pois) {
            var go = Instantiate(poiPrefab, poi.position, Quaternion.identity);
            go.name = poi.name;
            // Populate fields
            var presenter = go.GetComponent<PoiPresenter>();
            presenter.Initialize(poi);

            spawned.Add(go);
        }
    }
}
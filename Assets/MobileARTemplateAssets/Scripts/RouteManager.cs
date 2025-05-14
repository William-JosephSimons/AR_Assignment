using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouteManager : MonoBehaviour
{
    public Canvas mapCanvas;              // top-down map UI Canvas
    public Button goButton;               // UI Button to start AR view

    public GameObject waypointPrefab;     // Prefab for visualizing waypoints
    public LineRenderer pathLineRenderer; // Line renderer for drawing the path

    // Define the relative coordinates for the path waypoints
    // Path goes around the LEFT side of the POI block towards the destination.
    private readonly Vector3[] waypointWorldPositions = new Vector3[] {
        new(0, -1, -1.9f),       // 1. Start in front of first POI
        new(0, -1, 1),           // 2. Front Middle 
        new(-3, -1, 1f),         // 3. Front-left corner
        new(-3, -1, 5f),         // 4. Back-left corner
        new(0, -1, 5f),          // 5. Back Middle
        new(0, -1, 7.9f)         // 6. End in front of last POI
    };

    private readonly List<GameObject> spawnedWaypoints = new();

    void Start()
    {
        // Initial Setup & Validation
        if (pathLineRenderer == null) {
            pathLineRenderer = GetComponent<LineRenderer>();
             Debug.LogWarning("RouteManager: Path Line Renderer was not assigned, attempting to get it from GameObject.", this);
        }
         if (pathLineRenderer == null) {
             Debug.LogError("RouteManager: Path Line Renderer component not found or assigned! Cannot draw path.", this);
         } else {
             pathLineRenderer.enabled = false; // Start disabled
             pathLineRenderer.positionCount = 0;
         }

        if (mapCanvas == null) Debug.LogError("RouteManager: Map Canvas is not assigned!", this);
        if (goButton == null) Debug.LogError("RouteManager: Go Button is not assigned!", this);
        else goButton.onClick.AddListener(OnGoPressed);

        if (waypointPrefab == null) Debug.LogError("RouteManager: Waypoint Prefab is not assigned! Cannot spawn waypoints.", this);

        // Spawn Path immediately
        SpawnPath();

        // Show map UI initially
        ShowMap(true);
    }

    void ShowMap(bool show)
    {
        if (mapCanvas != null) {
            mapCanvas.enabled = show;
        }
        // Show/hide the button along with the map
        goButton?.gameObject.SetActive(show);
    }

    // Button simply hides the map
    void OnGoPressed()
    {
        Debug.Log("RouteManager: Go button pressed, hiding map.");
        ShowMap(false);
    }

    void SpawnPath()
    {
         // Check if prerequisites are met
         if (pathLineRenderer == null || waypointPrefab == null) {
             Debug.LogError("RouteManager: Cannot spawn path. Line Renderer or Waypoint Prefab is missing.");
             return;
         }
        if (waypointWorldPositions == null || waypointWorldPositions.Length < 2)
        {
            Debug.LogWarning("RouteManager: Not enough waypoints defined to draw a path.");
            return;
        }

        List<Vector3> currentWaypoints = new(waypointWorldPositions); // Use the predefined positions

        Debug.Log($"RouteManager: Spawning {currentWaypoints.Count} waypoints in positions.");

        for (int i = 0; i < currentWaypoints.Count; i++)
        {
            Vector3 worldPos = currentWaypoints[i];

            // Instantiate the waypoint visual marker at the positions
            GameObject waypointGO = Instantiate(waypointPrefab, worldPos, Quaternion.identity);
            waypointGO.name = $"Waypoint_{i}";
            spawnedWaypoints.Add(waypointGO);
        }

        // Configure the Line Renderer with positions
        pathLineRenderer.positionCount = currentWaypoints.Count;
        pathLineRenderer.SetPositions(currentWaypoints.ToArray());
        pathLineRenderer.enabled = true; // Make the path visible
    }
}
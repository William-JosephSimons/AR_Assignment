using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using System.Linq;

public class ARFloorplanAnchorSystem : MonoBehaviour
{
    [Header("Prefabs and Managers")]
    public GameObject floorplanPrefab;
    public ARAnchorManager anchorManager;
    public ARTrackedImageManager imageManager;
    public ARPlaneManager planeManager;
    public ARRaycastManager arRaycastManager;

    [Header("Marker Adjustment")]
    public string markerName = "EntryMarker"; // Name of image marker
    public Vector3 floorplanMarkerOffset = Vector3.zero; // Offset from marker to align floorplan

    [Header("Options")]
    public bool debugLog = true;
    public float maxPersonHeight = 2.5f;

    private GameObject floorplanInstance;
    private ARAnchor currentAnchor;
    private ARPlane lowestFloorPlane;

    void OnEnable()
    {
        imageManager.trackablesChanged.AddListener(OnImagesChanged);
        planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    void OnDisable()
    {
        imageManager.trackablesChanged.RemoveListener(OnImagesChanged);
    }

    async void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        foreach (var plane in args.added)
        {
            await HandlePlaneAddedOrUpdated(plane);
        }
        foreach (var plane in args.updated)
        {
            await HandlePlaneAddedOrUpdated(plane);
        }
        foreach (var planePair in args.removed)
        {
            if (lowestFloorPlane != null && lowestFloorPlane.trackableId == planePair.Key)
            {
                lowestFloorPlane = null;
            }
        }
    }

    async Task HandlePlaneAddedOrUpdated(ARPlane plane)
    {
        if (plane.alignment == PlaneAlignment.HorizontalUp)
        {
            if (debugLog) Debug.Log("Plane detected at: " + plane.center);

            if (lowestFloorPlane == null || (plane.transform.position.y > -maxPersonHeight && plane.transform.position.y < 0 && plane.transform.position.y < lowestFloorPlane.transform.position.y))
            {
                if (debugLog) Debug.Log("Lowest set at: " + plane.center);
                lowestFloorPlane = plane;
            }
            if (floorplanInstance)
            {
                // put the floorplan according to the plane
                Pose pose = FlattenPose(new(floorplanInstance.transform.position, floorplanInstance.transform.rotation));
                pose.position.y = lowestFloorPlane.transform.position.y;
                floorplanInstance.transform.SetWorldPose(pose);
                await reAnchor(pose, true);
            }
        }
    }

    async Task reAnchor(Pose pose, bool worldPositionStays = false)
    {
        if (debugLog) Debug.Log($"reAchoring {pose}");
        var result = await anchorManager.TryAddAnchorAsync(pose);
        if (result.status.IsSuccess())
        {
            if (currentAnchor != null) Destroy(currentAnchor.gameObject);
            floorplanInstance.transform.SetParent(result.value.transform, worldPositionStays);
            currentAnchor = result.value;
        }
    }

    async void OnImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            await AddOrUpdate(newImage);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            await AddOrUpdate(updatedImage);
        }

        foreach (var removedImage in eventArgs.removed)
        {
            // Handle removal if needed
        }
    }

    async Task AddOrUpdate(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage.name == markerName && trackedImage.trackingState == TrackingState.Tracking)
        {
            if (debugLog) Debug.Log($"Image {trackedImage.referenceImage.name} detected at {trackedImage.transform.position} with rotation {trackedImage.transform.eulerAngles}.");

            // Use Floorplan Marker Offset
            Vector3 newPosition = trackedImage.transform.position +
                                  trackedImage.transform.rotation * floorplanMarkerOffset;
            Quaternion newRotation = trackedImage.transform.rotation * Quaternion.Euler(90, 0, 0);
            Pose pose = FlattenPose(new(newPosition, newRotation));
            if (lowestFloorPlane)
            {
                pose.position.y = lowestFloorPlane.transform.position.y;
            }
            if (!floorplanInstance)
            {
                floorplanInstance = Instantiate(floorplanPrefab, pose.position, pose.rotation);
            }
            floorplanInstance.transform.SetWorldPose(pose);
            await reAnchor(pose, true);
        }
    }

    public static Pose FlattenPose(Pose pose)
    {
        // Step 1: Get forward direction from the pose's rotation
        Vector3 forward = pose.rotation * Vector3.forward;

        // Step 2: Remove vertical tilt
        forward.y = 0;

        // Step 3: Handle near-zero vectors (e.g., pointing straight up/down)
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        // Step 4: Build upright rotation
        Quaternion uprightRotation = Quaternion.LookRotation(forward, Vector3.up);

        // Step 5: Return flattened pose
        return new Pose(pose.position, uprightRotation);
    }

    public void Update()
    {
        if (floorplanInstance)
        {
            string[] names = {  "Access", "Normal", "WellLit" };
            GameObject[] paths = names.Select(name => GameObject.FindGameObjectWithTag(name))
                .Where(go => go != null)
                .ToArray();
            Debug.Log(paths);
            SceneData.changePathColours(paths, true, 5.7f, 5.8f);
        }
    }

}

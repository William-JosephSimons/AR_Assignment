using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;

public class ARFloorplanAnchorSystem : MonoBehaviour
{
    [Header("Prefabs and Managers")]
    public GameObject floorplanPrefab;
    public ARAnchorManager anchorManager;
    public ARTrackedImageManager imageManager;

    [Header("Marker Adjustment")]
    public string markerName = "EntryMarker"; // Name of image marker
    public Vector3 floorplanMarkerOffset = Vector3.zero; // Offset from marker to align floorplan

    [Header("Options")]
    public bool debugLog = true;

    private GameObject floorplanInstance;
    private ARAnchor currentAnchor;

    void OnEnable()
    {
        imageManager.trackablesChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        imageManager.trackablesChanged.RemoveListener(OnChanged);
    }

    async void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
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

            // Move floorplanMarkerOffset from marker, regardless of marker rotation
            Vector3 newPosition = trackedImage.transform.position +
                                  trackedImage.transform.rotation * floorplanMarkerOffset;
            Quaternion newRotation = trackedImage.transform.rotation * Quaternion.Euler(90, 0, 0);

            if (debugLog) Debug.Log("Refining floorplan using image marker.");

            Pose refinedPose = new(newPosition, newRotation);
            refinedPose = FlattenPose(refinedPose);
            if (!floorplanInstance)
            {
                floorplanInstance = Instantiate(floorplanPrefab, refinedPose.position, refinedPose.rotation);
            }
            else
            {
                floorplanInstance.transform.SetWorldPose(refinedPose);
            }

            var result = await anchorManager.TryAddAnchorAsync(refinedPose);
            if (result.status.IsSuccess())
            {
                if (currentAnchor != null) Destroy(currentAnchor.gameObject);
                floorplanInstance.transform.SetParent(result.value.transform, true);
                currentAnchor = result.value;
            }
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

}

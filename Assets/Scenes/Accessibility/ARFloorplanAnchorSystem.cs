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
            Quaternion newRotation = /* Quaternion.Euler(0, trackedImage.transform.eulerAngles.y, 0); ; */  trackedImage.transform.rotation * Quaternion.Euler(90, 0, 0);

            if (debugLog) Debug.Log("Refining floorplan using image marker.");

            Pose refinedPose = new(newPosition, newRotation);
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

}

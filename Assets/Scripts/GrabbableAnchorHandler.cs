using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(OVRGrabbable))]
public class GrabbableAnchorHandler : MonoBehaviour
{
    private OVRGrabbable grabbable;
    private OVRSpatialAnchor anchor;
    private bool wasGrabbedLastFrame = false;

    void Start()
    {
        grabbable = GetComponent<OVRGrabbable>();
        anchor = GetComponentInParent<OVRSpatialAnchor>();
    }

    void Update()
    {
        if (grabbable.isGrabbed)
        {
            if (!wasGrabbedLastFrame)
            {
                OnGrabStart();
            }

            wasGrabbedLastFrame = true;
        }
        else
        {
            if (wasGrabbedLastFrame)
            {
                OnGrabEnd();
            }

            wasGrabbedLastFrame = false;
        }
    }

    void OnGrabStart()
    {
        if (anchor != null)
        {
            anchor.enabled = false;
            Debug.Log("Anchor temporarily disabled for grab.");
        }
    }

    async void OnGrabEnd()
    {
        // Step 1: Destroy old anchor
        if (anchor != null)
        {
            Destroy(anchor);
            anchor = null;
        }

        // Step 2: Add new anchor at new position
        anchor = gameObject.AddComponent<OVRSpatialAnchor>();

        // Step 3: Wait for localization
        float timeout = 5f;
        float elapsed = 0f;
        while (!anchor.Localized && elapsed < timeout)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
        }

        if (!anchor.Localized)
        {
            Debug.LogWarning("Anchor failed to localize at new position.");
            return;
        }

        // Step 4: Save new anchor
        bool saved = await anchor.SaveAnchorAsync();
        if (saved)
        {
            Debug.Log($"✅ New anchor saved at new position: {anchor.Uuid}");

            // Step 5: Update decoration info (optional)
            var info = GetComponent<RoomDecorationInfo>();
            if (info != null)
            {
                info.uuid = anchor.Uuid.ToString();

                // Optional: update PlayerPrefs if your system tracks saved UUIDs
                SpatialAnchorDecorator.RemoveSavedDecoration(info.uuid); // remove old entry
                SpatialAnchorDecorator.SaveNewDecoration(info.uuid, info.prefabName); // re-save
            }
        }
        else
        {
            Debug.LogWarning("❌ Failed to save anchor after move.");
        }
    }

}

using System;
using System.Threading.Tasks;
using UnityEngine;
using Oculus.Interaction;
using Meta.XR.MRUtilityKitSamples;
[RequireComponent(typeof(Rigidbody))]
public class AnchorReplacer : MonoBehaviour
{
    private  EnvironmentPanelPlacement interactable;
    private const float AnchorTimeout = 5f;
    private bool wasBeingGrabbed = false;

    private string prefabName => gameObject.name;
    public int currentID;
    private void Awake()
    {
        interactable = GetComponentInChildren<EnvironmentPanelPlacement>();
        currentID = GetInstanceID();
    }

    private void Update()
    {
        if (interactable == null) return;

        if (interactable._isGrabbing == true)
        {
            if (!wasBeingGrabbed)
            {
                wasBeingGrabbed = true;
                OnGrabStart();
            }
        }
        else
        {
            if (wasBeingGrabbed)
            {
                wasBeingGrabbed = false;
                OnGrabReleased();
            }
        }
    }

    private void OnGrabStart()
    {
        Transform parent = transform.parent;
        if (parent == null) return;

        GameObject anchorGO = parent.gameObject;

        // ✅ Get info before destroying
        var info = anchorGO.GetComponent<RoomDecorationInfo>();
        if (info != null)
        {
            if (!string.IsNullOrEmpty(info.uuid))
            {
                // ✅ Remove from saved JSON
                SpatialAnchorDecorator.RemoveSavedDecoration(info.uuid);

            }

            // ✅ Optionally unregister from RoomMenuManager
            RoomMenuManager.Instance.UnregisterDecoration(info);
        }

        // ✅ Unparent and destroy
        transform.SetParent(null, true);
        Destroy(anchorGO);
    }


    private async void OnGrabReleased()
    {
        await Task.Yield(); // Wait one frame to settle transform

        // 1. Create new anchor GameObject
        string anchorName = $"Anchor_{prefabName}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        GameObject anchorGO = new GameObject(anchorName);
        anchorGO.transform.position = transform.position;
        anchorGO.transform.rotation = transform.rotation;

        // 2. Parent decoration under new anchor
        transform.SetParent(anchorGO.transform, true);

        // 3. Add OVRSpatialAnchor to new anchor
        var anchor = anchorGO.AddComponent<OVRSpatialAnchor>();

        // 4. Wait for localization
        float elapsed = 0f;
        while (!anchor.Localized && elapsed < AnchorTimeout)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
        }

        if (!anchor.Localized)
        {
            Debug.LogWarning("Anchor failed to localize after release.");
            Destroy(anchorGO);
            return;
        }

        // 5. Save anchor
        bool success = await anchor.SaveAnchorAsync();
        if (!success || anchor.Uuid == Guid.Empty)
        {
            Debug.LogWarning("Anchor failed to save.");
            Destroy(anchorGO);
            return;
        }

        // 6. Add RoomDecorationInfo and save UUID
        var info = anchorGO.AddComponent<RoomDecorationInfo>();
        info.ID = anchorGO.transform.GetChild(0).GetInstanceID();
        info.uuid = anchor.Uuid.ToString();
        info.prefabName = prefabName;
        info.linkedObject = anchorGO;

        SpatialAnchorDecorator.SaveNewDecoration(info.uuid, info.prefabName);
        RoomMenuManager.Instance.RegisterDecoration(info);

        

        Debug.Log($"New anchor saved for {prefabName} at {anchorGO.transform.position}");
    }

    
}

using UnityEngine;

public class DeletableDecoration : MonoBehaviour
{
    public string uuid;

    public async void Delete()
    {
        var anchor = GetComponent<OVRSpatialAnchor>();
        if (anchor != null)
        {
            bool deleted = await anchor.EraseAnchorAsync();
            if (deleted)
            {
                Debug.Log($"[Anchor] Deleted UUID: {uuid}");
                SpatialAnchorDecorator.RemoveSavedDecoration(uuid);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("Failed to delete spatial anchor.");
            }
        }
    }
}

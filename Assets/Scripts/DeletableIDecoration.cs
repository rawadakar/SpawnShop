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
                
                SpatialAnchorDecorator.RemoveSavedDecoration(uuid);
                Destroy(gameObject);
            }
            else
            {
                
            }
        }
    }
}

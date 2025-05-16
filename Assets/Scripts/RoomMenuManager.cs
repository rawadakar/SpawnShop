using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomMenuManager : MonoBehaviour
{
    public static RoomMenuManager Instance;

    public GameObject roomItemButtonPrefab;
    public Transform roomListContainer;
    public GameObject scrollViewPanel;

    public List<RoomDecorationInfo> activeDecorations = new();
    public System.Action<string> OnDecorationRemovedByUUID;
    void Awake() => Instance = this;

    public void RegisterDecoration(RoomDecorationInfo info)
    {
        activeDecorations.Add(info);
    }

    void CreateButton(RoomDecorationInfo info)
    {
        var btn = Instantiate(roomItemButtonPrefab, roomListContainer);
        btn.GetComponentInChildren<TMP_Text>().text = info.linkedObject.name;

        // Add drag support
        var drag = btn.AddComponent<DraggableUIItem>();
        drag.linkedObject = info.linkedObject;
        drag.uuid = info.uuid;
    }

    public async void RemoveFromRoom(string uuid)
    {
        var match = activeDecorations.Find(x => x.uuid == uuid);
        if (match != null)
        {
            var anchor = match.linkedObject.GetComponent<OVRSpatialAnchor>();
            if (anchor != null)
            {
                bool erased = await anchor.EraseAnchorAsync();
                if (erased)
                {
                    SpatialAnchorDecorator.RemoveSavedDecoration(uuid);
                    Destroy(match.linkedObject); // ✅ finally delete the object in scene
                    activeDecorations.Remove(match);
                    OnDecorationRemovedByUUID?.Invoke(uuid); // 🔔 Notify the spawn menu


                }
                else
                {
                    
                }
            }
            else
            {
                
            }
        }
    }

    public void ToggleRoomView(bool show)
    {
        scrollViewPanel.SetActive(show);
    }

    public List<RoomDecorationInfo> GetActiveDecorations()
    {
        return activeDecorations;
    }
}

﻿using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteZoneHandler : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    public float hoverScale = 1.2f;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only respond during a drag
        if (eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<DraggableUIItem>() != null)
        {
            transform.localScale = originalScale * hoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    public void OnDrop(PointerEventData eventData)
    {
        

        var item = eventData.pointerDrag?.GetComponent<DraggableUIItem>();
        if (item != null)
        {
            

            RoomMenuManager.Instance.RemoveFromRoom(item.uuid);
            Destroy(item.gameObject);
        }
        else
        {
            
        }
    }

}

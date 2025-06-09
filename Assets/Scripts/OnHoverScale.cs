using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] GameObject HoverAnim;
    
    private Vector3 OriginalScale;

    private void Start()
    {
        OriginalScale = HoverAnim.transform.localScale;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        
        HoverAnim.transform.localScale = OriginalScale * 1.2f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HoverAnim.transform.localScale = OriginalScale;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        
        HoverAnim.transform.localScale /= 1.2f;
    }
}

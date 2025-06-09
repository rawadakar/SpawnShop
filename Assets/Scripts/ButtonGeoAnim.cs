using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonGeoAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 OriginalScale;
    GameObject HoverAnim;
    Texture NormalTexture;
    Texture HoverTexture;
    Texture ClickTexture;
    public bool isItem = false;
    void Start()
    {
        NormalTexture = GetComponent<Image>().mainTexture;
        HoverTexture = GetComponent<Button>().spriteState.highlightedSprite.texture;
        ClickTexture = GetComponent<Button>().spriteState.pressedSprite.texture;
        HoverAnim = transform.GetChild(0).gameObject;
        foreach (Transform child in HoverAnim.transform)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = NormalTexture;
            }
        }

        if(isItem == false)
        {
            OriginalScale = HoverAnim.transform.localScale;
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Transform child in HoverAnim.transform)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = HoverTexture;
            }

        }
        if(isItem == false)
        {
            HoverAnim.transform.localScale = new Vector3(OriginalScale.x, OriginalScale.y, OriginalScale.z * 1.5f);
        }
        
    }

    

    public void OnPointerDown(PointerEventData eventData)
    {
        foreach (Transform child in HoverAnim.transform)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = ClickTexture;
            }
        }
        if(isItem == false)
        {
            HoverAnim.transform.localScale = OriginalScale;
        }
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        foreach (Transform child in HoverAnim.transform)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = ClickTexture;
            }
        }
        if(isItem == false)
        {
            HoverAnim.transform.localScale = OriginalScale;
        }
        
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (Transform child in HoverAnim.transform)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = NormalTexture;
            }
        }
        if(isItem == false)
        {
            HoverAnim.transform.localScale = new Vector3(OriginalScale.x, OriginalScale.y, OriginalScale.z / 1.5f);
        }
        
    }
}

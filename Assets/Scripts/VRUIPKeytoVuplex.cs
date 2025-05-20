using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VRUIPKeytoVuplex : MonoBehaviour
{
    private TMP_Text text;
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<TMP_Text>();
        button.onClick.AddListener(SendTextToWebView);
    }

    
    public void SendTextToWebView()
    {
        VRUIPVuplexConnector.Instance.OnVRKeyPressed(text.text);
    }
}

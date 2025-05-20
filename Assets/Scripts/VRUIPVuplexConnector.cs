using UnityEngine;
using TMPro;
using Vuplex.WebView;
using UnityEngine.UI;

public class VRUIPVuplexConnector : MonoBehaviour
{
    public CanvasWebViewPrefab webViewPrefab;
    public TMP_InputField optionalInputField; // leave null if you're typing only in webpage
    public bool mirrorToUnityInput = true;    // whether to also update Unity TMP_InputField
    public static VRUIPVuplexConnector Instance { get; private set; }
    private IWebView webView;
    bool isFocused = true;

    private void Awake()
    {
        Instance = this;
    }
    async void Start()
    {
        
        await webViewPrefab.WaitUntilInitialized();
        webView = webViewPrefab.WebView;
        webView.FocusedInputFieldChanged += (sender, eventArgs) =>
        {
            var shouldShowKeyboard = eventArgs.Type != FocusedInputFieldType.None;
            DeactivateToolBar(shouldShowKeyboard);
        };

        webViewPrefab.Scrolled += (sender, eventArgs) => {
            var scrolled = eventArgs.ScrollDelta != null;
            OnWebviewClicked();
        };

        webViewPrefab.Clicked += (sender, eventArgs) => {
            var scrolled = eventArgs.Point != null;
            OnWebviewClicked();
        };
    }
    //private System.Collections.IEnumerator WaitForWebView()
    //{
    //    while (webViewPrefab.WebView == null)
    //        yield return null;

    //    webView = webViewPrefab.WebView;
    //}
    void OnWebviewClicked()
    {
        isFocused = false;
        optionalInputField.DeactivateInputField();
    }
    public void OnVRKeyPressed(string key)
    {
        if (optionalInputField.isFocused == true)
        {
            isFocused = true;
        }
        
        
        // Mirror key to TMP_InputField (optional)
        if (isFocused == true)
        {
            
            
            if (key == "Backspace")
            {
                if (optionalInputField.text.Length > 0)
                    optionalInputField.text = optionalInputField.text.Substring(0, optionalInputField.text.Length - 1);
            }

            if (key == "Space")
            {
                optionalInputField.text = " ";
            }

            if (key == "Shift")
            {
                optionalInputField.text = key.ToUpper();
            }


            else
            {
                optionalInputField.text += key;
            }
        }

        else
        {
            webView.SendKey(key);
        }
    
    }
    
    void DeactivateToolBar(bool active)
    {
        isFocused = active;

        if (isFocused == true)
        {
            optionalInputField.ActivateInputField();
        }

        else
        {
            optionalInputField.DeactivateInputField();
        }
    }
    
}

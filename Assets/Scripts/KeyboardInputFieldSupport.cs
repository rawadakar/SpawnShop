using UnityEngine;
using UnityEngine.UI;
using Vuplex.WebView;
using TMPro;

class KeyboardInputFieldSupport : MonoBehaviour
{

    public CanvasWebViewPrefab webViewPrefab;
    public CanvasKeyboard keyboard;
    private IWebView webView;
    public bool isFocused = true;
    public TMP_InputField urlInputField;
    private bool isTypingInInputField = false;


    async void Start()
    {
        await keyboard.WaitUntilInitialized();
        keyboard.gameObject.SetActive(false);
        await webViewPrefab.WaitUntilInitialized();
        webViewPrefab.WebView.FocusedInputFieldChanged += (sender, eventArgs) => {
            var show = eventArgs.Type != FocusedInputFieldType.None;
            DisableKeyboard(show);

            EnableWebViewInput(show);
        };
        

            
            keyboard.KeyPressed += OnKeyPressed;
        urlInputField.onSelect.AddListener(_ => StartTyping());



        webViewPrefab.WebView.UrlChanged += (sender, e) =>
        {
            urlInputField.text = e.Url;
        };
    }

    

    public void DisableWebViewInput()
    {
        webViewPrefab.WebView.SetFocused(false);
    }

    public void EnableWebViewInput(bool show)
    {
        
        if (show == true)
        {
            StopTyping();
        }
        
    }

    void StartTyping()
    {
        isTypingInInputField = true;
        keyboard.gameObject.SetActive(true);
        // Block WebView from also typing
        if (webViewPrefab.WebView != null)
            webViewPrefab.WebView.SetFocused(false);
    }
    void OnKeyPressed(object sender, EventArgs<string> e)
    {
        if (!isTypingInInputField) return;

        string key = e.Value;

        if (key == "Backspace")
        {
            if (urlInputField.text.Length > 0)
            {
                urlInputField.text = urlInputField.text.Substring(0, urlInputField.text.Length - 1);
            }
        }
        else if (key == "Enter")
        {
            StopTyping();
        }
        else if (key.Length == 1)
        {
            urlInputField.text += key;
        }
    }

    void StopTyping()
    {
        isTypingInInputField = false;
        //keyboard.gameObject.SetActive(false);
        if (webViewPrefab.WebView != null)
            webViewPrefab.WebView.SetFocused(true);
    }

    void DisableKeyboard(bool show)
    {
        keyboard.gameObject.SetActive(show);
    }
}

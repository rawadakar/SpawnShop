using UnityEngine;
using TMPro;
using Vuplex.WebView;
using System.Collections;

public class WebViewToolbar : MonoBehaviour
{
    public CanvasWebViewPrefab webViewPrefab;
    public TMP_InputField urlInputField;

    void Start()
    {
        StartCoroutine(WaitForWebView());
    }

    private System.Collections.IEnumerator WaitForWebView()
    {
        while (webViewPrefab.WebView == null)
            yield return null;

        webViewPrefab.WebView.LoadUrl("https://www.google.com");
        webViewPrefab.WebView.LoadProgressChanged += OnLoadProgressChanged;
    }

    public void OnGoButtonClicked()
    {
        string input = urlInputField.text.Trim();

        // If input looks like a domain or URL
        if (input.StartsWith("http://") || input.StartsWith("https://") || input.Contains("."))
        {
            // If no http(s), add https
            if (!input.StartsWith("http://") && !input.StartsWith("https://"))
            {
                input = "https://" + input;
            }

            webViewPrefab.WebView.LoadUrl(input);
        }
        else
        {
            // Treat input as search query
            string searchQuery = System.Uri.EscapeDataString(input);
            string googleSearchUrl = $"https://www.google.com/search?q={searchQuery}";
            webViewPrefab.WebView.LoadUrl(googleSearchUrl);
        }
    }

    private void OnLoadProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        if (e.Progress == 1f) // Fully loaded
        {
            urlInputField.text = webViewPrefab.WebView.Url;
            
        }
    }

    public void OnBackButtonClicked()
    {
        
            if (webViewPrefab.WebView.CanGoBack().Result == true)
            {
                webViewPrefab.WebView.GoBack();
            }
        
    }

    public void OnRefreshButtonClicked()
    {
        webViewPrefab.WebView.Reload();
    }

    public void FocusInputField()
    {
        StartCoroutine(ForceCaretVisible());
    }

    private IEnumerator ForceCaretVisible()
    {
        yield return null; // wait one frame
        urlInputField.ActivateInputField();
        urlInputField.caretWidth = 2;
        urlInputField.customCaretColor = true;
        urlInputField.caretColor = Color.white;
    }
}

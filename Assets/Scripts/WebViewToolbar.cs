using UnityEngine;
using TMPro;
using Vuplex.WebView;

public class CanvasWebViewToolbar : MonoBehaviour
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
        webViewPrefab.WebView.UrlChanged += OnUrlChanged;
    }

    public void OnGoButtonClicked()
    {
        string url = urlInputField.text;

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;

        webViewPrefab.WebView.LoadUrl(url);
    }

    private void OnUrlChanged(object sender, UrlChangedEventArgs e)
    {
        urlInputField.text = e.Url;
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
}

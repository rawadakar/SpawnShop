using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class LayoutRefreshHelper : MonoBehaviour
{
    public GridLayoutGroup gridLayout;

    void Awake()
    {
        if (!gridLayout) gridLayout = GetComponent<GridLayoutGroup>();
    }

    public void RefreshLayout()
    {
        StartCoroutine(DoRefresh());
    }

    private IEnumerator DoRefresh()
    {
        if (gridLayout) gridLayout.enabled = false;

        yield return null; // wait 1 frame

        if (gridLayout) gridLayout.enabled = true;

        Canvas.ForceUpdateCanvases();
    }
}

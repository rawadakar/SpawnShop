using UnityEngine;
using System.Collections;

public class MenuToggleController : MonoBehaviour
{
    [Header("Menu Transform")]
    public Transform wristMenuTransform;

    [Header("Animation Settings")]
    public float animationDuration = 0.25f;
    public Vector3 hiddenScale = Vector3.zero;
    public Vector3 visibleScale = Vector3.one;

    private bool isVisible = false;
    private Coroutine currentAnimation;

    void Start()
    {
        // Ensure menu starts fully hidden
        wristMenuTransform.localScale = hiddenScale;
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start)) // Menu (≡) button on left controller
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        isVisible = !isVisible;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(AnimateScale(isVisible));
    }

    IEnumerator AnimateScale(bool show)
    {
        float elapsed = 0f;

        Vector3 startScale = wristMenuTransform.localScale;
        Vector3 targetScale = show ? visibleScale : hiddenScale;

        while (elapsed < animationDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
            wristMenuTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        wristMenuTransform.localScale = targetScale;
    }
}

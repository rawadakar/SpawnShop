using TMPro;
using UnityEngine;

public class VRTextCaretPlacer : MonoBehaviour
{
    public TMP_InputField inputField;

    private Camera uiCamera;

    void Start()
    {
        // Auto-assign the eye camera used for UI raycasting
        if (OVRManager.instance != null)
        {
            var cameraRig = OVRManager.instance.GetComponent<OVRCameraRig>();
            if (cameraRig != null)
            {
                uiCamera = cameraRig.centerEyeAnchor.GetComponent<Camera>();
            }
        }

        if (uiCamera == null)
        {
            Debug.LogError("Could not find a UI camera from OVRManager. Assign manually if needed.");
        }
    }

    public void PlaceCaretAtPointer()
    {
        if (uiCamera == null || inputField == null)
            return;

        Ray ray = new Ray(uiCamera.transform.position, uiCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            if (hit.transform == inputField.textComponent.transform || hit.transform.IsChildOf(inputField.textComponent.transform))
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    inputField.textComponent.rectTransform,
                    uiCamera.WorldToScreenPoint(hit.point),
                    uiCamera,
                    out Vector2 localCursor))
                {
                    int charIndex = TMP_TextUtilities.GetCursorIndexFromPosition(
                        inputField.textComponent,
                        localCursor,
                        uiCamera
                    );

                    if (charIndex != -1)
                    {
                        inputField.ActivateInputField();
                        inputField.stringPosition = charIndex;
                        inputField.caretPosition = charIndex;
                    }
                }
            }
        }
    }
}

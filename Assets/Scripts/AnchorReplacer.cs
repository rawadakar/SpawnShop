using System;
using System.Threading.Tasks;
using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Rigidbody))]
public class AnchorReplacer : MonoBehaviour
{
    private DistanceGrabInteractable interactable;
    private OVRSpatialAnchor currentAnchor;

    private const float AnchorTimeout = 5f;
    private string savedKey => $"anchor_{gameObject.name}_uuid";

    private bool wasBeingGrabbed = false;

    private void Awake()
    {
        interactable = GetComponentInChildren<DistanceGrabInteractable>();
        if (interactable == null)
        {
            
        }
    }

    private async void Start()
    {
        // Wait for OVRSpatialAnchor to be added by another script or system
        float elapsed = 0f;
        while (currentAnchor == null && elapsed < 1f)
        {
            currentAnchor = GetComponent<OVRSpatialAnchor>();
            await Task.Yield();
            elapsed += Time.deltaTime;
        }

        if (currentAnchor != null)
        {
            
        }
        else
        {
            
        }
    }


    private void Update()
    {
        if (interactable == null) return;

        if (interactable.State == InteractableState.Select)
        {
            if (!wasBeingGrabbed)
            {
                wasBeingGrabbed = true;
                OnGrabStart();
            }
        }
        else
        {
            if (wasBeingGrabbed)
            {
                wasBeingGrabbed = false;
                OnGrabReleased();
            }
        }
    }

    private void OnGrabStart()
    {
        if (currentAnchor != null)
        {
            Destroy(currentAnchor);
            currentAnchor = null;
            
        }
    }

    private async void OnGrabReleased()
    {
        await Task.Yield(); // wait a frame to finalize position

        currentAnchor = gameObject.AddComponent<OVRSpatialAnchor>();

        float elapsed = 0f;
        while (currentAnchor == null && elapsed < 1f)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
        }

        if (currentAnchor == null)
        {
            
            return;
        }

        // Wait for localization
        elapsed = 0f;
        while (!currentAnchor.Localized && elapsed < AnchorTimeout)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
        }

        if (!currentAnchor.Localized)
        {
            
            Destroy(currentAnchor);
            currentAnchor = null;
            return;
        }

        bool success = await currentAnchor.SaveAnchorAsync();
        if (success)
        {
            PlayerPrefs.SetString(savedKey, currentAnchor.Uuid.ToString());
            PlayerPrefs.Save();
            
        }
        else
        {
            
        }
    }
}

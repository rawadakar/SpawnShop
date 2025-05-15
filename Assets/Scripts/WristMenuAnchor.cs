using UnityEngine;

public class WristMenuAnchor : MonoBehaviour
{
    public Transform handAnchor;

    void Update()
    {
        if (handAnchor)
        {
            transform.position = handAnchor.position;
            transform.rotation = handAnchor.rotation;
        }
    }
}

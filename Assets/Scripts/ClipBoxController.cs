using System.Collections.Generic;
using UnityEngine;

public class ClipBoxController : MonoBehaviour
{
    public List<Material> targetMaterials; // 👈 Use a list

    void Update()
    {
        Vector3 halfSize = transform.localScale * 0.5f;
        Matrix4x4 worldToLocal = transform.worldToLocalMatrix;

        foreach (var mat in targetMaterials)
        {
            if (mat != null)
            {
                mat.SetVector("_ClipSize", halfSize);
                mat.SetMatrix("_ClipWorldToLocal", transform.worldToLocalMatrix);
            }
        }
    }
}
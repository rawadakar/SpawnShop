using Meta.XR.MRUtilityKit;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void GetCeiling()
    {
        StartCoroutine(GetCeilingCoroutine());
    }

    private IEnumerator GetCeilingCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        GameObject scan = MRUK.Instance.GetCurrentRoom().gameObject;

        foreach (Transform child in scan.transform)
        {


            if (child.name == "GLOBAL_MESH")
            {

                child.GetChild(0).gameObject.layer = LayerMask.NameToLayer("GlobalMesh");

            }




        }
    }
}

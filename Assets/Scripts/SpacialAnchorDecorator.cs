using Oculus.Interaction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SpatialAnchorDecorator : MonoBehaviour
{
    [Serializable]
    public class DecorationData
    {
        public string uuid;
        public string prefabName;
    }

    [Serializable]
    public class Wrapper
    {
        public List<DecorationData> items = new();
    }

    public GameObject decorationPrefab;
    public List<GameObject> allPrefabs;

    private const string SaveKey = "SavedDecorations";

    private void Start()
    {
        LoadDecorations();
    }

    public async void PlaceDecoration(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 1. Create a unique anchor GameObject
        string anchorName = $"Anchor_{prefab.name}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        GameObject anchorGO = new GameObject(anchorName);
        anchorGO.transform.position = position;
        anchorGO.transform.rotation = rotation;

        // 2. Add the spatial anchor
        var anchor = anchorGO.AddComponent<OVRSpatialAnchor>();

        // 3. Instantiate the decoration under the anchor
        GameObject instance = Instantiate(prefab, anchorGO.transform);
        instance.name = prefab.name;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        // 4. Wait for localization
        float timeout = 5f;
        float elapsed = 0f;
        while (!anchor.Localized && elapsed < timeout)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
        }

        if (!anchor.Localized)
        {
            Destroy(anchorGO);
            return;
        }

        // 5. Save the anchor
        bool success = await anchor.SaveAnchorAsync();

        if (success && anchor.Uuid != Guid.Empty)
        {
            var info = anchorGO.AddComponent<RoomDecorationInfo>();
            info.uuid = anchor.Uuid.ToString();
            info.linkedObject = anchorGO;
            info.prefabName = prefab.name;


            RoomMenuManager.Instance.RegisterDecoration(info);
            
            
            SaveNewDecoration(info.uuid, info.prefabName);
            
        }
        else
        {
            Destroy(anchorGO);
        }
    }

    private async void LoadDecorations()
    {
        RoomMenuManager.Instance.ClearDecorations();

        if (!PlayerPrefs.HasKey(SaveKey)) return;

        var wrapper = JsonUtility.FromJson<Wrapper>(PlayerPrefs.GetString(SaveKey));
        List<Guid> guids = new();
        Dictionary<Guid, string> prefabLookup = new();

        foreach (var deco in wrapper.items)
        {
            if (Guid.TryParse(deco.uuid, out Guid id))
            {
                guids.Add(id);
                prefabLookup[id] = deco.prefabName;
            }
        }

        List<OVRSpatialAnchor.UnboundAnchor> buffer = new();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(guids, buffer);

        foreach (var unbound in buffer)
        {
            if (!prefabLookup.TryGetValue(unbound.Uuid, out string prefabName))
                continue;

            GameObject prefab = allPrefabs.Find(p => p.name == prefabName);
            if (prefab == null)
                continue;

            // 1. Create the anchor GameObject
            GameObject anchorGO = new GameObject($"Anchor_{prefabName}_{Guid.NewGuid().ToString().Substring(0, 8)}");
            var anchor = anchorGO.AddComponent<OVRSpatialAnchor>();
            unbound.BindTo(anchor);

            // 2. Instantiate the prefab as a child
            float timeout = 5f;
            float elapsed = 0f;
            while (!anchor.Localized && elapsed < timeout)
            {
                await Task.Yield();
                elapsed += Time.deltaTime;
            }

            if (anchor.Localized)
            {
                // ✅ Instantiate decoration only after the anchor has localized
                GameObject instance = Instantiate(prefab, anchorGO.transform);
                instance.name = prefab.name;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;

                var info = anchorGO.AddComponent<RoomDecorationInfo>();
                info.uuid = anchor.Uuid.ToString();
                info.linkedObject = anchorGO;
                info.prefabName = prefab.name;

                RoomMenuManager.Instance.RegisterDecoration(info);
            }
            else
            {
                Destroy(anchorGO);
            }

            
            while (!anchor.Localized && elapsed < timeout)
            {
                await Task.Yield();
                elapsed += Time.deltaTime;
            }

            if (anchor.Localized)
            {
                var info = anchorGO.AddComponent<RoomDecorationInfo>();
                info.uuid = anchor.Uuid.ToString();
                info.linkedObject = anchorGO;
                info.prefabName = prefab.name;

                RoomMenuManager.Instance.RegisterDecoration(info);
            }
            else
            {
                Destroy(anchorGO);
            }
        }
    }

    public static void RemoveSavedDecoration(string uuid)
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        var wrapper = JsonUtility.FromJson<Wrapper>(PlayerPrefs.GetString(SaveKey));
        wrapper.items.RemoveAll(d => d.uuid == uuid);

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    public static void SaveNewDecoration(string uuid, string prefabName)
    {
        Wrapper wrapper = PlayerPrefs.HasKey(SaveKey)
            ? JsonUtility.FromJson<Wrapper>(PlayerPrefs.GetString(SaveKey))
            : new Wrapper();

        // Replace only if UUID exists, allowing multiple of the same prefab
        wrapper.items.RemoveAll(d => d.uuid == uuid);
        wrapper.items.Add(new DecorationData { uuid = uuid, prefabName = prefabName });

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }
}

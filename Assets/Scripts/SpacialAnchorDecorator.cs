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
        GameObject instance = Instantiate(prefab, position, rotation);
        instance.name = prefab.name; // ✅ Prevent (Clone)
        var anchor = instance.AddComponent<OVRSpatialAnchor>();

        float timeout = 5f;
        float elapsed = 0f;
        while (!anchor.Localized && elapsed < timeout)
        {
            await Task.Yield();
            elapsed += Time.deltaTime;
        }

        if (!anchor.Localized)
        {
            Debug.LogError("Anchor is not locatable after timeout.");
            Destroy(instance);
            return;
        }

        bool success = await anchor.SaveAnchorAsync();

        if (success && anchor.Uuid != Guid.Empty)
        {
            Debug.Log($"[Anchor] Saved UUID: {anchor.Uuid}");

            var info = instance.AddComponent<RoomDecorationInfo>();
            info.uuid = anchor.Uuid.ToString();
            info.linkedObject = instance;
            info.prefabName = prefab.name; // ✅ 👈 Write this here
            RoomMenuManager.Instance.RegisterDecoration(info);

            // Optionally also save the decoration to PlayerPrefs
            SaveNewDecoration(info.uuid, info.prefabName);
        }
        else
        {
            Debug.LogError("Failed to save spatial anchor.");
            Destroy(instance);
        }
    }

    private void SaveDecoration(string uuid, string prefabName)
    {
        Wrapper wrapper = PlayerPrefs.HasKey(SaveKey)
            ? JsonUtility.FromJson<Wrapper>(PlayerPrefs.GetString(SaveKey))
            : new Wrapper();

        wrapper.items.Add(new DecorationData { uuid = uuid, prefabName = prefabName });

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    private async void LoadDecorations()
    {
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
            else
            {
                Debug.LogWarning($"Invalid UUID format: {deco.uuid}");
            }
        }

        List<OVRSpatialAnchor.UnboundAnchor> buffer = new();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(guids, buffer);

        foreach (var unbound in buffer)
        {
            if (!prefabLookup.TryGetValue(unbound.Uuid, out string prefabName))
            {
                Debug.LogWarning($"Prefab name not found for anchor UUID: {unbound.Uuid}");
                continue;
            }

            GameObject prefab = allPrefabs.Find(p => p.name == prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab '{prefabName}' not found in allPrefabs list.");
                continue;
            }

            GameObject instance = Instantiate(prefab);
            var anchor = instance.AddComponent<OVRSpatialAnchor>();
            unbound.BindTo(anchor);

            float timeout = 5f;
            float elapsed = 0f;
            while (!anchor.Localized && elapsed < timeout)
            {
                await Task.Yield();
                elapsed += Time.deltaTime;
            }

            if (anchor.Localized)
            {
                var info = instance.AddComponent<RoomDecorationInfo>();
                info.uuid = anchor.Uuid.ToString();
                info.linkedObject = instance;
                RoomMenuManager.Instance.RegisterDecoration(info);

                // Optional: Update spawn count logic here if needed
                // SpawnLimiter.RegisterSpawn(prefabName);
            }
            else
            {
                Debug.LogWarning($"Anchor for prefab '{prefabName}' failed to localize.");
                Destroy(instance);
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

        Debug.Log($"Removed saved decoration with UUID: {uuid}");
    }

    public static void SaveNewDecoration(string uuid, string prefabName)
    {
        Wrapper wrapper = PlayerPrefs.HasKey(SaveKey)
            ? JsonUtility.FromJson<Wrapper>(PlayerPrefs.GetString(SaveKey))
            : new Wrapper();

        wrapper.items.Add(new DecorationData { uuid = uuid, prefabName = prefabName });

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }
}

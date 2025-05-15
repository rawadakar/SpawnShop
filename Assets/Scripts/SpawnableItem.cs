using UnityEngine;

[CreateAssetMenu(menuName = "Sandbox/Spawnable Item", fileName = "NewSpawnableItem")]
public class SpawnableItem : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public GameObject prefab;
    public Sprite icon;

    [Header("Classification")]
    public string category; // e.g. "Primitive", "Vehicle", etc.
    public bool isPaid;

    [Header("Spawn Limits")]
    public int maxSpawnCount = -1; // -1 means unlimited

    /// <summary>
    /// Returns the prefab's name or empty if not assigned.
    /// Used to match against loaded decorations in scene.
    /// </summary>
    public string PrefabName => prefab != null ? prefab.name : "";

    /// <summary>
    /// Whether this item has a limit on how many times it can be spawned.
    /// </summary>
    public bool IsLimited() => maxSpawnCount >= 0;
}

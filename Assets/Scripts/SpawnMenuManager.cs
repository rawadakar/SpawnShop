using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform categoryContainer;
    public Transform itemContainer;
    public GameObject categoryButtonPrefab;
    public GameObject itemButtonPrefab;

    [Header("Grid Layout Helpers")]
    public LayoutRefreshHelper categoryGridHelper;
    public LayoutRefreshHelper itemGridHelper;

    [Header("Item Database")]
    public List<SpawnableItem> allItems;

    [Header("Anchor Decorator")]
    public SpatialAnchorDecorator anchorDecorator;

    private Dictionary<string, List<SpawnableItem>> categorizedItems = new();
    private Dictionary<SpawnableItem, int> spawnCounts = new();
    private Dictionary<SpawnableItem, Button> itemButtons = new();
    private Dictionary<SpawnableItem, TMP_Text> itemLabels = new();

    private string currentCategory = "";

    void Start()
    {

        RoomMenuManager.Instance.OnDecorationRemovedByUUID += HandleDecorationRemoved;
        spawnCounts.Clear();
        itemButtons.Clear();
        itemLabels.Clear();
        BuildMenu();

        if (categorizedItems.Count > 0)
        {
            foreach (var category in categorizedItems.Keys)
            {
                ShowItemsForCategory(category);
                break;
            }
        }
    }

    void BuildMenu()
    {
        ClearChildren(categoryContainer);
        ClearChildren(itemContainer);
        categorizedItems.Clear();

        foreach (var item in allItems)
        {
            if (item == null || item.prefab == null || item.prefab.name.EndsWith("(Clone)"))
                continue;

            if (!categorizedItems.ContainsKey(item.category))
                categorizedItems[item.category] = new List<SpawnableItem>();

            categorizedItems[item.category].Add(item);
        }

        foreach (var category in categorizedItems.Keys)
        {
            GameObject categoryButton = Instantiate(categoryButtonPrefab, categoryContainer);
            categoryButton.GetComponentInChildren<TMP_Text>().text = category;

            categoryButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowItemsForCategory(category);
            });
        }

        GameObject roomBtn = Instantiate(categoryButtonPrefab, categoryContainer);
        roomBtn.GetComponentInChildren<TMP_Text>().text = "Room";
        roomBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ShowRoomItems();
        });
    }

    void ShowItemsForCategory(string category)
    {
        currentCategory = category;

        ClearChildren(itemContainer);
        itemButtons.Clear();
        itemLabels.Clear();

        foreach (var item in categorizedItems[category])
        {
            if (item.prefab == null || item.prefab.name.EndsWith("(Clone)"))
                continue;

            GameObject itemButtonObj = Instantiate(itemButtonPrefab, itemContainer);

            // Set icon
            var iconTransform = itemButtonObj.transform.Find("Icon");
            if (iconTransform)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage && item.icon)
                {
                    iconImage.sprite = item.icon;
                    iconImage.enabled = true;
                }
            }

            TMP_Text buttonText = itemButtonObj.GetComponentInChildren<TMP_Text>();
            Button button = itemButtonObj.GetComponent<Button>();

            itemButtons[item] = button;
            itemLabels[item] = buttonText;

            if (!spawnCounts.ContainsKey(item))
            {
                int activeInScene = RoomMenuManager.Instance.activeDecorations
                    .FindAll(x => x.linkedObject != null && x.linkedObject.name.StartsWith(item.PrefabName)).Count;

                spawnCounts[item] = activeInScene;
            }

            UpdateButtonState(item);

            if (!item.isPaid)
            {
                button.onClick.AddListener(() => TrySpawnItem(item));
            }
        }

        itemGridHelper?.RefreshLayout();
    }

    void TrySpawnItem(SpawnableItem item)
    {
        if (!spawnCounts.ContainsKey(item))
            spawnCounts[item] = 0;

        bool unlimited = item.maxSpawnCount < 0;
        bool canSpawn = unlimited || spawnCounts[item] < item.maxSpawnCount;

        if (canSpawn)
        {
            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            Quaternion spawnRot = Quaternion.identity;

            if (item.category == "Decoration")
            {
                anchorDecorator.PlaceDecoration(item.prefab, spawnPos, spawnRot);
            }
            else
            {
                Instantiate(item.prefab, spawnPos, spawnRot);
            }

            spawnCounts[item]++;

            UpdateButtonState(item);

            // Refresh the UI to reflect new spawn state
            if (currentCategory == item.category)
                ShowItemsForCategory(currentCategory);
        }
        else
        {
            Debug.Log($"Spawn limit reached for: {item.itemName}");
        }
    }

    void UpdateButtonState(SpawnableItem item)
    {
        if (!itemLabels.ContainsKey(item) || !itemButtons.ContainsKey(item)) return;

        int currentCount = spawnCounts.ContainsKey(item) ? spawnCounts[item] : 0;
        int spawnsLeft = item.maxSpawnCount < 0 ? -1 : item.maxSpawnCount - currentCount;
        string countText = spawnsLeft < 0 ? "Unlimited" : $"{spawnsLeft} left";

        string displayText = item.isPaid
            ? $"{item.itemName} 🔒"
            : $"{item.itemName} ({countText})";

        itemLabels[item].text = displayText;

        bool canSpawn = spawnsLeft != 0;
        itemButtons[item].interactable = !item.isPaid && canSpawn;
    }

    void ClearChildren(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        if (container == categoryContainer)
            categoryGridHelper?.RefreshLayout();
        else if (container == itemContainer)
            itemGridHelper?.RefreshLayout();
    }

    public void ShowRoomItems()
    {
        currentCategory = "Room";

        ClearChildren(itemContainer);
        itemButtons.Clear();
        itemLabels.Clear();

        foreach (var deco in RoomMenuManager.Instance.GetActiveDecorations())
        {
            var btn = Instantiate(itemButtonPrefab, itemContainer);

            btn.GetComponentInChildren<TMP_Text>().text = deco.prefabName;

            var drag = btn.AddComponent<DraggableUIItem>();
            drag.linkedObject = deco.linkedObject;
            drag.uuid = deco.uuid;
        }

        itemGridHelper?.RefreshLayout();
    }

    void HandleDecorationRemoved(string uuid)
    {
        foreach (var item in allItems)
        {
            if (item.category == "Decoration" && RoomMenuManager.Instance.GetActiveDecorations()
                .Find(x => x.uuid == uuid && x.linkedObject.name.StartsWith(item.PrefabName)) == null)
            {
                if (spawnCounts.ContainsKey(item) && item.maxSpawnCount > 0)
                {
                    spawnCounts[item] = Mathf.Max(0, spawnCounts[item] - 1);
                    UpdateButtonState(item);
                }
            }
        }

        // Refresh decorations view if it's the current category
        if (currentCategory == "Decoration")
            ShowItemsForCategory("Decoration");
    }
}

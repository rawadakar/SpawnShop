using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction;

public class SpawnMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform categoryContainer;
    public Transform itemContainer;
    public GameObject categoryButtonPrefab;
    public GameObject itemButtonPrefab;
    public GameObject roomItemButtonPrefab;

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

            Image iconImage = itemButtonObj.GetComponent<Image>();
            if (iconImage && item.icon)
            {
                iconImage.sprite = item.icon;
                iconImage.enabled = true;
            }

            TMP_Text buttonText = itemButtonObj.GetComponentInChildren<TMP_Text>();
            Button button = itemButtonObj.GetComponent<Button>();

            itemButtons[item] = button;
            itemLabels[item] = buttonText;

            if (!spawnCounts.ContainsKey(item))
            {
                int activeInScene = RoomMenuManager.Instance.GetActiveDecorations()
                    .FindAll(x => x.prefabName == item.PrefabName).Count;

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

            if (currentCategory == item.category)
                ShowItemsForCategory(currentCategory);
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

    void ShowRoomItems()
    {
        currentCategory = "Room";

        ClearChildren(itemContainer);
        itemButtons.Clear();
        itemLabels.Clear();

        foreach (var deco in RoomMenuManager.Instance.GetActiveDecorations())
        {
            GameObject btn = Instantiate(roomItemButtonPrefab, itemContainer);
            btn.GetComponentInChildren<TMP_Text>().text = deco.prefabName;

            // ✅ Set icon from spawnable item
            var spawnable = allItems.Find(x => x.PrefabName == deco.prefabName);
            if (spawnable != null)
            {
                Image iconImage = btn.GetComponent<Image>();
                if (iconImage && spawnable.icon)
                {
                    iconImage.sprite = spawnable.icon;
                    iconImage.enabled = true;
                }
            }

            // Drag-to-delete
            var drag = btn.AddComponent<DraggableUIItem>();
            drag.linkedObject = deco.linkedObject;
            drag.uuid = deco.uuid;

            // Lock button setup
            Transform lockBtn = btn.transform.Find("LockButton");
            if (lockBtn != null)
            {
                Button lockButton = lockBtn.GetComponent<Button>();
                TMP_Text lockText = lockBtn.GetComponentInChildren<TMP_Text>();
                bool locked = false;

                lockButton.onClick.AddListener(() =>
                {
                    var grab = deco.linkedObject.transform.GetComponentInChildren<Grabbable>().transform.gameObject;
                    if (grab) grab.gameObject.SetActive(!locked);
                    locked = !locked;
                    lockText.text = locked ? "L" : "U";
                });
            }
        }

        itemGridHelper?.RefreshLayout();
    }

    void HandleDecorationRemoved(string uuid)
    {
        foreach (var item in allItems)
        {
            if (item.category == "Decoration" && RoomMenuManager.Instance.GetActiveDecorations()
                .Find(x => x.uuid == uuid && x.prefabName == item.PrefabName) == null)
            {
                if (spawnCounts.ContainsKey(item) && item.maxSpawnCount > 0)
                {
                    spawnCounts[item] = Mathf.Max(0, spawnCounts[item] - 1);
                    UpdateButtonState(item);
                }
            }
        }

        if (currentCategory == "Decoration")
            ShowItemsForCategory("Decoration");
    }
}

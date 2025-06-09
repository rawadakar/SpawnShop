using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.UI;

public class SpawnMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform categoryContainer;
    public Transform itemContainer;
    public GameObject categoryButtonPrefab;
    public GameObject itemButtonPrefab;
    public GameObject roomItemButtonPrefab;
    public Button roomBtn;
    [Header("Grid Layout Helpers")]
    public LayoutRefreshHelper categoryGridHelper;
    public LayoutRefreshHelper itemGridHelper;

    [Header("Item Database")]
    public List<SpawnableItem> allItems;
    public string currentSubCategory = "";
    [Header("Anchor Decorator")]
    public SpatialAnchorDecorator anchorDecorator;

    public Dictionary<string, List<SpawnableItem>> categorizedItems = new();
    private Dictionary<SpawnableItem, int> spawnCounts = new();
    private Dictionary<SpawnableItem, Button> itemButtons = new();
    private Dictionary<SpawnableItem, TMP_Text> itemLabels = new();
    public static SpawnMenuManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;
        roomBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            //currentCategory = "Decoration";
            ShowRoomItems();

        });
    }
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

            if (!categorizedItems.ContainsKey(item.subCategory))
                categorizedItems[item.subCategory] = new List<SpawnableItem>();

            categorizedItems[item.subCategory].Add(item);
        }

        foreach (var subCategory in categorizedItems.Keys)
        {
            GameObject categoryButton = Instantiate(categoryButtonPrefab, categoryContainer);
            categoryButton.layer = LayerMask.NameToLayer("StencilButton1");
            Transform[] element = categoryButton.GetComponentsInChildren<Transform>();
            foreach (Transform t in element)
            {
                if (t != null)
                {
                    t.gameObject.layer = LayerMask.NameToLayer("StencilButton1");
                }

            }
            
            
            categoryButton.GetComponentInChildren<TMP_Text>().text = subCategory;

            categoryButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowItemsForCategory(subCategory);
            });
        }

        

    }

    public void ShowItemsForCategory(string subCategory)
    {
        currentSubCategory = subCategory;

        ClearChildren(itemContainer);
        itemButtons.Clear();
        itemLabels.Clear();

        foreach (var item in categorizedItems[subCategory])
        {
            if (item.prefab == null || item.prefab.name.EndsWith("(Clone)"))
                continue;

            GameObject itemButtonObj = Instantiate(itemButtonPrefab, itemContainer);
            itemButtonObj.layer = LayerMask.NameToLayer("StencilButton1");
            Transform[] element = itemButtonObj.GetComponentsInChildren<Transform>();
            foreach (Transform t in element)
            {
                if (t != null)
                {
                    t.gameObject.layer = LayerMask.NameToLayer("StencilButton1");
                }

            }

            
            // ✅ Apply icon to child named "Icon"
            Transform iconChild = itemButtonObj.transform.GetChild(0).transform.Find("Icon");
            if (iconChild != null)
            {
                Debug.Log("Image Found");
                Image iconImage = iconChild.GetComponent<Image>();
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

            if (currentSubCategory == item.subCategory)
                ShowItemsForCategory(currentSubCategory);
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

        //itemLabels[item].text = displayText;

        bool canSpawn = spawnsLeft != 0;
        itemButtons[item].interactable = !item.isPaid && canSpawn;
        if (!canSpawn)
        {
            Image img = itemButtons[item].transform.Find("Icon").GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(1, 1, 1, 0.4f);
            }
        }
        
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
        currentSubCategory = "Room";

        ClearChildren(itemContainer);
        itemButtons.Clear();
        itemLabels.Clear();

        foreach (var deco in RoomMenuManager.Instance.GetActiveDecorations())
        {
            GameObject btn = Instantiate(roomItemButtonPrefab, itemContainer);
            btn.layer = LayerMask.NameToLayer("StencilButton1");
            Transform[] element = btn.GetComponentsInChildren<Transform>();
            foreach (Transform child in element)
            {
                child.gameObject.layer = LayerMask.NameToLayer("StencilButton1");
            }
            //btn.GetComponentInChildren<TMP_Text>().text = "";
            Debug.Log(deco.transform.GetChild(0).name);
            // ✅ Set icon from spawnable item on child named "Icon"
            var spawnable = allItems.Find(x => x.itemName == deco.transform.GetChild(0).name);
            if (spawnable != null)
            {
                Debug.Log("spawnable Found");
                Transform iconChild = btn.transform.GetChild(0).transform.Find("Icon");
                if (iconChild != null)
                {
                    Debug.Log("Image Found");
                    Image iconImage = iconChild.GetComponent<Image>();
                    if (iconImage && spawnable.icon)
                    {
                        iconImage.sprite = spawnable.icon;
                        iconImage.enabled = true;
                    }
                }
            }
            else
            {
                Debug.Log("spawnable NOT Found");
            }

            var drag = btn.AddComponent<DraggableUIItem>();
            drag.uuid = deco.uuid; // ✅ Only store uuid
            drag.ID = deco.ID;

            Transform lockBtn = btn.transform.Find("LockButton");
            if (lockBtn != null)
            {
                Button lockButton = lockBtn.GetComponent<Button>();
                GameObject lockIcon = lockBtn.Find("LockButton").gameObject;
                GameObject UnlockIcon = lockBtn.Find("UnlockButton").gameObject;

                // 🔄 Load and apply initial lock state
                bool locked = LoadLockState(drag.uuid);

                var updatedDeco = RoomMenuManager.Instance.GetActiveDecorations()
                    .Find(x => x.uuid == drag.uuid);

                if (updatedDeco?.linkedObject != null && updatedDeco.linkedObject.transform.childCount > 0)
                {
                    Transform root = updatedDeco.linkedObject.transform;
                    var grab = root.GetChild(0).Find("GrabCollider");
                    var placer = root.GetChild(0).Find("Placer");

                    if (grab != null)
                        grab.gameObject.SetActive(!locked);
                    if (placer != null)
                        placer.gameObject.SetActive(!locked);
                }

                lockIcon.SetActive(locked);
                UnlockIcon.SetActive(!locked);

                // 🔘 Lock button logic
                lockButton.onClick.AddListener(() =>
                {
                    var refreshedDeco = RoomMenuManager.Instance.GetActiveDecorations()
                        .Find(x => x.ID == drag.ID);

                    if (refreshedDeco?.ID == null)
                    {
                        
                        return;
                    }

                    Transform root = refreshedDeco.linkedObject.transform;
                    var grab = root.GetChild(0).Find("GrabCollider");
                    var placer = root.GetChild(0).Find("Placer");

                    if (grab != null && placer != null)
                    {
                        bool currentLockState = LoadLockState(drag.uuid);
                        bool newLockState = !currentLockState;

                        grab.gameObject.SetActive(!newLockState);
                        placer.gameObject.SetActive(!newLockState);
                        

                        lockIcon.SetActive(newLockState);
                        UnlockIcon.SetActive(!newLockState);

                        SaveLockState(drag.uuid, newLockState);
                    }
                });
            }
        }

        itemGridHelper?.RefreshLayout();
    }



    void SaveLockState(string uuid, bool isLocked)
    {
        PlayerPrefs.SetInt($"LockState_{uuid}", isLocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    bool LoadLockState(string uuid)
    {
        return PlayerPrefs.GetInt($"LockState_{uuid}", 0) == 1;
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

        if (currentSubCategory == "Decoration")
            ShowItemsForCategory("Decoration");
    }
}

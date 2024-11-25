using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {

    public static readonly int InventoryWidth = 8;
    public static readonly int InventoryHeight = 4;

    public Inventory inventory;
    public int selectedBlockIndex = 0;
    public Sprite slot;
    public Sprite nothing;
    public GameObject inventoryBackGround;
    public GameObject toolbarBackGround;
    public GameObject toolbar;
    public GameObject inventoryObj;
    public World world;
    public RectTransform highlight;
    public readonly Image[] images = new Image[InventoryWidth * InventoryHeight];
    public readonly Image[] toolbarImages = new Image[InventoryWidth];
    public int slotIndex = 0;
    public bool inUI;

    [SerializeField] public Text blockName;
    [SerializeField] public Hand hand;

    void Start () {

        hand = GameObject.Find("Hand").GetComponent<Hand>();

        inventory.AddItemStackToInventory(new(256, 1));
        inventory.AddItemStackToInventory(new(257, 1));
        inventory.AddItemStackToInventory(new(258, 1));
        inventory.AddItemStackToInventory(new(259, 1));
        inventory.AddItemStackToInventory(new(0, 4));
        inventory.AddItemStackToInventory(new(1, 4));
        inventory.AddItemStackToInventory(new(2, 4));
        inventory.AddItemStackToInventory(new(3, 4));

        Cursor.lockState = CursorLockMode.Locked;
        SetSprites();
        inventoryBackGround.SetActive(false);
        inventoryObj.SetActive(false);
        highlight.position = toolbarImages[0].transform.position;
        RenderSprites();
        selectedBlockIndex = GetSelected();
    }
    private int GetSelected () {
        return 1;
    }
    private void Update () {


        Scroll();
        SetSlidebarValues();

        if (Input.GetKeyDown(KeyCode.E)) {
            inUI = false;
            inventoryBackGround.SetActive(false);
            inventoryObj.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    private void SetSlidebarValues () {
    }
    private void Scroll () {
        float value = Input.GetAxis("Mouse ScrollWheel");
        if (value != 0) {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
                slotIndex--;
            else
                slotIndex++;

            if (slotIndex >= InventoryWidth)
                slotIndex = 0;
            else if (slotIndex < 0)
                slotIndex = InventoryWidth - 1;

            highlight.position = toolbarImages[slotIndex].transform.position;
            selectedBlockIndex = GetSelected();
        }
    }
    private void SetSprites () {

        int f = 0;

        for (int x = 0; x < InventoryWidth; x++) {
            for (int y = 0; y < InventoryHeight; y++) {


                Vector2 pos = new Vector2(100 * x - 450, -100 * y + 10) + new Vector2(1060, 540);

                GameObject obj = new();
                obj.AddComponent<Image>().sprite = slot;
                obj.layer = 5;
                obj.transform.SetParent(inventoryBackGround.transform);
                obj.GetComponent<RectTransform>().position = pos;

                GameObject a = new();
                images[f] = a.AddComponent<Image>();
                images[f].sprite = nothing;
                a.layer = 5;
                a.transform.SetParent(inventoryObj.transform);
                a.GetComponent<RectTransform>().position = pos;
                a.GetComponent<RectTransform>().sizeDelta *= 0.8f;

                f++;
            }
        }
        for (int i = 0; i < InventoryWidth; i++) {


            Vector2 pos = new Vector2(100 * i - 450, -500 + 10) + new Vector2(1060, 540);

            GameObject obj = new();
            obj.AddComponent<Image>().sprite = slot;
            obj.layer = 5;
            obj.transform.SetParent(toolbarBackGround.transform);
            obj.GetComponent<RectTransform>().position = pos;

            GameObject a = new();
            toolbarImages[i] = a.AddComponent<Image>();
            toolbarImages[i].sprite = nothing;
            a.layer = 5;
            a.transform.SetParent(toolbar.transform);
            a.GetComponent<RectTransform>().position = pos;
            a.GetComponent<RectTransform>().sizeDelta *= 0.8f;
        }
    }
    private void RenderSprites () {

        for (int i = 0; i < InventoryWidth * InventoryHeight; i++) {

            Sprite a = nothing;

            if (inventory.GetItemStack(i) != null) {

                a = world.itemTypes[Mathf.Max(inventory.GetItemStack(i).id - 256, 0)].sprite;

                if (i < 8) {
                    toolbarImages[i].sprite = a;
                }

            }
            images[i].sprite = a;
        }
    }
}

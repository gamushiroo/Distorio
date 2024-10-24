using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {

    public Sprite slot;
    public Sprite nothing;
    public GameObject inventoryBackGround;
    public GameObject toolbarBackGround;
    public GameObject toolbar;
    public GameObject inventoryObj;
    public Player player;
    public World world;
    public RectTransform highlight;
    public readonly Image[] images = new Image[Data.InventoryWidth * Data.InventoryHeight];
    public readonly Image[] toolbarImages = new Image[Data.InventoryWidth];
    public int slotIndex = 0;
    public Slider hpBar;
    public Text hpText;
    public bool inUI;

    [SerializeField] public Text blockName;
    [SerializeField] public Hand hand;

    private void Start () {

        hand = GameObject.Find("Hand").GetComponent<Hand>();

        player.inventory.AddItemStackToInventory(new ItemStack(256, 1));
        player.inventory.AddItemStackToInventory(new ItemStack(257, 1));
        player.inventory.AddItemStackToInventory(new ItemStack(258, 1));
        player.inventory.AddItemStackToInventory(new ItemStack(259, 1));

        Cursor.lockState = CursorLockMode.Locked;
        SetSprites();
        inventoryBackGround.SetActive(false);
        inventoryObj.SetActive(false);
        highlight.position = toolbarImages[0].transform.position;

        RenderSprites();
        player.selectedBlockIndex = GetSelected();

    }

    private int GetSelected () {

        return player.inventory.GetItemStack(slotIndex).id;


    }
    private void Update () {

        Scroll();
        SetSlidebarValues();

        if (!inUI) {
            if (Input.GetMouseButtonDown(1) && world.GetInventory(player.SelectingPos)) {
                inUI = true;
                inventoryBackGround.SetActive(true);
                inventoryObj.SetActive(true);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                player.locked = true;

            } else if (Input.GetMouseButtonDown(1) && !world.blockTypes[world.GetVoxelID(player.tryPlacingPos)].hasCollision && player.selectedBlockIndex < 128 && world.blockTypes[world.GetVoxelID(player.SelectingPos)].hasCollision) {

                player.SetBBBL(player.tryPlacingPos, player.selectedBlockIndex);
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            inUI = false;
            inventoryBackGround.SetActive(false);
            inventoryObj.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            player.locked = false;
        }
    }
    private void SetSlidebarValues () {
        hpBar.value = player.entity.Health / Data.player.health;
        hpText.text = Mathf.FloorToInt(player.entity.Health).ToString("#,#");
    }
    private void Scroll () {
        float value = Input.GetAxis("Mouse ScrollWheel");
        if (value != 0) {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
                slotIndex--;
            else
                slotIndex++;

            if (slotIndex >= Data.InventoryWidth)
                slotIndex = 0;
            else if (slotIndex < 0)
                slotIndex = Data.InventoryWidth - 1;

            int a = GetSelected();
            player.selectedBlockIndex = a;
            if (a < 128) {
                blockName.text = world.blockTypes[a].blockName;
                hand.GenerateMesh(a);
            } else {
                blockName.text = ItemRegistry.GetItem(a).GetName();
                hand.GenerateMesh(0);
            }
            highlight.position = toolbarImages[slotIndex].transform.position;
            player.selectedBlockIndex = GetSelected();
        }
    }
    private void SetSprites () {

        int f = 0;

        for (int x = 0; x < Data.InventoryWidth; x++) {
            for (int y = 0; y < Data.InventoryHeight; y++) {


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
        for (int i = 0; i < Data.InventoryWidth; i++) {


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

        for (int i = 0; i < Data.InventoryWidth * Data.InventoryHeight; i++) {

            Sprite a = nothing;

            if(player.inventory.GetItemStack(i) != null) {

                a = world.itemTypes[player.inventory.GetItemStack(i).id - 256].sprite;

            }

            if(i < 8) {
                toolbarImages[i].sprite = a;
            }
            images[i].sprite = a;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Inventory {
    private readonly ItemStack[] itemStacks = new ItemStack[Data.inventoryX * Data.inventoryY];
    public readonly Image[] images = new Image[Data.inventoryX * Data.inventoryY];
    public readonly Image[] backs = new Image[Data.inventoryX * Data.inventoryY];
    World world;
    bool isOpened;
    public int slotIndex = 0;

    public Inventory (Toolbar toolbar, World world, Transform back) {
        this.world = world;
        int index = 0;
        for (int l = 0; l < Data.inventoryY; l++) {
            for (int i = 0; i < Data.inventoryX; i++) {
                itemStacks[index] = new(0, 0);
                GameObject gam = new();
                images[index] = gam.AddComponent<Image>();
                gam.layer = 5;
                gam.transform.SetParent(toolbar.transform);
                if (l == 0) {
                    gam.GetComponent<RectTransform>().position = new Vector2(100 * i - 450, -500 + 10) + new Vector2(960, 540);
                } else {
                    gam.GetComponent<RectTransform>().position = new Vector2(100 * i - 450, -100 * l + 10) + new Vector2(960, 540);
                }
                index++;
            }
        }
        index = 0;
        for (int l = 0; l < Data.inventoryY; l++) {
            for (int i = 0; i < Data.inventoryX; i++) {
                GameObject gam = new();
                backs[index] = gam.AddComponent<Image>();
                backs[index].sprite = world.slot;
                gam.layer = 5;
                gam.transform.SetParent(back.transform);
                if (l == 0) {
                    gam.GetComponent<RectTransform>().position = new Vector2(100 * i - 450, -500 + 10) + new Vector2(960, 540);
                } else {
                    gam.GetComponent<RectTransform>().position = new Vector2(100 * i - 450, -100 * l + 10) + new Vector2(960, 540);
                }
                index++;
            }
        }

        Switch(false);
    }
    public ItemStack GetSelected () {
        return itemStacks[slotIndex];
    }
    public void Scroll (float value) {
        if (value > 0)
            slotIndex--;
        else
            slotIndex++;
        if (slotIndex > Data.inventoryX - 1)
            slotIndex = 0;
        if (slotIndex < 0)
            slotIndex = Data.inventoryX - 1;
    }
    public void SetItem (int index, ItemStack itemStack) {
        itemStacks[index] = itemStack;
        RenderItems();
    }
    public void RenderItems () {
        for (int i = 0; i < Data.inventoryX * Data.inventoryY; i++) {
            byte id = itemStacks[i].id;
            if (id < 128) {
                images[i].sprite = world.blockTypes[id].sprite;
            } else {
                images[i].sprite = world.itemTypes[id - 128].sprite;
            }

            if (i < Data.inventoryX) {
                images[i].enabled = itemStacks[i].id != 0;
            } else {

                backs[i].enabled = isOpened;
                images[i].enabled = itemStacks[i].id != 0 && isOpened;
            }
        }
    }
    public void Switch (bool value) {
        isOpened = value;
        RenderItems();
    }
}

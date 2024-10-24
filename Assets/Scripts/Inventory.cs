public class Inventory {

    private readonly ItemStack[] inventory = new ItemStack[32];
    public int currentItem;


    public Inventory () {
        Instantiate();

    }

    public ItemStack GetCurrentItem () {
        return this.currentItem < 9 && this.currentItem >= 0 ? this.inventory[this.currentItem] : null;
    }

    public ItemStack GetItemStack (int i) {
        return inventory[i];

    }

    private void Instantiate () {
        for (int i = 0; i < 32; i++) {
            inventory[i] = null;
        }
    }

    public bool AddItemStackToInventory (ItemStack itemStackIn) {
        int i;

        while (true) {
            i = itemStackIn.stackSize;
            itemStackIn.stackSize = StorePartialItemStack(itemStackIn);

            if (itemStackIn.stackSize <= 0 || itemStackIn.stackSize >= i) {
                break;
            }
        }

        return itemStackIn.stackSize < i;
    }
    private int StorePartialItemStack (ItemStack itemStackIn) {
        int item = itemStackIn.GetItemID();
        int i = itemStackIn.stackSize;
        int j = StoreItemStack(itemStackIn);
        if (j < 0) {
            j = GetFirstEmptyStack();
        }
        if (j < 0) {
            return i;
        } else {
            if (inventory[j] == null) {
                inventory[j] = new ItemStack(item, 0);
            }
            int k = i;
            if (i > inventory[j].GetMaxStackSize() - inventory[j].stackSize) {
                k = inventory[j].GetMaxStackSize() - inventory[j].stackSize;
            }
            if (k == 0) {
                return i;
            } else {
                i -= k;
                inventory[j].stackSize += k;
                return i;
            }
        }
    }
    private int StoreItemStack (ItemStack itemStackIn) {
        for (int i = 0; i < inventory.Length; ++i) {
            if (inventory[i] != null
                && inventory[i].GetItemID() == itemStackIn.GetItemID()
                && inventory[i].IsStackable()
                && inventory[i].stackSize < inventory[i].GetMaxStackSize())
                return i;
        }
        return -1;
    }
    public int GetFirstEmptyStack () {
        for (int i = 0; i < inventory.Length; ++i) {
            if (inventory[i] == null) {
                return i;
            }
        }
        return -1;
    }
}
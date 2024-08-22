using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour {

    [SerializeField] private World world;
    [SerializeField] private Hand hand;
    [SerializeField] private Player player;
    [SerializeField] private ItemSlot[] itemSlots;
    [SerializeField] private RectTransform highlight;
    [SerializeField] private Text blockName;

    private int slotIndex = 0;

    private void Start () {

        world = GameObject.Find("World").GetComponent<World>();
        hand = GameObject.Find("Hand").GetComponent<Hand>();

        foreach (ItemSlot slot in itemSlots) {

            slot.icon.sprite = world.blockTypes[slot.itemID].sprite;
            slot.icon.enabled = true;

        }
    }

    private void Update () {

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0) {

            if (scroll > 0)
                slotIndex--;
            else
                slotIndex++;

            if (slotIndex > itemSlots.Length - 1)
                slotIndex = 0;
            if (slotIndex < 0)
                slotIndex = itemSlots.Length - 1;

            highlight.position = itemSlots[slotIndex].icon.transform.position;
            player.selectedBlockIndex = itemSlots[slotIndex].itemID;
            hand.GenerateMesh(itemSlots[slotIndex].itemID);
            blockName.text = world.blockTypes[itemSlots[slotIndex].itemID].blockName;

        }
    }
}

[System.Serializable]
public class ItemSlot {

    public byte itemID;
    public Image icon;

}
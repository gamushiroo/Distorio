using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour {

    [SerializeField] private World world;
    [SerializeField] private Hand hand;
    [SerializeField] private Player player;

    [SerializeField] private RectTransform highlight;
    [SerializeField] private Text blockName;

    public GameObject background;
    public  Inventory inventory ;

    private void Start () {

        world = GameObject.Find("World").GetComponent<World>();
        hand = GameObject.Find("Hand").GetComponent<Hand>();

        inventory = new(this, world, background.transform);
        inventory.SetItem(0, new(131, 1));
        inventory.SetItem(1, new(6, 1));

        inventory.SetItem(2, new(9, 1));
        inventory.SetItem(30, new(5, 1));
        highlight.position = inventory.images[0].transform.position;

    }

    private void Update () {

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0) {
            inventory.Scroll(scroll);
            ItemStack a = inventory.GetSelected();
            highlight.position = inventory.images[inventory.slotIndex].transform.position;
            player.selectedBlockIndex = a.id;

            if (a.id < 128) {
                blockName.text = world.blockTypes[a.id].blockName;
                hand.GenerateMesh(a.id);
            } else {
                blockName.text = world.itemTypes[a.id - 128].itemName;
                hand.GenerateMesh(0);
            }
        }
    }
}

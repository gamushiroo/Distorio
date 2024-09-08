using UnityEngine;
using UnityEngine.UI;

public class MyUI : MonoBehaviour {


    public bool inUI;
    public Player player;
    public Slider hpBar;
    public Text hpText;
    public World world;
    public Text temp;
    public Toolbar toolbar;


    private void Start () {
        Cursor.lockState = CursorLockMode.Locked;


    }

    private void LateUpdate () {

        hpBar.value = player.entity.Health / Data.player.health;
        hpText.text = Mathf.FloorToInt(player.entity.Health).ToString("#,#");

        temp.text = Mathf.FloorToInt(world.GetTemp(player.camera.transform.position)).ToString("#,#") + "��C";

        if (Input.GetKeyDown(KeyCode.E)) {

            Switch();

        }

    }


    public void Switch () {
        inUI = !inUI;

        if (inUI) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

        } else {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        toolbar.inventory.Switch(inUI);
    }
}

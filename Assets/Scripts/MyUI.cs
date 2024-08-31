using UnityEngine;
using UnityEngine.UI;

public class MyUI : MonoBehaviour {


    public bool inUI;
    public Player player;
    public Slider hpBar;
    public Text hpText;
    public World world;
    public Text temp;


    private void Start () {
        Cursor.lockState = CursorLockMode.Locked;


    }

    private void LateUpdate () {

        hpBar.value = player.health / Data.player.health;
        hpText.text = Mathf.FloorToInt(player.health).ToString("#,#");

        temp.text = Mathf.FloorToInt(world.GetTemp(player.camera.transform.position)).ToString("#,#") + "ÅãC";
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

    }
}

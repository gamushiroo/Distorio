using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyUI : MonoBehaviour {


    public bool inUI;
    public Player player;
    public Slider hpBar;
    public Text hpText;


    private void Start () {

        Cursor.lockState = CursorLockMode.Locked;


    }

    private void LateUpdate () {

        hpBar.value = player.health / Data.player.health;
        hpText.text = Mathf.FloorToInt(player.health).ToString("#,#");
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

using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICanvasLobby : MonoBehaviour {

    public GameObject playerSelectCanvas;

    public GameObject player0;
    public GameObject player1;
    public GameObject player2;

    public GameObject panelKnight;
    public GameObject panel2Handed;
    public GameObject panelSwordsman;

    public Texture knight1;
    public Texture knight2;

    public Texture warrior1;
    public Texture warrior2;
    public Texture warrior3;
    public Texture warrior4;

    public Texture swordsman1;
    public Texture swordsman2;
    public Texture swordsman3;

    public Button hostButton;

    [HideInInspector]
    public short playerPrefabIndex;

    [HideInInspector]
    public int counter;

    private string[] tex = { "warrior1", "warrior2", "warrior3", "warrior4" };
    private int texCounter;
	// Use this for initialization
	void Start () {
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire5"))
            ButtonRight();

        if (Input.GetButtonDown("Fire6"))
        {
            texCounter++;
            if (texCounter >= 4)
                texCounter = 0;
            setMaterial(tex[texCounter]);
        }

        // a button
        if (Input.GetButtonDown("Fire3"))
            hostButton.Select();
    }

    public void ButtonRight()
    {
        counter += 1;
        if (counter > 2)
        {
            counter = 0;
        }

        UpdatePlayer();
    }

    public void ButtonLeft()
    {
        counter -= 1;
        if (counter < 0)
        {
            counter = 2;
        }

        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        switch (counter)
        {
            case 0:
                disablePlayer();
                player0.gameObject.SetActive(true);
                panelKnight.gameObject.SetActive(true);
                break;
            case 1:
                disablePlayer();
                player1.gameObject.SetActive(true);
                panel2Handed.gameObject.SetActive(true);
                break;
            case 2:
                disablePlayer();
                player2.gameObject.SetActive(true);
                panelSwordsman.gameObject.SetActive(true);
                break;
        }
    }

    private void disablePlayer()
    {
        player0.gameObject.SetActive(false);
        player1.gameObject.SetActive(false);
        player2.gameObject.SetActive(false);

        panelKnight.gameObject.SetActive(false);
        panel2Handed.gameObject.SetActive(false);
        panelSwordsman.gameObject.SetActive(false);
    }

    public void setMaterial(string tex)
    {
        GameObject player = null;
        switch (counter)
        {
            case 0:
                player = player0;
                break;
            case 1:
                player = player1;
                break;
            case 2:
                player = player2;
                break;
        }

        Texture texture = null;

        switch (tex)
        {
            case "knight1":
                texture = knight1;
                playerPrefabIndex = 0;
                break;
            case "knight2":
                texture = knight2;
                playerPrefabIndex = 1;
                break;
            case "warrior1":
                texture = warrior1;
                playerPrefabIndex = 2;
                break;
            case "warrior2":
                texture = warrior2;
                playerPrefabIndex = 3;
                break;
            case "warrior3":
                texture = warrior3;
                playerPrefabIndex = 4;
                break;
            case "warrior4":
                texture = warrior4;
                playerPrefabIndex = 5;
                break;
            case "swordsman1":
                texture = swordsman1;
                playerPrefabIndex = 6;
                break;
            case "swordsman2":
                texture = swordsman2;
                playerPrefabIndex = 7;
                break;
            case "swordsman3":
                texture = swordsman3;
                playerPrefabIndex = 8;
                break;
        }

        foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
        {
            renderer.material.SetTexture("_MainTex", texture);
        }
    }
}

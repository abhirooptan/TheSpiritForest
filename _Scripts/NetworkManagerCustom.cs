using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MsgTypes
{
    public const short PlayerPrefab = MsgType.Highest + 1;

    public class PlayerPrefabMsg : MessageBase
    {
        public short controllerID;
        public short prefabIndex;
        //public Texture texture;
    }
}

public class NetworkManagerCustom : NetworkManager
{

    public GameObject eventSystem;
    public GameObject btnStartHost;
    public GameObject btnJoinGame;
    public GameObject panelLobby;
    public GameObject instructionScreen;
    public GameObject menuPanel;
    public GameObject backButton;

    public GameObject playerSelectCanvas;
    UICanvasLobby script;

    public GameObject knight;
    public GameObject warrior;
    public GameObject swordsman;
    public GameObject location;

    Vector3 tempPos;

    NetworkManagerHUD hud;
    bool showGUI = false;
    bool showCursor = true;

    private void Start()
    {
        MenuBackButton();
        dontDestroyOnLoad = true;
        script = playerSelectCanvas.gameObject.GetComponent<UICanvasLobby>();
        hud = GetComponent<NetworkManagerHUD>();
        hud.offsetX = 60;
        hud.offsetY = 75;

        knight.transform.position = Camera.main.ScreenToWorldPoint(panelLobby.transform.position);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            showGUI = !showGUI;

        if (Input.GetButton("Instruction"))
            instructionScreen.gameObject.SetActive(true);
        if (Input.GetButtonUp("Instruction"))
            instructionScreen.gameObject.SetActive(false);

        if (Input.GetButtonDown("Cancel"))
            showCursor = !showCursor;
    }

    private void FixedUpdate()
    {
        Cursor.visible = showCursor;
        hud.showGUI = showGUI;
        panelLobby.gameObject.SetActive(!showGUI);

        tempPos = Camera.main.ScreenToWorldPoint(location.transform.position);
        knight.transform.position = new Vector3(tempPos.x + 1.75f, tempPos.y - 1, tempPos.z + 3.5f);
        warrior.transform.position = new Vector3(tempPos.x + 1.75f, tempPos.y - 1, tempPos.z + 3.5f);
        swordsman.transform.position = new Vector3(tempPos.x + 1.75f, tempPos.y - 1, tempPos.z + 3.5f);
    }

    public void MenuInstructionButton()
    {
        menuPanel.gameObject.SetActive(false);
        instructionScreen.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
    }

    public void MenuStoryButton()
    {
        dontDestroyOnLoad = false;
        Destroy(gameObject);
        SceneManager.LoadScene("Intro");
    }

    public void MenuQuitButton()
    {
        Application.Quit();
    }

    public void MenuBackButton()
    {
        menuPanel.gameObject.SetActive(true);
        instructionScreen.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    public void StartupHost()
    {
        //NetworkServer.Reset();
        SetPort();
        NetworkManager.singleton.StartHost();
        playerSelectCanvas.gameObject.SetActive(false);
        knight.SetActive(false);
        warrior.SetActive(false);
        swordsman.SetActive(false);
    }

    public void JoinGame()
    {
        SetIPAddress();
        SetPort();
        NetworkManager.singleton.StartClient();
        if (!IsClientConnected())
        {
            StartupHost();
        }
    }

    void SetIPAddress()
    {
        string ipAddress = GameObject.Find("InputField").transform.FindChild("Text").GetComponent<Text>().text;
        NetworkManager.singleton.networkAddress = ipAddress;
    }

    void SetPort()
    {
        NetworkManager.singleton.networkPort = 4444;
    }

    private void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            StartCoroutine(SetupMenuSceneButtons());
        }
        else
        {
            SetupOtherSceneButtons();
        }
    }

    IEnumerator SetupMenuSceneButtons()
    {
        yield return new WaitForSeconds(0.3f);
        foreach(EventSystem eventSys in FindObjectsOfType<EventSystem>())
        {
            eventSys.gameObject.SetActive(false);
        }

        eventSystem.gameObject.SetActive(true);
    }

    void SetupOtherSceneButtons()
    {
        GameObject.Find("ButtonDisconnect").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("ButtonDisconnect").GetComponent<Button>().onClick.AddListener(NetworkManager.singleton.StopHost);
    }

    public override void OnStopHost()
    {
        playerSelectCanvas.gameObject.SetActive(true);
        knight.SetActive(false);
        warrior.SetActive(false);
        swordsman.SetActive(false);
        playerSelectCanvas.GetComponent<UICanvasLobby>().ButtonLeft();
        playerSelectCanvas.GetComponent<UICanvasLobby>().ButtonRight();
        base.OnStopHost();
    }

    // ==========================================================================
    // player selection logic below DO NOT TOUCH
    // ==========================================================================

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler(MsgTypes.PlayerPrefab, OnResponsePrefab);
        base.OnStartServer();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        client.RegisterHandler(MsgTypes.PlayerPrefab, OnRequestPrefab);
        base.OnClientConnect(conn);
    }

    private void OnRequestPrefab(NetworkMessage netMsg)
    {
        MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
        msg.controllerID = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>().controllerID;
        msg.prefabIndex = playerSelectCanvas.GetComponent<UICanvasLobby>().playerPrefabIndex;
        client.Send(MsgTypes.PlayerPrefab, msg);
    }

    private void OnResponsePrefab(NetworkMessage netMsg)
    {
        MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();
        playerPrefab = spawnPrefabs[msg.prefabIndex];
        // TODO playerPrefab.GetComponent<PlayerScript>().setTexture(texture);
        
        //Debug.Log(playerPrefab.name + " spawned!");

        base.OnServerAddPlayer(netMsg.conn, msg.controllerID);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
        msg.controllerID = playerControllerId;
        NetworkServer.SendToClient(conn.connectionId, MsgTypes.PlayerPrefab, msg);
    }

   
}
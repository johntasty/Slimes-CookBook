using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.InputSystem;
using Steamworks;

public class LobbyPlayer : NetworkRoomPlayer
{
    private SlimesCookBook playerInput;

    public event System.Action<Color32> OnPlayerColorChanged;
    public event System.Action<ulong> OnPlayerNumberChanged;
    public event System.Action<int> OnPlayerParentChanged;
    public event System.Action<bool> OnPlayerStateChanged;

    // Players List to manage playerNumber
    static readonly List<LobbyPlayer> playersList = new List<LobbyPlayer>();
    static readonly List<uint> playerPortraits = new List<uint>();

    [Header("Player UI")]
    public GameObject playerUIPrefab;

    GameObject playerUIObject;
    LobbyPlayerUi playerUI = null;

    #region SyncVars

    [SyncVar(hook = nameof(PlayerNumberChanged))]
    public ulong playerNumber = 0;

    [SyncVar(hook = nameof(PlayerColorChanged))]
    public Color32 playerColor = Color.white;

    [SyncVar(hook = nameof(PlayerParentChanged))]
    public int playerParent = 1;

    [SyncVar(hook = nameof(ReadyState))]
    public bool readyToStart;


    RoomManagment room = NetworkManager.singleton as RoomManagment;
    // This is called by the hook of playerNumber SyncVar above
    public int GetPrefabSpawn()
    {
        return playerParent;
    }
    void PlayerNumberChanged(ulong _, ulong newPlayerNumber)
    {      

        OnPlayerNumberChanged?.Invoke(newPlayerNumber);
    }

    // This is called by the hook of playerColor SyncVar above
    void PlayerColorChanged(Color32 _, Color32 newPlayerColor)
    {
        OnPlayerColorChanged?.Invoke(newPlayerColor);
    }
    // This is called by the hook of playerParent SyncVar above
    void PlayerParentChanged(int oldParent, int newParent)
    {
        if (playerUIObject == null) return;
        int occupied = LobbyUi.GetPlayersPanel(newParent).transform.childCount;
        if (occupied > 0 && newParent != 1) { playerParent = 1; return; }
        playerUIObject.transform.SetParent(LobbyUi.GetPlayersPanel(newParent).transform);
        OnPlayerParentChanged?.Invoke(newParent);
    }
       
    void ReadyState(bool oldReadyState, bool newReadyState)
    {        
        OnPlayerStateChanged?.Invoke(newReadyState);           
    }
    #endregion

    #region Server

    public override void OnStartServer()
    {
        
        base.OnStartServer();

        // Add this to the static Players List
        playersList.Add(this);

        // set the Player Color SyncVar
        playerColor = Random.ColorHSV(0f, 1f, 0.9f, 0.9f, 1f, 1f);

        // set the initial player data
        playerParent = 1;

        readyToBegin = false;
    }

    //[ServerCallback]
    public void ResetPlayerNumbers(ulong steamId)
    {
        this.playerNumber = steamId;      
    }
   
    public override void OnStopServer()
    {       
        playersList.Remove(this);
    }
    // This only runs on the server, called from OnStartServer via InvokeRepeating
  
    #endregion

    #region Client
    public override void OnStartClient()
    {
       
        // Instantiate the player UI as child of the Players Panel
        playerUIObject = Instantiate(playerUIPrefab, LobbyUi.GetPlayersPanelDefault());
        
        playerUI = playerUIObject.GetComponent<LobbyPlayerUi>();

        playerPortraits.Add(playerUIObject.GetComponent<NetworkIdentity>().netId);
        // wire up all events to handlers in PlayerUI
        OnPlayerNumberChanged = playerUI.OnPlayerNumberChanged;
        OnPlayerColorChanged = playerUI.OnPlayerColorChanged;
        OnPlayerParentChanged = playerUI.OnPlayerParentChanged;
        OnPlayerStateChanged = playerUI.OnPlayerReadyState;

        // Invoke all event handlers with the initial data from spawn payload
        OnPlayerNumberChanged.Invoke(playerNumber);
        OnPlayerColorChanged.Invoke(playerColor);
        OnPlayerParentChanged.Invoke(playerParent);
        OnPlayerStateChanged.Invoke(readyToBegin);
    }
    public override void OnClientExitRoom()
    {
       
    }
    public override void OnClientEnterRoom()
    {
       
    }
    public override void OnStartLocalPlayer()
    {
        playerUI.SetLocalPlayer();
        _Setup();
        // Activate the main panel
        LobbyUi.SetActive(true);
        LobbyUi.GetReadyButton().gameObject.SetActive(true);
        LobbyUi.GetReadyButton().Select();
        LobbyUi.GetReadyButton().onClick.AddListener(ReadyUp);
        LobbyUi.GetReadyButton().enabled = true;
    }

    public override void OnStopLocalPlayer()
    {
        // Disable the main panel for local player
        //LobbyUi.GetReadyButton().gameObject.SetActive(false);
        //LobbyUi.GetReadyButton().enabled = false;
        //LobbyUi.SetActive(false);
    }
    public override void OnStopClient()
    {
        
        // disconnect event handlers
        OnPlayerNumberChanged = null;
        OnPlayerColorChanged = null;
        OnPlayerParentChanged = null;
        OnPlayerStateChanged = null;
        // Remove this player's UI object
        LobbyUi.GetReadyButton().onClick.RemoveAllListeners();
        Destroy(playerUIObject);
    }
    #endregion

    #region Commands
    [ClientRpc]
    public void RpcStartButton()
    {
        
        LobbyUi.GetStartButton().gameObject.SetActive(true);
        LobbyUi.GetStartButton().onClick.AddListener(SceneChanging);
    }
    [Command]
    void SceneChanging()
    {
        RoomManagment.singleton.SceneChange();
    }
    [Command]
    public void CmdChangeReady(bool readyState)
    {
        readyToBegin = readyState;
        readyToStart = readyToBegin;
        
        if (room != null)
        {
            
            room.ReadyStatusChanged();
        }
    }

    void ReadyUp()
    {
        if (!isLocalPlayer) { return; }
        if(playerParent == 1) {
            StartCoroutine(IssueWarning());
            return; }
        readyToBegin = !readyToBegin;       
        CmdChangeReady(readyToBegin);
    }
    [Command]
    void CmdMoveRight()
    {              
        if (playerParent == 2) return;
        playerParent++;
    }
    [Command]
    void CmdMoveLeft()
    {
       
        if (playerParent == 0) return;
        playerParent--;
        
    }

    #endregion
    #region MonoBehaviour
    private void _Setup()
    {
        if (!isLocalPlayer) return;
       
        GetComponent<PlayerInput>().enabled = true;       
        playerInput = new SlimesCookBook();
        playerInput.Enable();

    }
    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        int index = (int)context.ReadValue<Vector2>().x; 
        if(index == 1)
        {
            CmdMoveRight();
        }else if(index == -1)
        {
            CmdMoveLeft();
        }

    }
 IEnumerator IssueWarning()
    {
        playerUI.SelectHCaracter("Select a Character.");
        yield return new WaitForSeconds(0.5f);
        playerUI.SelectHCaracter("");
    }
    #endregion
}

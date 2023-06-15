using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Mirror;
public class SteamLobbyManagment : MonoBehaviour
{
    [SerializeField] GameObject buttons = null;
    private AdditiveNetwork networkManager;

    private const string HostAddressKey = "HostAddress";

    // Steam Api specific call backs
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;


    public static CSteamID LobbyId { get; private set; }
    private void Start()
    {
        networkManager = GetComponent<AdditiveNetwork>();
        // Wont run if steam is not started in the clients PC
        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }
    // Sends request to steam to create a lobby
    public void HostLobby()
    {
        buttons.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }
   
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        // If the lobby couldnt be made it wont disable the start
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            return;
        }
        LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        // Regular Mirror hosting
        networkManager.StartHost();
        // Creates the data needed for steam so others can join
        SteamMatchmaking.SetLobbyData(
            LobbyId, 
            HostAddressKey, 
            SteamUser.GetSteamID().ToString());
    }
    // Fires when a join Lobby is used from steam friends
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }

        string hostAddres = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey);

        networkManager.networkAddress = hostAddres;
        // Regular Mirror client start
        networkManager.StartClient();
    }
}

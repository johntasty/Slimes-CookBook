using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

/// <summary>
/// This is a specialized NetworkManager that includes a networked room.
/// The room has slots that track the joined players, and a maximum player count that is enforced.
/// It requires that the NetworkRoomPlayer component be on the room player objects.
/// NetworkRoomManager is derived from NetworkManager, and so it implements many of the virtual functions provided by the NetworkManager class.
/// </summary>
public class AdditiveNetwork : NetworkRoomManager
{
    public static new AdditiveNetwork singleton { get; private set; }

    /// <summary>Dictionary of transforms populated by NetworkStartPositions</summary>
    public static Dictionary<string, Transform> teleportRegistar = new Dictionary<string, Transform>();
    public List<GameObject> GamePlayersPrefabs = new List<GameObject>();
    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        singleton = this;
    }

    [Header("Additive Scenes - First is start scene")]

    [Scene, Tooltip("Add additive scenes here.\nFirst entry will be players' start scene")]
    public string[] additiveScenes;

    [Header("Fade Control - See child FadeCanvas")]

    [Tooltip("Reference to FadeInOut script on child FadeCanvas")]
    public FadingUi fadeInOut;

    // This is set true after server loads all subscene instances
    bool subscenesLoaded;

    // This is managed in LoadAdditive, UnloadAdditive, and checked in OnClientSceneChanged
    public bool isInTransition;
    public bool IsInTransition
    {
        get => isInTransition;
    }

    #region SceneManagment
    public override void OnServerSceneChanged(string sceneName)
    {
        //base.OnServerSceneChanged(sceneName);
        // This fires after server fully changes scenes, e.g. offline to online
        // If server has just loaded the Container (online) scene, load the subscenes on server
        if (sceneName == GameplayScene)
        {
            StartCoroutine(ServerLoadSubScenes());
        }            
    }
  
    IEnumerator ServerLoadSubScenes()
    {
        foreach (string additiveScene in additiveScenes)
        {
            yield return SceneManager.LoadSceneAsync(additiveScene, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D // change this to .Physics2D for a 2D game
            });
        }
       
        subscenesLoaded = true;
    }

    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if (sceneOperation == SceneOperation.UnloadAdditive)
            StartCoroutine(UnloadAdditive(sceneName));

        if (sceneOperation == SceneOperation.LoadAdditive)
            StartCoroutine(LoadAdditive(sceneName));
    }
    IEnumerator LoadAdditive(string sceneName)
    {
        isInTransition = true;

        // This will return immediately if already faded in
        // e.g. by UnloadAdditive or by default startup state
        yield return fadeInOut.FadeIn();

        // host client is on server...don't load the additive scene again
        // Start loading the additive subscene      
        if (mode == NetworkManagerMode.ClientOnly)
        {
            // Start loading the additive subscene
            loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
                yield return null;
        }

        // Reset these to false when ready to proceed
        NetworkClient.isLoadingScene = false;
        isInTransition = false;

        OnClientSceneChanged();

        // Reveal the new scene content.
        yield return fadeInOut.FadeOut();
    }

    IEnumerator UnloadAdditive(string sceneName)
    {
        isInTransition = true;

        // This will return immediately if already faded in
        // e.g. by LoadAdditive above or by default startup state.
        yield return fadeInOut.FadeIn();

        // host client is on server...don't unload the additive scene here.
       
        if (mode == NetworkManagerMode.ClientOnly)
        {
           
            if (SceneManager.GetSceneByPath(sceneName).IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
                yield return Resources.UnloadUnusedAssets();
            }
            //else
            //{
            //    Debug.Log("Scene invalid");
            //}
        }

        // Reset these to false when ready to proceed
        NetworkClient.isLoadingScene = false;
        isInTransition = false;

        OnClientSceneChanged();

        // There is no call to FadeOut here on purpose.
        // Expectation is that a LoadAdditive or full scene change
        // will follow that will call FadeOut after that scene loads.
    }

    public override void OnClientSceneChanged()
    {
        // Only call the base method if not in a transition.
        // This will be called from DoTransition after setting doingTransition to false
        // but will also be called first by Mirror when the scene loading finishes.
        if (!isInTransition)
            base.OnClientSceneChanged();
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
       
        // This fires from a Ready message client sends to server after loading the online scene
        base.OnServerReady(conn);
        if (SceneManager.GetActiveScene().name == "RoomScene")
        {
            OnRoomServerPlayersNotReady();
            foreach (NetworkRoomPlayer player in roomSlots)
                if (player != null)
                {
                    player.gameObject.GetComponent<LobbyPlayer>().playerParent = 1;
                    player.gameObject.GetComponent<LobbyPlayer>().readyToStart = false;
                    player.readyToBegin = false;
                }
        }
        if (conn.identity == null)
        {
            StartCoroutine(AddPlayerDelayed(conn));
            
        }
         if (conn != null && conn.identity != null)
            {
                GameObject roomPlayer = conn.identity.gameObject;

                // if null or not a room player, don't replace it
                if (roomPlayer != null && roomPlayer.GetComponent<NetworkRoomPlayer>() != null)
                    StartCoroutine(ReplacePlayerDelayed(conn));
            }

    }
    IEnumerator ReplacePlayerDelayed(NetworkConnectionToClient conn)
    {
        // Wait for server to async load all subscenes for game instances
        while (!subscenesLoaded)
            yield return null;

        // Send Scene msg to client telling it to load the first additive scene
        conn.Send(new SceneMessage { sceneName = additiveScenes[0], sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

        yield return new WaitForSeconds(fadeInOut.GetDuration());
       
        // We have Network Start Positions in first additive scene...pick one
        Transform start = GetStartPosition();
        int prefab = conn.identity.gameObject.GetComponent<LobbyPlayer>().GetPrefabSpawn();
        GameObject prefabToSpawn = null;
        // Instantiate player as child of start position - this will place it in the additive scene
        // This also lets player object "inherit" pos and rot from start position transform
        if (prefab == 0)
        {
            prefabToSpawn = Instantiate(GamePlayersPrefabs[0], start);
        }
        else if (prefab == 2)
        {
            prefabToSpawn = Instantiate(GamePlayersPrefabs[1], start);
        }
        //GameObject player = Instantiate(playerPrefab, start);
        // now set parent null to get it out from under the Start Position object
        prefabToSpawn.transform.SetParent(null);
        SceneManager.MoveGameObjectToScene(prefabToSpawn, SceneManager.GetSceneByPath(additiveScenes[0]));
        // Wait for end of frame before adding the player to ensure Scene Message goes first
        yield return new WaitForEndOfFrame();

        NetworkServer.Destroy(conn.identity.gameObject);
        NetworkServer.ReplacePlayerForConnection(conn, prefabToSpawn, true);
       
    }
    // This delay is mostly for the host player that loads too fast for the
    // server to have subscenes async loaded from OnServerSceneChanged ahead of it.
    IEnumerator AddPlayerDelayed(NetworkConnectionToClient conn)
    {

        // We have Network Start Positions in first additive scene...pick one
        Transform start = GetStartPosition();

        // Instantiate player as child of start position - this will place it in the additive scene
        // This also lets player object "inherit" pos and rot from start position transform
        GameObject player = Instantiate(roomPlayerPrefab.gameObject, start);
        // now set parent null to get it out from under the Start Position object
        player.transform.SetParent(null);

        // Wait for end of frame before adding the player to ensure Scene Message goes first
        yield return new WaitForEndOfFrame();

        PendingPlayer pending;
        pending.conn = conn;
        pending.roomPlayer = player;
        pendingPlayers.Add(pending);

        // Finally spawn the player object for this connection
        NetworkServer.AddPlayerForConnection(conn, player);

        CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(
            SteamLobbyManagment.LobbyId,
            numPlayers - 1);
        if (conn.identity.TryGetComponent(out LobbyPlayer lobbyPlayer))
        {
            lobbyPlayer.ResetPlayerNumbers(steamId.m_SteamID);
        }
    }
    #endregion

    #region Server Callbacks
   
    public static void RegisterTeleportPositions(Transform start, string SceneName)
    {       
        if (teleportRegistar.ContainsKey(SceneName)) return;
        teleportRegistar.Add(SceneName, start);
       
    }
    public static void UnRegisterTeleportPositions(string SceneName)
    {
        teleportRegistar.Remove(SceneName);
    }
    public Transform GetTeleportPosition(string SceneName)
    {
        teleportRegistar.TryGetValue(SceneName, out Transform position);        
        return position;
    }
    /// <summary>
    /// This is called on the server when the server is started - including when a host is started.
    /// </summary>
    public override void OnRoomStartServer() { }

    /// <summary>
    /// This is called on the server when the server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnRoomStopServer() { }

    /// <summary>
    /// This is called on the host when a host is started.
    /// </summary>
    public override void OnRoomStartHost() { }

    /// <summary>
    /// This is called on the host when the host is stopped.
    /// </summary>
    public override void OnRoomStopHost() { }

    /// <summary>
    /// This is called on the server when a new client connects to the server.
    /// </summary>
    /// <param name="conn">The new connection.</param>
    public override void OnRoomServerConnect(NetworkConnectionToClient conn) { }

    /// <summary>
    /// This is called on the server when a client disconnects.
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn) { }

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName) { }

    /// <summary>
    /// This allows customization of the creation of the room-player object on the server.
    /// <para>By default the roomPlayerPrefab is used to create the room-player, but this function allows that behaviour to be customized.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <returns>The new room-player object.</returns>
    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        return base.OnRoomServerCreateRoomPlayer(conn);
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <param name="roomPlayer">The room player object for this connection.</param>
    /// <returns>A new GamePlayer object.</returns>
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>This is only called for subsequent GamePlay scenes after the first one.</para>
    /// <para>See OnRoomServerCreateGamePlayer to customize the player object for the initial GamePlay scene.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnRoomServerAddPlayer(conn);
       
    }

    /// <summary>
    /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
    /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
    /// </summary>
    /// <param name="conn">The connection of the player</param>
    /// <param name="roomPlayer">The room player object.</param>
    /// <param name="gamePlayer">The game player object.</param>
    /// <returns>False to not allow this player to replace the room player.</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    /// <summary>
    /// This is called on server from NetworkRoomPlayer.CmdChangeReadyState when client indicates change in Ready status.
    /// </summary>
    public override void ReadyStatusChanged()
    {
        base.ReadyStatusChanged();
    }

    /// <summary>
    /// This is called on the server when all the players in the room are ready.
    /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
    /// </summary>
    public override void OnRoomServerPlayersReady()
    {
        if (SceneManager.GetActiveScene().name == "RoomScene")
        {
            NetworkConnectionToClient idLeader = roomSlots[0].GetComponent<NetworkIdentity>().connectionToClient;
            idLeader.identity.transform.GetComponent<LobbyPlayer>().RpcStartButton(idLeader);
        }
    }
    public void SceneChange()
    {

        ServerChangeScene(GameplayScene);

    }
   
    /// <summary>
    /// This is called on the server when CheckReadyToBegin finds that players are not ready
    /// <para>May be called multiple times while not ready players are joining</para>
    /// </summary>
    public override void OnRoomServerPlayersNotReady() {
        if (SceneManager.GetActiveScene().name == "RoomScene")
        {
            foreach (NetworkRoomPlayer player in roomSlots)
                if (player != null)
                {
                    player.GetComponent<LobbyPlayer>().RpcStartDisable();
                }
            
        }
    }
    
    #endregion

    #region Client Callbacks

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client enters the room.
    /// </summary>
    public override void OnRoomClientEnter() { }

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client exits the room.
    /// </summary>
    public override void OnRoomClientExit() { }

    /// <summary>
    /// This is called on the client when it connects to server.
    /// </summary>
    public override void OnRoomClientConnect() { }

    /// <summary>
    /// This is called on the client when disconnected from a server.
    /// </summary>
    public override void OnRoomClientDisconnect() { }

    /// <summary>
    /// This is called on the client when a client is started.
    /// </summary>
    public override void OnRoomStartClient() { }

    /// <summary>
    /// This is called on the client when the client stops.
    /// </summary>
    public override void OnRoomStopClient() { }

    /// <summary>
    /// This is called on the client when the client is finished loading a new networked scene.
    /// </summary>
    public override void OnRoomClientSceneChanged() { }

    #endregion

    #region Optional UI

    public override void OnGUI()
    {
        base.OnGUI();
    }

    #endregion
}

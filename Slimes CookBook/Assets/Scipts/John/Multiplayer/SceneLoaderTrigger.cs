using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
public class SceneLoaderTrigger : NetworkBehaviour
{
    [Scene, Tooltip("Which scene to send player from here")]
    public string destinationScene;

    [SyncVar(hook = nameof(OnLabelTextChanged))]
    public string labelText;
    public void OnLabelTextChanged(string _, string newValue)
    {
        gameObject.name = labelText;
    }
    void OnTriggerEnter(Collider other)
    {
        // tag check in case you didn't set up the layers and matrix as noted above
        if (!other.CompareTag("Wizard")) return;

        //// applies to host client on server and remote clients
        //if (other.TryGetComponent(out PlayerController playerController))
        //    //playerController.enabled = false;
        Debug.Log(isServer);
        //if (isServer)
        //{
        //    StartCoroutine(SendPlayerToNewScene(other.transform.parent.gameObject));
        //}     
        
    }
    [ServerCallback]
    IEnumerator Test(GameObject player)
    {
        Debug.Log(player);
        yield return null;
    }
    [ServerCallback]
    IEnumerator SendPlayerToNewScene(GameObject player)
    {
        Debug.Log("triggered");
        if (player.TryGetComponent(out NetworkIdentity identity))
        {
            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null) yield break;

            //// Tell client to unload previous subscene. No custom handling for this.
            //conn.Send(new SceneMessage { sceneName = gameObject.scene.path, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });

            ////NetworkServer.RemovePlayerForConnection(conn, false);

            //// reposition player on server and client
            //player.transform.position = startPosition;
            //player.transform.LookAt(Vector3.up);

            //// Move player to new subscene.
            //SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(destinationScene));

            // Tell client to load the new subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = destinationScene, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

            //NetworkServer.AddPlayerForConnection(conn, player);

            //// host client would have been disabled by OnTriggerEnter above
            //if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.TryGetComponent(out PlayerController playerController))
            //    playerController.enabled = true;
            yield return null;
        }
    }
}

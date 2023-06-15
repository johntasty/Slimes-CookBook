using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
public class SceneLoader : NetworkBehaviour
{
    [Scene, Tooltip("Which scene to send player from here")]
    public string destinationScene;
    [SerializeField]
    string destinationSceneName;
   
    [Tooltip("Where to spawn player in Destination Scene")]
    public Vector3 startPosition;

    [ServerCallback]
    private void Start()
    {
        StartCoroutine(PositionDelay());
    }
    IEnumerator PositionDelay()
    {
        //gets teleports and checkpoints, delay is added as sometimes it went out of sync
        yield return new WaitForSeconds(1f);
        startPosition = AdditiveNetwork.singleton.GetTeleportPosition(destinationSceneName).position;
    }

    void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            //disabling controls so they dont run off
            other.GetComponent<CharacterController>().enabled = false;
            if (isServer)
                StartCoroutine(SendPlayerToNewScene(other.transform.parent.gameObject));
           
        }
    }
   //this only happens on the server, the client remains on the buffer scene 
    [ServerCallback]
    IEnumerator SendPlayerToNewScene(GameObject parent)
    {
        if (parent.TryGetComponent(out NetworkIdentity identity))
        {
            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null) yield break;

            // Tell client to unload previous subscene. No custom handling for this.     
            conn.Send(new SceneMessage { sceneName = gameObject.scene.path, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });
           
            yield return new WaitForSeconds(AdditiveNetwork.singleton.fadeInOut.GetDuration());
            NetworkServer.RemovePlayerForConnection(conn, false);

            parent.transform.position = startPosition;

            foreach (Transform child in parent.transform)
            {
                child.position = startPosition;
            }
            // Move player to new subscene.
            SceneManager.MoveGameObjectToScene(parent, SceneManager.GetSceneByPath(destinationScene));
            // Tell client to load the new subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = destinationScene, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });
            yield return new WaitForSeconds(AdditiveNetwork.singleton.fadeInOut.GetDuration());
            
            NetworkServer.AddPlayerForConnection(conn, parent);
            parent.GetComponentInChildren<CharacterController>().enabled = true;
        }
    }
}
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
        yield return new WaitForSeconds(1f);
        startPosition = AdditiveNetwork.singleton.GetTeleportPosition(destinationSceneName).position;
    }
    // Note that I have created layers called Player(6) and Portal(7) and set them
    // up in the Physics collision matrix so only Player collides with Portal.
    void OnTriggerEnter(Collider other)
    {
        // tag check in case you didn't set up the layers and matrix as noted above
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            other.GetComponent<CharacterController>().enabled = false;
            if (isServer)
                StartCoroutine(SendPlayerToNewScene(other.transform.parent.gameObject));
            //SceneChanging(other.transform.parent.gameObject);
        }
    }
    [Command(requiresAuthority = false)]
    void SceneChanging(GameObject parent)
    {
        StartCoroutine(SendPlayerToNewScene(parent));
    }
  
    [ServerCallback]
    IEnumerator SendPlayerToNewScene(GameObject parent)
    {
        if (parent.TryGetComponent(out NetworkIdentity identity))
        {
            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null) yield break;

            // Tell client to unload previous subscene. No custom handling for this.     
            conn.Send(new SceneMessage { sceneName = gameObject.scene.path, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });
            Debug.Log(gameObject.scene.path);
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
            Debug.Log(destinationScene);
            NetworkServer.AddPlayerForConnection(conn, parent);
            parent.GetComponentInChildren<CharacterController>().enabled = true;
        }
    }
}
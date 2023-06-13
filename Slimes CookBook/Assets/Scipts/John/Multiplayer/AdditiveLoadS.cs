using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
public class AdditiveLoadS : NetworkBehaviour
{
    [Scene, Tooltip("Which scene to send player from here")]
    public string destinationScene;
   

    void OnTriggerEnter(Collider other)
    {
        // tag check in case you didn't set up the layers and matrix as noted above
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            
            if (isServer)
                StartCoroutine(SendPlayerToNewScene(other.transform.parent.gameObject));

        }
    }

    [ServerCallback]
    IEnumerator SendPlayerToNewScene(GameObject parent)
    {
        if (parent.TryGetComponent(out NetworkIdentity identity))
        {
            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null) yield break;
            
            // Move player to new subscene.
            SceneManager.MoveGameObjectToScene(parent, SceneManager.GetSceneByPath(destinationScene));
            // Tell client to load the new subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = destinationScene, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });
           

        }
    }
}

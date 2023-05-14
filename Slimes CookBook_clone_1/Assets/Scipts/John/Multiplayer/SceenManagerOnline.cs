using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SceenManagerOnline : NetworkBehaviour
{
    [SerializeField]
    RoomManagment _Manager;

    [Scene]
    [FormerlySerializedAs("m_OfflineScene")]
    [Tooltip("Scene that Mirror will switch to when the client or server is stopped")]
    public string PuzzleScene = "";

    bool loaded = false;
    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        _Manager = RoomManagment.singleton as RoomManagment;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (loaded) return;
        loaded = true;
        RoomManagment.loadingSceneAsync = SceneManager.LoadSceneAsync(PuzzleScene, LoadSceneMode.Additive);

    }
}

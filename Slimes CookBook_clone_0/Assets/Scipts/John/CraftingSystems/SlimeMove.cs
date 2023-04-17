using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SlimeMove : MonoBehaviour
{
    [SerializeField]
    GameObject PlayerSlime;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Crafting")
        {
            PlayerSlime.transform.position = new Vector3(-75f, 1f, 75f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.tag == "Loader" && SceneManager.sceneCount == 1)
        {

           SceneManager.LoadScene("Crafting", LoadSceneMode.Additive);

        }
        if (other.tag == "Unload" && SceneManager.sceneCount == 2)
        {
            PlayerSlime.transform.position = new Vector3(0f, 1f, 0f);
            SceneManager.UnloadSceneAsync("Crafting");

        }
    }
}

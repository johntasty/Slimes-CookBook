using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiOnOff : MonoBehaviour
{
   public void OnOffFunction(GameObject panel)
    {
        bool on_off = !panel.activeInHierarchy;
        panel.SetActive(on_off);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

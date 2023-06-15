using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class LobbyPlayerUi : MonoBehaviour
{

    [Header("Player Components")]
    public Image image;


    [Header("Child Text Objects Name")]
    public TMP_Text playerNameText;

    [Header("Child Text Objects ReadyState")]
    public TMP_Text playerReadyState;

    [Header("Child Text Objects Warning")]
    public TMP_Text playerCommand;
    // Sets a highlight color for the local player
    public void SetLocalPlayer()
    {
        // add a visual background for the local player in the UI
        image.color = new Color(1f, 1f, 1f, 0.1f);
    }

    // This value can change as clients leave and join
    public void OnPlayerNumberChanged(ulong newPlayerNumber)
    {
        // Steam Api used to grab the users name
        var cSteam = new CSteamID(newPlayerNumber);
        playerNameText.text = SteamFriends.GetFriendPersonaName(cSteam);
       
    }

    // Random color set by Player::OnStartServer
    public void OnPlayerColorChanged(Color32 newPlayerColor)
    {
        playerNameText.color = newPlayerColor;
    }
    // Ready up text on ui visual
    public void OnPlayerReadyState(bool newPlayerState)
    {
        if (newPlayerState)
            playerReadyState.text = "Ready";
        else
            playerReadyState.text = "Not Ready";
        
    }
    public void SelectHCaracter(string commandStrign)
    {
        playerCommand.text = commandStrign;
    }
    public void OnPlayerParentChanged(int newParent)
    {        
        //this.transform.parent = this.transform.parent.parent.GetChild(newParent);
        //playerNameText.color = newPlayerColor;
    }
}

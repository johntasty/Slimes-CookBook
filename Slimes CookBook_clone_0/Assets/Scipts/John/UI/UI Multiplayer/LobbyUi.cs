using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUi : MonoBehaviour
{
    [Tooltip("Assign Main Panel so it can be turned on from Player:OnStartClient")]
    public RectTransform mainPanel;

    [Tooltip("Assign Players Panel for instantiating PlayerUI as child")]
    public RectTransform playersPanel;

    [Tooltip("Assign Start Button")]
    public Button startButton;


    [Tooltip("Assign Ready Button")]
    public Button readyButton;
    // static instance that can be referenced from static methods below.
    static LobbyUi instance;

    void Awake()
    {
        instance = this;
    }

    public static void SetActive(bool active)
    {
        instance.mainPanel.gameObject.SetActive(active);
    }

    public static RectTransform GetPlayersPanelDefault() => instance.playersPanel.GetChild(1).GetComponent<RectTransform>();
    public static RectTransform GetPlayersPanel(int index) => instance.playersPanel.GetChild(index).GetComponent<RectTransform>();
    public static Button GetStartButton() => instance.startButton;
    public static Button GetReadyButton() => instance.readyButton;
}

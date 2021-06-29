
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerListEntry : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text PlayerNameText;
    public Image PlayerColorImage;
    public Button PlayerButtonReady;

    private int clientID;
    private bool isPlayerReady;
    #region UNITY
    public void OnEnable()
    {
        PlayerNumbering.OnPlayerNumberingChanged += OnPlayerNumberingChanged;
    }
    public void OnDisable()
    {
        PlayerNumbering.OnPlayerNumberingChanged -= OnPlayerNumberingChanged;
    }
    // Start is called before the first frame update
    void Start()
    {
        //OnPlayerNumberingChanged();
        if (PhotonNetwork.LocalPlayer.ActorNumber != clientID)
        {
            PlayerButtonReady.gameObject.SetActive(false);
        }
        else
        {
            Hashtable initialProps = new Hashtable() { {ModelGame.PLAYER_READY, isPlayerReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
            PhotonNetwork.LocalPlayer.SetScore(0);

            PlayerButtonReady.onClick.AddListener(() =>
            {
                isPlayerReady = !isPlayerReady;
                SetPlayerReady(isPlayerReady);

                Hashtable props = new Hashtable() { { ModelGame.PLAYER_READY, isPlayerReady } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                if (PhotonNetwork.IsMasterClient)
                {
                    FindObjectOfType<LobbyMainPanel>().LocalPlayerPropertiesUpdated();
                }
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion
    public void Initialize(int clientID, string playerName)
    {
        this.clientID = clientID;
        PlayerNameText.text = playerName;
    }
    private void OnPlayerNumberingChanged()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if(p.ActorNumber == clientID)
            {
                PlayerColorImage.color = ModelGame.GetColor(p.GetPlayerNumber());
                PlayerNameText.color = PlayerColorImage.color;
            }
        }
    }
    public void SetPlayerReady(bool playerReady)
    {
        PlayerButtonReady.GetComponentInChildren<TMP_Text>().text = playerReady ? "Go!" : "Ready?";
    }
}

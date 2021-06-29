using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyMainPanel : MonoBehaviourPunCallbacks
{
    [Header("Player Info")]
    public InputField PlayerNameInput;

    [Header("Main Menu Panel")]
    public GameObject MainMenuPanel;

    [Header("Room List Panel")]
    public GameObject RoomListPanel;

    public GameObject RoomListContent;
    public GameObject RoomListEntryPrefab;

    [Header("Create Room Panel")]
    public GameObject CreateRoomPanel;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomPanel;
    public GameObject RoomPanelContent;

    public Button StartGameButton;
    public GameObject PlayerListEntryPrefab;

    [Header("ABC")]
    [SerializeField] private TMP_InputField RoomNameInput;
    [SerializeField] private TMP_InputField RoomPasswordInput;
    [SerializeField] private Slider RoomMaxplayers;
    [SerializeField] private Toggle RoomIsVisible;
    [SerializeField] private TMP_Text MaxPlayersText;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = "1";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region UNITY
    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

        PlayerNameInput.text = "Player " + Random.Range(1000, 10000);
    }
    #endregion

    #region PUN CALLBACKS
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server!");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        SetActivePanel(RoomListPanel.name);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            object isPlayerReady;
            if (changedProps.TryGetValue(ModelGame.PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }
        }

        StartGameButton.gameObject.SetActive(CheckPlayersPrepared());
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnect to Master Server! " + cause);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }
    public override void OnJoinedLobby()
    {
        // whenever this joins a new lobby, clear any previous room lists
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
        ClearRoomListView();
    }
    public override void OnJoinedRoom()
    {
        // joining (or entering) a room invalidates any cached lobby room list (even if LeaveLobby was not called due to just joining a room)
        cachedRoomList.Clear();


        SetActivePanel(InsideRoomPanel.name);
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(RoomPanelContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerListEntry>().Initialize(p.ActorNumber, p.NickName);

            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(ModelGame.PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }

            playerListEntries.Add(p.ActorNumber, entry);
        }
        StartGameButton.gameObject.SetActive(CheckPlayersPrepared());
        //PhotonNetwork.LoadLevel("Main");
    }
    public override void OnLeftRoom()
    {
        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
        SetActivePanel(RoomListPanel.name);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(PlayerListEntryPrefab);
        entry.transform.SetParent(RoomPanelContent.transform);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);

        StartGameButton.gameObject.SetActive(CheckPlayersPrepared());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);

        StartGameButton.gameObject.SetActive(CheckPlayersPrepared());
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(RoomListPanel.name);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        OnCreateRoomButtonClickedToPanel();
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.gameObject.SetActive(CheckPlayersPrepared());
        }
    }
    #endregion

    #region UI CALLBACKS
    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("Main");
    }
    public void OnExitGameButtonClicked()
    {
        Application.Quit();
    }
    public void OnJoinRandomRoomButtonClicked()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public void OnMultiplayerButtonClicked()
    {
        OnNickNameEndEdit();
        PhotonNetwork.ConnectUsingSettings();
    }
    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        SetActivePanel(MainMenuPanel.name);
        PhotonNetwork.Disconnect();
    }
    public void OnCreateRoomButtonClickedToPanel()
    {
        SetActivePanel(CreateRoomPanel.name);
        //PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 });
    }
    public void OnCreateRoomButtonClicked()
    {
        string RoomName = RoomNameInput.text;
        string RoomPassword = RoomPasswordInput.text;
        byte MaxPlayers = (byte)RoomMaxplayers.value;
        bool IsVisible = RoomIsVisible.isOn;
        PhotonNetwork.CreateRoom(RoomName, new RoomOptions() { MaxPlayers = MaxPlayers, PlayerTtl = 10000, IsVisible = IsVisible, IsOpen = RoomPassword == string.Empty }, null);
        SetActivePanel(InsideRoomPanel.name);
    }
    public void OnNickNameEndEdit()
    {
        string playerName = PlayerNameInput.text;

        if (string.Empty != playerName)
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            Debug.Log("Player's name set to " + playerName);
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }
    public void OnSliderValueChanged()
    {
        MaxPlayersText.text = "Max players: " + RoomMaxplayers.value;
    }
    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }
    #endregion
    private bool CheckPlayersPrepared()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(ModelGame.PLAYER_READY, out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }
    private void SetActivePanel(string activePanel)
    {
        MainMenuPanel.SetActive(activePanel.Equals(MainMenuPanel.name));
        RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name));    // UI should call OnRoomListButtonClicked() to activate this
        CreateRoomPanel.SetActive(activePanel.Equals(CreateRoomPanel.name));
        InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));        
    }
    public void LocalPlayerPropertiesUpdated()
    {
        StartGameButton.gameObject.SetActive(CheckPlayersPrepared());
    }
    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab);
            entry.transform.SetParent(RoomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }
}

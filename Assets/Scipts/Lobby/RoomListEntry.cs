using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class RoomListEntry : MonoBehaviour
{
    public Text RoomNameText;
    public Text RoomPlayersText;
    public Button JoinRoomButton;

    private string roomName;
    // Start is called before the first frame update
    void Start()
    {
        JoinRoomButton.onClick.AddListener(() =>
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            PhotonNetwork.JoinRoom(roomName);
        });
    } 
    public void Initialize(string roomName, byte currentName, byte maxPlayers)
    {
        this.roomName = roomName;
        RoomNameText.text = roomName;
        RoomPlayersText.text = currentName + " / " + maxPlayers;
    }
}

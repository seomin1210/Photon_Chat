using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class ServerManager : MonoBehaviourPunCallbacks
{
    private GameObject _playerPrefab = null;

    public InputField Nick_Input;
    public GameObject Before_Connect;
    public GameObject After_Connect;

    void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private IEnumerator Start()
    {
        yield return PhotonNetwork.ConnectUsingSettings();

        if (PhotonNetwork.IsConnected)
        {
            Before_Connect.SetActive(false);
            After_Connect.SetActive(true);
        }
        else
        {
            Application.Quit();
        }
    }

    public void ClickButton()
    {
        if (Nick_Input.text.Length > 1 && Nick_Input.text.Length < 9)
        {
            PhotonNetwork.NickName = Nick_Input.text;

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 10;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;

            PhotonNetwork.JoinOrCreateRoom("Room 2", roomOptions, TypedLobby.Default);
        }
    }

    // Player Join Room
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room");
        base.OnJoinedRoom();
        After_Connect.SetActive(false);
        // Player Create
        _playerPrefab = PhotonNetwork.Instantiate("Player", transform.position, transform.rotation);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left a room");
        base.OnLeftRoom();
        PhotonNetwork.Destroy(_playerPrefab);
    }

    // Other Player join Room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("A New ohter Player joined the room");
        base.OnPlayerEnteredRoom(newPlayer);
    }

    // Disconnected Server, call this function
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected Server");
        base.OnDisconnected(cause);
        Application.Quit();
    }
}

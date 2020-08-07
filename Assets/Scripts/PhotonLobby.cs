using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

using ExitGames.Client.Photon;
using System; // added for String.Format()
using UnityEngine.UI;
using System.Collections.Generic;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text progressLabel;
    [SerializeField]
    private byte maxPlayersPerRoom = 4;
    private string defaultRoomName = "defaultExerciseRoom";
    RoomOptions roomOptions;
    private const byte COLOR_CHANGE_EVENT = 0;

    public static PhotonLobby Lobby;

    private int roomNumber = 1;
    private int userIdCount;

    private void Awake()
    {
        roomOptions = new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = maxPlayersPerRoom };
        if (Lobby == null)
        {
            Lobby = this;
        }
        else
        {
            if (Lobby != this)
            {
                Destroy(Lobby.gameObject);
                Lobby = this;
            }
        }

        DontDestroyOnLoad(gameObject);

        StartNetwork();
    }

    public override void OnConnectedToMaster()
    {
        var randomUserId = UnityEngine.Random.Range(0, 999999);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.UserId = randomUserId.ToString();
        userIdCount++;
        PhotonNetwork.NickName = PhotonNetwork.AuthValues.UserId;
        _print(true, "OnConnectedToMaster set props, joining random");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _print(true, String.Format("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause));
        networkEventsDisable();
    }

    public void JoinOrCreateRoom()
    {
        _print(true, "JoinOrCreateRoom " + defaultRoomName);
        PhotonNetwork.JoinOrCreateRoom(defaultRoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        _print(true, "\nPhotonLobby.OnJoinedRoom(): " + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom.Name != defaultRoomName) {
            _print(true, "PhotonNetwork.CurrentRoom.Name != defaultRoomName");
            _print(true, PhotonNetwork.CurrentRoom.Name + " != " + defaultRoomName);
            JoinOrCreateRoom();
        }

        networkEventsEnable();
        _print(true, "Other/Total players in room: " + PhotonNetwork.CountOfPlayersInRooms + " / " + (PhotonNetwork.CountOfPlayersInRooms + 1));
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        _print(true, "\nPhotonLobby.OnJoinRandomFailed()");
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _print(true, "\nPhotonLobby.OnCreateRoomFailed()");
        CreateRoom();
    }

    public override void OnCreatedRoom()
    {
        _print(true, "\nPhotonLobby.OnCreatedRoom()");
        base.OnCreatedRoom();
        roomNumber++;
    }

    private void StartNetwork()
    {
        _print(true, "\nPhotonLobby.StartNetwork()");
        PhotonNetwork.ConnectUsingSettings();
        Lobby = this;
    }

    private void CreateRoom()
    {
        _print(true, "\nPhotonLobby.CreateRoom()");
        JoinOrCreateRoom();
    }

    private void _print(bool shouldPrint, string msg)
    {
        if (shouldPrint) Debug.Log(msg);
        if (shouldPrint) progressLabel.text += "\n" + msg;
    }

    private void networkEventsEnable()
    {
        _print(true, "adding event callback");
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void networkEventsDisable()
    {
        _print(true, "removing event callback");
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        if (obj.Code == COLOR_CHANGE_EVENT)
        {   
            _print(true, "received COLOR_CHANGE_EVENT");
        }
        else
        {
            _print(true, "unhandled obj.Code: " + obj.Code);
        }
    }
}

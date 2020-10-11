using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Collections.Generic;

public class PhotonLobby : MonoBehaviourPunCallbacks
{   // e9fe0af3-b001-4ef6-82d3-c102fafd5a63
    [SerializeField]
    private Text progressLabel;

    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    public static PhotonLobby Lobby;

    private string defaultRoomName = "defaultExerciseRoom";
    private const byte COLOR_CHANGE_EVENT = 0;
    private const byte BODY_TRACKING_EVENT = 1;
    private int roomNumber = 1;
    private int userIdCount;

    RoomOptions roomOptions;

    private void Start() {
        _print(true, "Start begin");
        roomOptions = new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = maxPlayersPerRoom };
        if (Lobby == null) {
            _print(true, "Start: Lobby == null");
            Lobby = this;
        }

        if (Lobby != this) {
            _print(true, "Start: Lobby != this");
            Destroy(Lobby.gameObject);
            Lobby = this;
        }

        DontDestroyOnLoad(gameObject);
        _print(true, "ConnectUsingSettings begin");
        PhotonNetwork.ConnectUsingSettings();
        _print(true, "Start finish");
    }

    public override void OnConnectedToMaster() {
        _print(true, "OnConnectedToMaster begin");
        var randomUserId = UnityEngine.Random.Range(0, 999999);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.UserId = randomUserId.ToString();
        userIdCount++;
        PhotonNetwork.NickName = PhotonNetwork.AuthValues.UserId;
        _print(true, "OnConnectedToMaster finish, joining random");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        _print(true, "\nPhotonLobby.OnJoinRandomFailed()");
        JoinOrCreateRoom_defaultRoomName();
    }


    public override void OnCreateRoomFailed(short returnCode, string message) {
        _print(true, "\nPhotonLobby.OnCreateRoomFailed()");
        JoinOrCreateRoom_defaultRoomName();
    }

    public void JoinOrCreateRoom_defaultRoomName() {
        _print(true, "JoinOrCreateRoom_" + defaultRoomName);
        PhotonNetwork.JoinOrCreateRoom(defaultRoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom() {
        base.OnJoinedRoom();
        _print(true, "\nPhotonLobby.OnJoinedRoom(): " + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom.Name != defaultRoomName) {
            _print(true, "PhotonNetwork.CurrentRoom.Name != defaultRoomName");
            _print(true, PhotonNetwork.CurrentRoom.Name + " != " + defaultRoomName);
            JoinOrCreateRoom_defaultRoomName();
        }

        networkEventsEnable();
        _print(true, "Other/Total players in room: " + PhotonNetwork.CountOfPlayersInRooms + " / " + (PhotonNetwork.CountOfPlayersInRooms + 1));
    }



    public override void OnCreatedRoom()
    {
        _print(true, "\nPhotonLobby.OnCreatedRoom()");
        base.OnCreatedRoom();
        roomNumber++;
    }

#region Debugging -------------------------------------------------------------------------------
    private void _print(bool shouldPrint, string msg)
    {
        if (shouldPrint) Debug.Log(msg);
        if (shouldPrint) progressLabel.text += "\n" + msg;
    }
#endregion
    
#region Networking -------------------------------------------------------------------------------
    public override void OnDisconnected(DisconnectCause cause)
    {
        _print(true, $"PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {cause}");
        networkEventsDisable();
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
        if (obj == null || obj.Code == null) 
        {
            _print(true, "invalid EventData obj recieved");
            return;
        }

        object[] datas = (object[])obj.CustomData;
        switch (obj.Code) {
            case COLOR_CHANGE_EVENT:
                _print(true, "received COLOR_CHANGE_EVENT");
                break;
            case BODY_TRACKING_EVENT:
                _print(true, "received BODY_TRACKING_EVENT");

                string coordinateString = (string)datas[0];
                _print(true, coordinateString);
                break;
            default:
                _print(true, "default unhandled obj.Code: " + obj.Code);
                break;
        }
    }
#endregion
}

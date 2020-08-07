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
    private static byte maxPlayersPerRoom = 4;
    private string defaultRoomName = "defaultExerciseRoom";
    RoomOptions roomOptions = new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = maxPlayersPerRoom };

    public static PhotonLobby Lobby;

    private int roomNumber = 1;
    private int userIdCount;

    private void Awake()
    {
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
        // GenericNetworkManager.OnReadyToStartNetwork += StartNetwork;
    }

    public override void OnConnectedToMaster()
    {
        _print(true, "\nPhotonLobby.OnConnectedToMaster() start");
        var randomUserId = UnityEngine.Random.Range(0, 999999);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.UserId = randomUserId.ToString();
        userIdCount++;
        PhotonNetwork.NickName = PhotonNetwork.AuthValues.UserId;
        PhotonNetwork.JoinRandomRoom();
        _print(true, "\nPhotonLobby.OnConnectedToMaster() end");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _print(true, String.Format("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause));
        networkEventsDisable();
        _print(true, String.Format("Cause: ", cause.ToString()));
    }

    public void JoinOrCreateRoom()
    {
        _print(true, "JoinOrCreateRoom " + defaultRoomName);
        PhotonNetwork.JoinOrCreateRoom(defaultRoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        _print(true, "\nPhotonLobby.OnJoinedRoom()");
        _print(true, "Current room name: " + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom.Name != defaultRoomName) {
            _print(true, "PhotonNetwork.CurrentRoom.Name != defaultRoomName");
            _print(true, PhotonNetwork.CurrentRoom.Name + " != " + defaultRoomName);
            JoinOrCreateRoom();
        }

        networkEventsEnable();
        _print(true, "Other players in room: " + PhotonNetwork.CountOfPlayersInRooms);
        _print(true, "Total players in room: " + (PhotonNetwork.CountOfPlayersInRooms + 1));
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        _print(true, "\nPhotonLobby.OnJoinRandomFailed()");
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _print(true, "\nPhotonLobby.OnCreateRoomFailed()");
        Debug.LogError("Creating Room Failed");
        CreateRoom();
    }

    public override void OnCreatedRoom()
    {
        _print(true, "\nPhotonLobby.OnCreatedRoom()");
        base.OnCreatedRoom();
        roomNumber++;
    }

    public void OnCancelButtonClicked()
    {
        _print(true, "\nPhotonLobby.OnCancelButtonClicked()");
        PhotonNetwork.LeaveRoom();
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
        // var roomOptions = new RoomOptions {IsVisible = true, IsOpen = true, MaxPlayers = 10};
        // PhotonNetwork.CreateRoom("Room" + UnityEngine.Random.Range(1, 3000), roomOptions);
        JoinOrCreateRoom();
    }

    private Queue<string> queue;
    private string[] queueArr = new string[5];
    private string queueStr = "";
    private void _print(bool shouldPrint, string msg, bool collapse=false)
    {
        if (collapse) {
            if (queue == null) {
                queue = new Queue<string>();
            }
            queue.Enqueue(msg);
            if (queue.Count > 5) 
            {
                queue.Dequeue();
            }

            queueArr = queue.ToArray();

            queueStr = "";
            foreach (var s in queue.ToArray())
                queueStr += s;
            // mineList.text = queue.Count + queueStr;
            return;
        }
        if (shouldPrint) Debug.Log(msg);
        if (shouldPrint) progressLabel.text += "\n" + msg;
    }

    // -------------------- --------------------
    
        private const byte COLOR_CHANGE_EVENT = 0;

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

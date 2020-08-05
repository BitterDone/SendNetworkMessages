using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using ExitGames.Client.Photon;
using System; // added for String.Format()
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.MyCompany.MyGame
{
    /*
    Skipping The Player Name
    */
    public class LauncherScript : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields
        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private static byte maxPlayersPerRoom = 4;

        private string defaultRoomName = "defaultExerciseRoom";
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = maxPlayersPerRoom };

        #endregion

        #region Public Fields
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private Text progressLabel;
        [SerializeField]
        private Text mineList;
        #endregion


        #region Private Fields
        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>
        string gameVersion = "1";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;
        bool inRoom = false;
        #endregion

        #region MonoBehaviour CallBacks
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            // Connect(); // commented when the Play Button was added
            // progressLabel.SetActive(true); // only for GameObject, changed to Text
            controlPanel.SetActive(true);

            Connect();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            // progressLabel.SetActive(true); // only for GameObject, changed to Text
            controlPanel.SetActive(false);
            
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

    #endregion
    
    #region MonoBehaviourPunCallbacks Callbacks
    public override void OnConnectedToMaster()
    {
        _print(true, "PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
        // we don't want to do anything if we are not attempting to join a room.
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            // PhotonNetwork.JoinRandomRoom();
            _print(true, "JoinOrCreateRoom " + defaultRoomName);
            PhotonNetwork.JoinOrCreateRoom(defaultRoomName, roomOptions, TypedLobby.Default);
            isConnecting = false;
        }
        // if (!PhotonNetwork.InLobby)
        // {
        //     _print(true, "Not inside a Lobby");
        //     LoadBalancingClient.OpJoinLobby(null);
        // }
        }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _print(true, String.Format("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause));
        // progressLabel.SetActive(true); // only for GameObject, changed to Text
        controlPanel.SetActive(true);
        isConnecting = false;
        inRoom = false;
        networkEventsDisable();
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        _print(true, "PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        // PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        PhotonNetwork.CreateRoom(defaultRoomName, roomOptions);
    }

    public override void OnJoinedLobby()
    {
        if (PhotonNetwork.InLobby)
        {
            _print(true, "Inside a Lobby");
        }
        else
        {
            _print(true, "Not inside a Lobby");
            // LoadBalancingClient.OpJoinLobby(null);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        string rooms = "rooms: ";
        foreach (RoomInfo r in roomList)
        {
            // List< RoomInfo >  emptyRoomTtl isOpen isVisible name
            // rooms += String.Format("{0} {1} {2} {3}", r.Name, r.EmptyRoomTtl, r.IsOpen, r.IsVisible) + "\n";
            rooms += r.ToString() + "\n";
        }
        _print(true, rooms);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        printNumberPlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        printNumberPlayers();
    }

    private void printNumberPlayers()
    {
        // _print(true, PhotonNetwork.countOfPlayers);
        _print(true, "CountOfPlayersOnMaster: " + PhotonNetwork.CountOfPlayersOnMaster.ToString());
        _print(true, "CountOfPlayersInRooms: " + PhotonNetwork.CountOfPlayersInRooms.ToString());
        _print(true, "CountOfPlayers: " + PhotonNetwork.CountOfPlayers.ToString());
    }

    public override void OnJoinedRoom()
    {
        _print(true, "PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        // progressLabel.SetActive(true); // only for GameObject, changed to Text

        // // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        _print(true, "Name: " + PhotonNetwork.CurrentRoom.Name);
        _print(true, "PlayerCount: " + PhotonNetwork.CurrentRoom.PlayerCount);
        _print(true, "EmptyRoomTtl: " + PhotonNetwork.CurrentRoom.EmptyRoomTtl);
        // if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        // {
            _print(true, "We load the 'Room for 1' ");


            // if (base.photonView.IsMine) {
                inRoom = true;
                networkEventsEnable();
            // }
        // }
    }

        #endregion
// ------------------------------------------------------------------------------
        #region cubeColorChangeCode

        [SerializeField]
        private GameObject colorChangeCube;

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
                // _print(true, "COLOR_CHANGE_EVENT");
                object[] datas = (object[])obj.CustomData;
                float r = (float)datas[0];
                float g = (float)datas[1];
                float b = (float)datas[2];
                setCubeColor(r, g, b);
            }
            else
            {
                _print(true, "unhandled obj.Code: " + obj.Code);
            }
        }

        public void onClick_changeColor()
        {
            changeColor();
        }

        private void changeColor()
        {
            float r = UnityEngine.Random.Range(0f, 1f);
            float g = UnityEngine.Random.Range(0f, 1f);
            float b = UnityEngine.Random.Range(0f, 1f);

            // colorChangeCube.color = new Color(r, g, b, 1f);
            // setCubeColor(r, g, b);
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {
                // InterestGroup = (byte)view.group,
                Receivers = ReceiverGroup.All,
                // CachingOption = EventCaching.AddToRoomCache
            };

            // cant specify incoming objects
            object[] datas = new object[] { r, g, b }; // base.photonView.ViewID,
            PhotonNetwork.RaiseEvent(COLOR_CHANGE_EVENT, datas, raiseEventOptions, SendOptions.SendUnreliable);
            _print(false, "raised COLOR_CHANGE_EVENT");
            // GO a, b Send RPC on A over network, other clients also get RPC on A
            // send event over network on A, and Object B subscribes to events, both receive the event
            // create RPC behavior unreliably, send photon view id base.photonView.ViewID,
        }

        private void setCubeColor(float r, float g, float b)
        {
            var cubeRenderer = colorChangeCube.GetComponent<Image>();
            cubeRenderer.material.SetColor("_Color", new Color(r, g, b, 1f));
        }

        void Update()
        {
            if (!inRoom)                        { _print(false, "!inRoom"); return; }
            if (!Input.GetKeyDown(KeyCode.Space))   { _print(false, "!KeyCode.Space"); return; }
            // if (base.photonView.IsMine)         { _print(false, "base.mine"); changeColor(); }
            if (photonView.IsMine)              { _print(true, "mine ", true); changeColor(); }
            else                                { _print(true, "not mine"); }

        }

        // Queue queue = new Queue();
        // queue.Enqueue(1);

        // "Number of elements in the Queue: {0}", queue.Count
        // queue.Dequeue()

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
                mineList.text = queue.Count + queueStr;
                return;
            }
            if (shouldPrint) Debug.Log(msg);
            if (shouldPrint) progressLabel.text += "\n" + msg;
        }
    #endregion

    }
}
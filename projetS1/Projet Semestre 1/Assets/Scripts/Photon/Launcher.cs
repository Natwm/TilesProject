﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{


    string gameVersion = "1";
    public string nickname = "SOMEONE";
    [SerializeField]bool isAutoConnect;

    [Space]
    [Header("Room's settings")]
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField] private byte maxPlayersPerRoom = 4;

    // Start is called before the first frame update
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        DontDestroyOnLoad(GameObject.Find("IngameTerminal"));
        DontDestroyOnLoad(GameObject.Find("Canvas"));
        DontDestroyOnLoad(GameObject.Find("EventSystem"));
        DontDestroyOnLoad(gameObject);

        nickname = PlayerPrefs.GetString("Nickname", nickname);
    }

    private void Start()
    {
        if(isAutoConnect)
            Connect();
    }
    // Update is called once per frame
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            //SetPlayerName(nickname);
            PhotonNetwork.ConnectUsingSettings();
        }
    }


    /////////////////////
    //// SERVER CONNECTION CALLBACKS
    /////////////////////////////////////////

    public override void OnConnectedToMaster()
    {
        Debug.LogFormat("Connected to master server.");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }





    ///////////////////////////
    ///// ROOM CONNECTION CALLBACKS
    ////////////////////////////////////////
    // for local client only
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed()");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    // for local client only
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom()");
        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            // #Critical
            // Load the Room Level.
            Debug.LogFormat("LOAD LEVEL from OnJoinedRoom");
            PhotonNetwork.LoadLevel("room");
        }
    }

    // for everyone
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("Player {0} entered the room with nickname {1}", newPlayer.UserId, newPlayer.NickName);

        // if (PhotonNetwork.IsMasterClient)
        // {
        //     Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        //     Debug.LogFormat("Loading arena room....");
        //     LoadArena();
        // }
    }

    // for everyone
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        Debug.LogFormat("Player {0} left the room with nickname {1}", newPlayer.UserId, newPlayer.NickName);
    }




    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        Debug.LogFormat("LOAD LEVEL from LoadArena");
        PhotonNetwork.LoadLevel("room");
    }





    //////////////////////////
    //// USER RELATED STUFF
    ///////////////////////////////
    /// <summary>
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetPlayerName(string value)
    {
        // #Important
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }
        PhotonNetwork.NickName = value;
        Debug.LogWarning("Nickname have change to :" + value);

        PlayerPrefs.SetString("Nickname", value);
    }
}

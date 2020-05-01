using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class NetworkPlayer : MonoBehaviour, IPunObservable, IOnEventCallback
{
  [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
  public static GameObject LocalPlayerInstance;
  public int PlayerID = -1; // -1 means uninitialized

  PhotonView photonView;

  public PlayerCursor cursor;
  
  void Awake()
  {
    photonView = GetComponent<PhotonView>();

    // #Important
    // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
    if (photonView.IsMine)
    {
        NetworkPlayer.LocalPlayerInstance = this.gameObject;
    }
    // #Critical
    // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
    DontDestroyOnLoad(this.gameObject);

    // Add ourself to InputManager
    cursor = GetComponentInChildren<PlayerCursor>();
    NetworkInputManager.playerInputs.Add(cursor);

    Debug.LogFormat("Instanciated player {0}", photonView.Owner.NickName);

    if(PhotonNetwork.IsMasterClient)
    {
      SendPlayerID(photonView.Owner.NickName, PhotonNetwork.CurrentRoom.PlayerCount-1);
    }
  }


  void SendPlayerID(string PlayerName, int ID)
  {
    Debug.LogFormat("master client sending event to players : {0} will be player {1}", PlayerName, ID);
    byte evCode = 1; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
    object[] content = new object[] { PlayerName, ID }; // Array contains the target position and the IDs of the selected units

    RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
    raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
    raiseEventOptions.Receivers = ReceiverGroup.All;
    SendOptions sendOptions = new SendOptions();
    sendOptions.DeliveryMode = DeliveryMode.Reliable;
    PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
  }


  public void OnEnable()
  {
    PhotonNetwork.AddCallbackTarget(this);
  }

  public void OnDisable()
  {
    PhotonNetwork.RemoveCallbackTarget(this);
  }


  public void OnEvent(EventData photonEvent)
  {
    byte eventCode = photonEvent.Code;

    if (eventCode == 1)
    {
      Debug.LogFormat("client {0} received event {1}", photonView.Owner.NickName, photonEvent.Code);
      object[] data = (object[])photonEvent.CustomData;

      string PlayerName = (string)data[0];
      int playerID = (int)data[1];
      
      // get all player instances and set the correct ID
      NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
      for (int i = 0; i < players.Length; i++)
      {
        if(players[i].photonView.Owner.NickName == PlayerName)
        {
          players[i].PlayerID = playerID;
          players[i].transform.GetComponentInChildren<PlayerCursor>().playerID = playerID;
          break;
        }
      }
    }
  }


  public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
  {
  }
}

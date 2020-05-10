﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class PlayerMouvement : MonoBehaviour, IPunObservable, IOnEventCallback
{
    #region Param
    [Header("Debug")]
    [Tooltip("Affiche les différents raycast dans l'inspector")]
    public bool drawDebug;
    public bool isOffline;
    public Vector3 offset;
    
    

    [Space]
    [Header("Player's Settings")]

    [Tooltip("-1 means uninitialized")]
    public int PlayerID = -1;

    [Tooltip("Correspond a l'arc de cercle autours du joueur")]
    [SerializeField] private float FOV;

    PhotonView view;
    private Grid grid;
    List<GameObject> m_Neighbours = new List<GameObject>();

    [Tooltip("Indique si le joueur peut jouer")]
    public bool m_canPlay = false;

    [Tooltip("Indique si le joueur peut effectuer une action")]
    public bool m_ActionPhase = false;

    [Space]
    [Header("Canvas")]
    private TMP_Text infoText;
    private NetworkUi m_Canvas;
    public float tempsAffichagemessage = 2f;

    



    #endregion

    #region Awake || Start
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
        m_Canvas = GameObject.Find("Launcher").GetComponent<NetworkUi>();

        if (view.IsMine)
        {
            NetworkPlayer.LocalPlayerInstance = this.gameObject;
        }

        
        gameObject.name += "_" + view.Owner.NickName;
        gameObject.transform.parent = FindObjectOfType<Grid>().gameObject.transform;

        if (PhotonNetwork.IsMasterClient)
        {
            SendPlayerID(view.Owner.NickName, PhotonNetwork.CurrentRoom.PlayerCount - 1);
        }


        grid = transform.parent.GetComponent<Grid>();
        Debug.Log(m_Canvas);
        m_Canvas.SetGameUI(this,PhotonNetwork.IsMasterClient);
    }
    #endregion

    

    #region LateUpdate || Update || FixedUpdate
    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward * 50);
        if (m_canPlay && !m_ActionPhase)
        {
            //MOUVEMENT BY CLICK
            if (Input.GetButtonDown("Fire1") && GetComponent<PhotonView>().IsMine)
            {
                ApplyDesiredMovement();
                UseFov();
            }
        }
    }

    #endregion

    #region Click Mouvement
    Vector3 CalculeMouvement()
    {
        Vector3 targetpos = new Vector3(-1, -1, -1);
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -1;

        Debug.DrawRay(mousePosition, Vector3.forward);
        RaycastHit hit;
        if(Physics.Raycast(mousePosition, Vector3.forward,out hit, 50f))
        {
            GameObject selectedPosition = hit.collider.gameObject;

            targetpos = new Vector3(grid.LocalToCell(selectedPosition.transform.position).x, grid.LocalToCell(selectedPosition.transform.position).y, -1);
        }
        return targetpos;
    }

    void ApplyDesiredMovement()
    {
        Vector3 targetPosition = CalculeMouvement();
        if(targetPosition != new Vector3(-1, -1 ))
        {
            transform.position = targetPosition;
            SendMouvementDone();
        }
        
    }

    bool ApplyDesiredMovementForward()
    {
        Vector3 targetPosition = CalculeMouvement();
        

        if (CanApplyMouvement(targetPosition))
        {
            transform.position = targetPosition;

            //The player 2 Can play, so change the game State
            return true;
        }
        //The player 1 do a invalid action, do not change the game state*/
        return false;
    }

    bool CanApplyMouvement(Vector3 targetPos)
    {
        return transform.position.x == targetPos.x || transform.position.y == targetPos.y ? true : false;
    }

    #endregion

    #region ArrowKey Movement
    void ApplyDesiredMovementArrowKey(Vector3Int dir)
    {
        Vector3Int currentPos = grid.LocalToCell(transform.position);
        Vector3Int targetpos = currentPos + dir;
        Debug.Log(targetpos);
        transform.position = targetpos;
    }

    void ArrowMouvement()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            NetworkPlayer.LocalPlayerInstance = this.gameObject;

            Vector3Int movementDir = Vector3Int.zero;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                movementDir.x = -1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                movementDir.x = 1;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                movementDir.y = 1;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                movementDir.y = -1;
            }

            if (movementDir != Vector3Int.zero)
            {
                ApplyDesiredMovementArrowKey(movementDir);
            }
        }
    }


    #endregion

    #region METHODE
    void UseFov()
    {
        m_Neighbours.Clear();
        foreach (Collider item in Physics.OverlapSphere(transform.GetChild(0).position, FOV))
        {
            if (item.gameObject.GetComponent<TilesBehaviours>() != null)
            {
                m_Neighbours.Add(item.gameObject);
                //Erreur est ici
                item.gameObject.GetComponent<TilesBehaviours>().callColor();
            }
        }
    }


    void ChangeTurn()
    {
        m_canPlay = true;
        m_Canvas.UpdateInterface(m_ActionPhase, m_canPlay);
    }

    void LaunchGame()
    {
        m_Canvas.StartGameUI();
        if(m_Canvas.UpdateInterface(m_ActionPhase, m_canPlay))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                m_canPlay = true;
                m_Canvas.UpdateInterface(m_ActionPhase, m_canPlay);
                Debug.Log(m_canPlay);
            }
        } 
        
    }

    #endregion

    #region Event 

    void SendSeed()
    {
        byte evCode = 5; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { Random.seed }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    void SendMouvementDone()
    {
        m_canPlay = false;
        m_ActionPhase = true;

        m_Canvas.UpdateInterface(m_ActionPhase, m_canPlay);

        byte evCode = 2; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { 1, "tour" }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    void SendActionDone()
    {
        m_ActionPhase = false;
        m_canPlay = false;

        m_Canvas.UpdateInterface(m_ActionPhase, m_canPlay);
        byte evCode = 3; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { transform.GetChild(0).position, FOV , "Je sais pas encore" }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
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

    public void SendStart()
    {
        Debug.Log("master client sending that the game Start");
        byte evCode = 2; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] {}; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.All;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }


    #endregion

    #region Button
    public void CheckIsNearChest()
    {
        bool result = false;
        Debug.Log(m_Neighbours.Count);
        foreach (GameObject item in m_Neighbours)
        {
            if (item.GetComponent<TilesBehaviours>().IsChest)
            {
                //call l'affichage d'un message 
                result = true;
                break;
            }
            result = false;
        }

        Debug.Log("result" + result);
        StartCoroutine(m_Canvas.ShowInformation(result, false));

        if (view.IsMine)
        {
            SendActionDone();
        }
        
        //message affichage
    }

    public void CheckIsChest()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position + offset, Vector3.forward * 2, out hit, Mathf.Infinity);

        Color color = hit.collider.gameObject != null ? Color.green : Color.red;

        Debug.DrawRay(transform.position + offset, Vector3.forward * 2, color);

        bool result = hit.collider.gameObject.GetComponent<TilesBehaviours>().IsChest;
        Debug.Log(hit.collider.gameObject.name);
    
        StartCoroutine(m_Canvas.ShowInformation(result, true));

        if (view.IsMine)
        {
            SendActionDone();
        }
    }

    public void PassTurn()
    {
        
        Debug.Log("Je passe");
        if (view.IsMine)
        {
            SendActionDone();
        }
    }

    public void GameStart()
    {
        Debug.Log("Start Game");

        if (view.IsMine)
        {
            SendStart();
        }
    }
    #endregion

    #region Affichage

    void UpdateBoard(Vector3 position, float radius, string tag)
    {
        Debug.LogWarning("Modification du plateau ");
        Debug.Log("Le radius " +radius);
        Debug.Log("la position " + position);
        Collider[] effacer = Physics.OverlapSphere(position, radius);

        foreach  (Collider item in effacer)
        {
            if(item.gameObject.GetComponent<TilesBehaviours>() != null)
            {
                Debug.Log(item.gameObject);
                item.gameObject.GetComponent<TilesBehaviours>().EraseTiles();
            }
        }
    }

    #endregion

    #region Interface

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        object[] data;
        switch (eventCode)
        {
            case 1:
                //Debug.LogFormat("client {0} received event {1}", view.Owner.NickName, photonEvent.Code);
                data= (object[])photonEvent.CustomData;
                string playerName = (string)data[0];
                int playerID = (int)data[1];
                SetId(playerName, playerID);
                break;

            case 2:
                data = (object[])photonEvent.CustomData;
                LaunchGame();
                break;

            case 3:
                ChangeTurn();
                data= (object[])photonEvent.CustomData;
                Vector3 pos = (Vector3)data[0];
                float radius = (float)data[1];
                string tag = (string)data[2];
                Debug.LogWarning(pos);
                UpdateBoard(pos, radius, tag);
                break;


        }
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
    }

    #endregion

    #region GETTER && SETTER
    public TMP_Text InfoText { get => infoText; set => infoText = value; }
    public NetworkUi Canva { get => m_Canvas; set => m_Canvas = value; }


    void SetId(string PlayerName, int playerID)
    {
        // get all player instances and set the correct ID
        PlayerMouvement[] players = FindObjectsOfType<PlayerMouvement>();
        for (int i = 0; i < players.Length; i++)
        {
            Player owner = players[i].gameObject.GetComponent<PhotonView>().Owner;
            if (owner.NickName == PlayerName)
            {
                players[i].PlayerID = playerID;
                break;
            }
        }
    }
    #endregion

    #region GIZMO 
    private void OnDrawGizmos()
    {
        if (drawDebug)
        {
            Gizmos.DrawSphere(transform.GetChild(0).position, FOV);

            //axes of Mouvements
            Gizmos.DrawLine(transform.position + offset, (transform.position + Vector3.up) * 3 + offset);
            Gizmos.DrawLine(transform.position + offset, (transform.position + Vector3.down) * 3 + offset);
            Gizmos.DrawLine(transform.position + offset, (transform.position + Vector3.left) * 3 + offset);
            Gizmos.DrawLine(transform.position + offset, (transform.position + Vector3.right) * 3 + offset);
            Gizmos.DrawLine(transform.position + offset, (transform.position + Vector3.forward) * 3 + offset);
        }

    }


    #endregion
}

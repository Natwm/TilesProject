using System.Collections;
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

    [Tooltip("Permet de lancer le jeu en offline ou non")]
    public bool isOffline;

    [Tooltip("Correspond a l'offset utiliser lors des tracer de raycast")]
    public Vector3 offset;


    [Space]
    [Header("Player's Settings")]

    [Tooltip("-1 means uninitialized")]
    public int PlayerID = -1;

    [Tooltip("Correspond a l'arc de cercle autours du joueur")]
    [SerializeField] private float FOV;

    [Tooltip("Indique les actions que le joueur peut faire")]
    [SerializeField]private m_Action m_MyActionPhase = m_Action.Wait;

    [Tooltip("Le nombre maximum de cartes que le joueur peut avoir")]
    [SerializeField] private int nbCardToDraw = 2;

    PhotonView view;
    private Grid grid;
    private GridGen terrain;
    List<GameObject> m_Neighbours = new List<GameObject>();

    [Space]
    [Header("Player's Layer")]
    [Tooltip("Où le joueur peut ce déplacer")]
    [SerializeField] private LayerMask targetLayer;

    [Tooltip("Où le joueur peut ce déplacer")]
    [SerializeField] private LayerMask cardLayer;


    [Space]
    [Header ("Card")]
    public bool canDraw = false;
    public List<Carte> hand;
    public GameObject handGO;
    [SerializeField] private GameObject gameDeck;

    [Space]
    [Header("Canvas")]
    private TMP_Text infoText;
    private NetworkUi m_Canvas;
    public float tempsAffichagemessage = 2f;

    
    public enum m_Action { Mouvement, Action, Wait }

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
        m_Canvas.SetGameUI(this, PhotonNetwork.IsMasterClient);
        gameDeck = GameObject.Find("Deck(Clone)");
    }
    #endregion



    #region LateUpdate || Update || FixedUpdate
    // Update is called once per frame
    void Update()
    {
        if(drawDebug)
            Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward * 50);

        if (PlayerID == 0 && gameDeck.GetComponent<GestionCartes>().ready && hand.Count == 0 && view.IsMine)
        {
            Debug.Log(gameDeck.GetComponent<GestionCartes>().ready);
            DrawCard();
        }

        if (Input.GetKeyDown(KeyCode.E) && view.IsMine)
        {
            foreach (var item in hand)
            {
                Debug.LogWarning(item.cardName);
            }
            
            //ChangeCard();
            //Debug.Log(gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand.Count);
        }

        if (m_MyActionPhase == m_Action.Mouvement)
        {
            //MOUVEMENT BY CLICK
            if (Input.GetButtonDown("Fire1") && GetComponent<PhotonView>().IsMine)
            {
                ApplyDesiredMovement();
                UseFov();
            }
        }

        if(m_MyActionPhase == m_Action.Action)
        {
            if (Input.GetButtonDown("Fire1") && GetComponent<PhotonView>().IsMine)
            {
                ChangemyCard();
            }   
        }
    }

    #endregion

    #region Click Mouvement
    Vector3 CalculeMouvement()
    {
        Vector3 targetpos = transform.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -1;

        Debug.DrawRay(mousePosition, Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(mousePosition, Vector3.forward, out hit, 50f, targetLayer))
        {
            GameObject selectedPosition = hit.collider.gameObject;
            if(selectedPosition.GetComponent<CellData>().state == CellData.m_State.Show)
                targetpos = new Vector3(grid.LocalToCell(selectedPosition.transform.position).x, grid.LocalToCell(selectedPosition.transform.position).y, -1);
        }
        return targetpos;
    }

    void ApplyDesiredMovement()
    {
        Vector3 targetPosition = CalculeMouvement();
        if (targetPosition != new Vector3(-1, -1))
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
        //m_canPlay = true;
        m_MyActionPhase = m_Action.Mouvement;
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
    }

    void LaunchGame()
    {
        m_Canvas.StartGameUI();
        GestionCartes deck = gameDeck.GetComponent<GestionCartes>();
        terrain = GameObject.FindObjectOfType<GridGen>();
        Instantiate(handGO).transform.parent = GameObject.Find("GamePanel").transform;

        if (PhotonNetwork.IsMasterClient)
        {
            m_MyActionPhase = m_Action.Mouvement;
        }
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);

       /* if (PhotonNetwork.IsMasterClient && canDraw)
        {
            DrawCard();
        }*/
    }

    public void DrawCard()
    {
        Debug.Log("j'ai ma carte et je suis " + this.name);
        Debug.Log("je pioche et il y a carte : " + gameDeck.GetComponent<GestionCartes>().allCards.Count);
        for (int y = 0; y < nbCardToDraw; y++)
        {
            int cardIndex = Random.Range(0, gameDeck.GetComponent<GestionCartes>().allCards.Count);
            Carte cardDraw = gameDeck.GetComponent<GestionCartes>().allCards[cardIndex];
            gameDeck.GetComponent<GestionCartes>().allCards.RemoveAt(cardIndex);
            Debug.Log("ma carte : " + cardDraw.cardName);
            hand.Add(cardDraw);
            gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand.Add(cardDraw);
            cardDraw.Create();
        }
        if(hand.Count == 2)
        {
            Debug.Log("il reste cate : " + gameDeck.GetComponent<GestionCartes>().allCards.Count);
            terrain.HighlightTypeOfCell(hand[0].cardType);
            SendPlayerhasDraw();
        }
            
    }

    void ModifDeck(int handID, int hand1ID)
    {
        Debug.Log("Avant " + gameDeck.GetComponent<GestionCartes>().allCards.Count);
        //faire un for récuperer la position de l'objet et sup après
        List<Carte> deck = gameDeck.GetComponent<GestionCartes>().allCards;
        List<Carte> toRemove = new List<Carte>();

        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i].cardId == handID || deck[i].cardId == hand1ID)
            {
                toRemove.Add(deck[i]);
            }
        }

        foreach (Carte item in toRemove)
        {
            gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand.Add(item);
            gameDeck.GetComponent<GestionCartes>().allCards.Remove(item);
        }
        Debug.Log("j'ai ma carte");
        Debug.Log("Après " + gameDeck.GetComponent<GestionCartes>().allCards.Count);
    }

    void ChangeCard(Carte cardToDrop)
    {
        //showdeck();
        
        Debug.Log("je change de carte");
        int newCardIndex = Random.Range(0, gameDeck.GetComponent<GestionCartes>().allCards.Count);
        Debug.Log("je change de carte avec la cartes numéro : " + newCardIndex);
        Carte drop = cardToDrop;
        Carte get = gameDeck.GetComponent<GestionCartes>().allCards[newCardIndex];

        gameDeck.GetComponent<GestionCartes>().allCards.Add(drop);
        gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand.Remove(drop);
        hand.Remove(drop);
        Destroy(drop.ingameDisplay);

        hand.Add(get);
        get.Create();
        gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand.Add(get);
        gameDeck.GetComponent<GestionCartes>().allCards.Remove(get);

        Debug.Log("Je met " + drop.cardName +" dans la pioche et je prend "+ get.cardName +" dans ma main" );

        SendDeckHasChange(drop, get);
    }

    void ChangemyCard()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -1;
        RaycastHit hit;

        if (Physics.Raycast(mousePosition, Vector3.forward, out hit, Mathf.Infinity, cardLayer))
        {
            Debug.Log(hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.cardName);
            Carte toDrop = hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead;
            ChangeCard(toDrop);
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
        // m_canPlay = false;
        //m_ActionPhase = true;
        m_MyActionPhase = m_Action.Action;

        m_Canvas.UpdateInterface(m_MyActionPhase, hand);

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
        /*m_ActionPhase = false;
        m_canPlay = false;*/
        m_MyActionPhase = m_Action.Wait;

        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
        byte evCode = 3; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { transform.GetChild(0).position, FOV, "Je sais pas encore" }; // Array contains the target position and the IDs of the selected units

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
        object[] content = new object[] { }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.All;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    public void SendPlayerhasDraw()
    {
        Debug.Log("A player have draw");
        byte evCode = 4; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] {PlayerID,hand[0].cardId, hand[1].cardId }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    public void SendDeckHasChange(Carte drop, Carte get)
    {
        Debug.Log("A player have draw");
        byte evCode = 5; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { drop.cardId, get.cardId}; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
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
            if (item.GetComponent<CellData>().isTreasure)
            {
                //call l'affichage d'un message 
                result = true;
                break;
            }
            result = false;
        }

        Debug.Log("is CheckIsNearChest =" + result);
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

        bool result = hit.collider.gameObject.GetComponent<CellData>().isTreasure;
        Debug.Log(hit.collider.gameObject.name);
        Debug.Log("is CheckIsChest =" + result);
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
        Debug.Log("Le radius " + radius);
        Debug.Log("la position " + position);
        Collider[] effacer = Physics.OverlapSphere(position, radius);

        foreach (Collider item in effacer)
        {
            if (item.gameObject.GetComponent<TilesBehaviours>() != null)
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
                data = (object[])photonEvent.CustomData;
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
                data = (object[])photonEvent.CustomData;
                Vector3 pos = (Vector3)data[0];
                float radius = (float)data[1];
                string tag = (string)data[2];
                Debug.LogWarning(pos);
                UpdateBoard(pos, radius, tag);
                break;

            case 4:
                data = (object[])photonEvent.CustomData;
                int playerId = (int)data[0];
                int handID = (int)data[1];
                int hand1ID = (int)data[2];
                Debug.Log(hand1ID + "recu le deck"+ handID);

                ModifDeck(hand1ID, handID);
                if (PlayerID == playerId + 1)
                {
                    DrawCard();
                }
                break;

            case 5:
                data = (object[])photonEvent.CustomData;
                int carteToDeckId = (int)data[0];
                int carteToHandId = (int)data[1];
                Carte toRemove;

                foreach (Carte carte in gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand)
                {
                    Debug.Log("carte.cardId = " + carte.cardName + " carteId = " + carteToDeckId);
                    if(carte.cardId == carteToDeckId)
                    {
                        Debug.Log("it's true");
                        toRemove = carte;
                        gameDeck.GetComponent<GestionCartes>().allCards.Add(carte);
                        gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand.Remove(toRemove);
                        break;
                    }
                }

                foreach (Carte carte in gameDeck.GetComponent<GestionCartes>().allCards)
                {
                    Debug.Log("carte.cardId = " + carte.cardName + " carteId = " + carteToDeckId);
                    if (carte.cardId == carteToHandId)
                    {
                        Debug.Log("it's true");
                        toRemove = carte;
                        gameDeck.GetComponent<GestionCartes>().allCards.Remove(carte);
                        gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand.Add(toRemove);
                        break;
                    }
                }

                //if (toRemove != null)
                //  gameDeck.GetComponent<GestionCartes>().allCardsinPlayerHand.Remove(toRemove);
                //showdeck();
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
    public int NbCardToDraw { get => nbCardToDraw; set => nbCardToDraw = value; }

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

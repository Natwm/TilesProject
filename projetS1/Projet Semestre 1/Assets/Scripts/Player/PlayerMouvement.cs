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
    [SerializeField] private m_Action m_MyActionPhase = m_Action.Wait;

    [Tooltip("Le nombre maximum de cartes que le joueur peut avoir")]
    [SerializeField] private int nbCardToDraw = 2;

    [Tooltip("Indique la catégorie de la bombe que le joueur utilise")]
    [SerializeField] private Bomb m_MyBomb = Bomb.Nothing;

    
    List<GameObject> m_Neighbours = new List<GameObject>();
    List<GameObject> share = new List<GameObject>();

    [Space]
    [Header("Player Component")]
    private CharacterController characterController;
    public Camera cam;
    PhotonView view;
    private Grid grid;
    private GridGen terrain;

    [Space]

    [Header("Environment Check Properties")]
    public GameObject groundCheck;
    public LayerMask groundMask;
    [SerializeField] private float groundDistance;
    [SerializeField] private float interactDistance;
    [SerializeField] private bool isGrounded;

    [Space]

    [Header("Movement Variable")]
    [SerializeField]
    private float speedMouvement = 10.0f;
    [SerializeField]
    private float gravity = -9.51f;
    Vector3 velocity;

    private GameObject tile;
    public GameObject interactTile;

    [Space]
    [Header("Player's Layer")]
    [Tooltip("Où le joueur peut ce déplacer")]
    [SerializeField] private LayerMask targetLayer;

    [Tooltip("Où le joueur peut ce déplacer")]
    [SerializeField] private LayerMask cardLayer;


    [Space]
    [Header("Mine")]
    [SerializeField] int amountOfRedMines = 2;
    [SerializeField] int amountOfWhiteMines = 3;
    [SerializeField] int amountOfBlackMines = 3;

    [Space]
    [Header("Card")]
    public bool canDraw = false;
    public List<Carte> hand;
    public GameObject handGO;
    [SerializeField] private GameObject gameDeck;

    [Space]
    [Header("Canvas")]
    public float tempsAffichageMessage = 2f;
    private TMP_Text infoText;
    private NetworkUi m_Canvas;

    [Space]
    [Header("Other")]
    [SerializeField] private float TimeCardAvailable = 2.0f;
    [SerializeField] private bool turnCard = false;

    public enum m_Action { Mouvement, Action, Wait, End }
    public enum Bomb { RED, BLACK, WHITE, Nothing }

    #endregion

    #region Awake || Start

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
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

    void Update()
    {
        physicsCheck();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (view.IsMine)
        {
            if (PlayerID == 0 && gameDeck.GetComponent<GestionCartes>().ready && hand.Count == 0)
            {
                Debug.Log("Les cartes sont générer" + gameDeck.GetComponent<GestionCartes>().ready);
                MakeHand();
            }

            if (isGrounded  && view.IsMine && m_MyActionPhase != m_Action.End)
            {
                FpsMove();
                if (tile != null && Input.GetKeyDown(KeyCode.E) && m_MyActionPhase == m_Action.Mouvement)
                {
                    interactTile = tile;
                    UseFov();
                    SendMouvementDone();
                }
                if (tile != null && Input.GetKeyDown(KeyCode.E) && m_MyActionPhase == m_Action.Action && m_MyBomb != Bomb.Nothing)
                {
                    interactTile = tile;
                    PlayerAction();
                }
            }
        }
    }

    #endregion

    #region Mouvement
    #region FPS Mouvement

    void FpsMove()
    {
            float moveVertical = Input.GetAxis("Vertical");
            float moveHorizontal = Input.GetAxis("Horizontal");

            Vector3 mouvement = (transform.right * moveHorizontal + transform.forward * moveVertical) * speedMouvement;
            characterController.Move(mouvement * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
    }

    #endregion

    #region Click Mouvement

    /// <summary>
    /// Apply the movement
    /// </summary>
    void ApplyDesiredMovement()
    {
        Vector3 targetPosition = CalculeMouvement();
        foreach (PlayerMouvement players in GameObject.FindObjectsOfType<PlayerMouvement>())
        {
            if (targetPosition == players.gameObject.transform.position)
            {
                return;
            }
        }
        transform.position = targetPosition;
        UseFov();
        SendMouvementDone();
    }

    /// <summary>
    /// calculate and check if the mouvement is possible
    /// </summary>
    /// <returns></returns>
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

            if (selectedPosition.GetComponent<CellData>().State == CellData.m_State.Show)
                targetpos = new Vector3(grid.LocalToCell(selectedPosition.transform.position).x, grid.LocalToCell(selectedPosition.transform.position).y, -1);
        }
        return targetpos;
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

    #endregion

    #region METHODE
    public void UseFov()
    {
        Debug.Log("use FOV");
        m_Neighbours.Clear();

        Vector3 pos = new Vector3(grid.LocalToCell(interactTile.transform.position).x + offset.x,0, grid.LocalToCell(interactTile.transform.position).y + offset.y);
        Debug.LogError(pos);
        foreach (Collider item in Physics.OverlapSphere(pos, FOV))
        {
            if (item.gameObject.GetComponent<CellData>() != null)
            {
                m_Neighbours.Add(item.gameObject);
                item.gameObject.GetComponent<CellData>().ShowTile(gameObject.name);

                if (item.gameObject.GetComponent<CellData>().IsBomb())
                    bombImact(item.gameObject.GetComponent<CellData>());
            }
        }
    }
   
    void TurnCard(bool canturnCard)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -1;
        RaycastHit hit;
        if (Physics.Raycast(mousePosition, Vector3.forward, out hit, Mathf.Infinity, cardLayer))
        {
            if (hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.CanTurnCard)
            {
                hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.TurnCard();
                Destroy(hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.ingameDisplay);
                hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.Create();
            }
            else if (canturnCard)
            {
                hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.CanTurn();
                hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.TurnCard();
                Destroy(hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.ingameDisplay);
                hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.Create();

                SendUnlockCard(hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.cardName);
            }
            else
                Debug.Log("nothing append");

            turnCard = false;
            Debug.Log("nothing append" + canturnCard);
        }

    }

    void bombImact(CellData item)
    {
        List<Mine> affect = new List<Mine>();
        foreach (Mine mine in item.Listbomb)
        {
            if (mine.BombOwner != gameObject.name)
            {
                Debug.LogWarning("Ajout dans affect, il est a : " +mine.BombOwner +" et c'est une " + mine.BombState);
                affect.Add(mine);
            }
        }
        if (affect.Count > 0)
        {
            foreach ( Mine elt in affect)
            {
                SendBombTrigger(elt.BombState, item.gameObject.transform.parent.name, elt.BombOwner);
                item.ResetTile(elt);
            }
        }
    }
    void BombTrigger(CellData bomb)
    {
        List<Mine> bombtoReset = new List<Mine>(); 
        foreach (var item in bomb.Listbomb)
        {
            if(item.BombOwner == gameObject.name)
            {
                Mine.m_BombState typeOfBomb = item.BombState;
                switch (typeOfBomb)
                {
                    case Mine.m_BombState.BLACK:
                        Debug.Log("black");
                        BLACKtrigger();
                        break;
                    case Mine.m_BombState.WHITE:
                        Debug.Log("white");
                        WHITEtrigger();
                        break;
                    case Mine.m_BombState.Nothing:
                        Debug.Log("Nothing");
                        break;
                    case Mine.m_BombState.RED:
                        Debug.Log("Red");
                        StartCoroutine(REDtrigger());
                        break;
                    default:
                        Debug.Log("Default");
                        break;
                }
                bombtoReset.Add(item);
            }
            else
            {
                Debug.Log("Ce n'est pas ma bombe " + item.BombState);
            }
        }
        if (bombtoReset.Count > 0)
        {
            foreach (var mine in bombtoReset)
            {
                bomb.ResetTileMine(mine);
            }
        }
    }

    IEnumerator REDtrigger()
    {
        foreach (Carte item in hand)
        {
            Destroy(item.ingameDisplay);
        }
        Debug.LogWarning("C'est une ROUGE !");
        foreach (Carte item in gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand)
        {
            item.Create();
            Debug.Log(item.cardName);
        }
        yield return new WaitForSeconds(TimeCardAvailable);

        foreach (Carte item in gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand)
        {
            Destroy(item.ingameDisplay);
        }
        foreach (Carte item in hand)
        {
            item.Create();
        }
    }

    void BLACKtrigger()
    {
        DrawCard();
    }

    void WHITEtrigger()
    {
        Debug.LogWarning("C'est une BLANCHE !");
        turnCard = true;
        Debug.Log("ok");
    }

    void highlightTiles()
    {
        foreach (var item in m_Neighbours)
        {
            item.GetComponent<CellData>().Bomb(gameObject.name);
        }
    }

    void LaunchGame()
    {
        Debug.Log("Start");
        m_Canvas.StartGameUI();
        GestionCartes deck = gameDeck.GetComponent<GestionCartes>();
        terrain = GameObject.FindObjectOfType<GridGen>();
        Instantiate(handGO).transform.parent = GameObject.Find("GamePanel").transform;

        m_MyActionPhase = m_Action.Mouvement;

        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
    }

    void PlayerAction()
    {
        /*if (Physics.Raycast(mousePosition, Vector3.forward, out hit, Mathf.Infinity, cardLayer))
        {
            ChangeMyCard(hit);
            m_MyActionPhase = m_Action.Wait;
        }*/

        if (interactTile != null)
        {
            if (interactTile.GetComponent<CellData>().CanPlantBomb && m_MyBomb != Bomb.Nothing)
            {
                foreach (var item in m_Neighbours)
                {
                    item.GetComponent<CellData>().ShowTile(gameObject.name);
                    item.GetComponent<CellData>().CanPlantBomb = false;
                }
                interactTile.GetComponent<CellData>().PlantBomb(m_MyBomb, gameObject.name);

                SendDropMine(m_MyBomb, interactTile.gameObject.transform.parent.name);
                
                m_MyBomb = Bomb.Nothing;

                SendActionDone();
            }
        }
    }

    void ChangePhase(string playerName, int playerPhase, bool actionPhase)
    {
        switch (playerPhase)
        {
            case 0:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.Mouvement;
                break;

            case 1:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.Action;
                
                break;

            case 2:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.Wait;
                break;

            case 3:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.End;
                break;
        }
        if (actionPhase)
            ActionPhase();
        else
            ChangeTurn();
    }

    void ActionPhase()
    {
        Debug.Log("action");
        PlayerMouvement[] players = GameObject.FindObjectsOfType<PlayerMouvement>();
        foreach (PlayerMouvement player in players)
        {
            if(player.m_MyActionPhase != m_Action.Wait)
            {
                return;
            }
        }
        if (view.IsMine)
        {
           Debug.Log("action");
           SendActionPhase();
        }
        
    }
    void ChangeTurn()
    {
        Debug.Log("Mouvement");
        PlayerMouvement[] players = GameObject.FindObjectsOfType<PlayerMouvement>();
        foreach (PlayerMouvement player in players)
        {
            if (player.m_MyActionPhase != m_Action.Wait)
            {
                Debug.Log("pas tous le monde a joué");
                return;
            }
        }
        if (view.IsMine && interactTile !=null)
        {
            Debug.Log("Mouvement");
            SendChangeTurn();
        }
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
    }

    public void MakeHand()
    {
        Debug.Log("j'ai ma carte et je suis " + this.name);
        Debug.Log("je pioche et il y a carte : " + gameDeck.GetComponent<GestionCartes>().allCards.Count);
        for (int y = 0; y < nbCardToDraw; y++)
        {
            if (gameDeck.GetComponent<GestionCartes>().allCards.Count > 0)
                DrawCard();
        }
        if (hand.Count == 2)
        {
            Debug.Log("il reste carte : " + gameDeck.GetComponent<GestionCartes>().allCards.Count);
            terrain.HighlightTypeOfCell(hand[0].cardType);

        }

    }

    Carte DrawCard()
    {
        int cardIndex = Random.Range(0, gameDeck.GetComponent<GestionCartes>().allCards.Count);
        Carte cardDraw = gameDeck.GetComponent<GestionCartes>().allCards[cardIndex];
        gameDeck.GetComponent<GestionCartes>().allCards.RemoveAt(cardIndex);
        Debug.Log("ma carte : " + cardDraw.cardName);
        hand.Add(cardDraw);
        gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand.Add(cardDraw);
        cardDraw.Create();

        SendPlayerhasDraw(cardDraw);

        return cardDraw;
    }

    void ModifDeck(string handID)
    {
        //Debug.Log("Avant " + gameDeck.GetComponent<GestionCartes>().allCards.Count);
        //faire un for récuperer la position de l'objet et sup après
        List<Carte> deck = gameDeck.GetComponent<GestionCartes>().allCards;
        List<Carte> toRemove = new List<Carte>();

        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i].cardName == handID)
            {
                toRemove.Add(deck[i]);
            }
        }

        foreach (Carte item in toRemove)
        {
            gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand.Add(item);
            gameDeck.GetComponent<GestionCartes>().allCards.Remove(item);
        }
        Debug.Log("j'ai ma carte");
        //Debug.Log("Après " + gameDeck.GetComponent<GestionCartes>().allCards.Count);
    }

    void ChangeCard(Carte cardToDrop)
    {
        //showdeck();

        Debug.Log("je change de carte");
        int newCardIndex = Random.Range(0, gameDeck.GetComponent<GestionCartes>().allCards.Count);
        Debug.Log("je change de carte avec la cartes numéro : " + newCardIndex);
        Carte drop = cardToDrop;

        gameDeck.GetComponent<GestionCartes>().allCards.Add(drop);
        gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand.Remove(drop);
        hand.Remove(drop);
        Destroy(drop.ingameDisplay);

        Carte get = DrawCard();

        Debug.Log("Je met " + drop.cardName + " dans la pioche et je prend " + get.cardName + " dans ma main");

        SendDeckHasChange(drop, get);
    }

    void ChangeMyCard(RaycastHit hit)
    {
        Debug.Log(hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead.cardName);
        Carte toDrop = hit.collider.gameObject.transform.GetChild(0).GetComponent<CardReader>().cardToRead;
        ChangeCard(toDrop);
    }

    public void PlantBombBlack()
    {
        if (amountOfBlackMines > 0)
        {
            highlightTiles();
            m_MyBomb = Bomb.BLACK;
            amountOfBlackMines--;
        }
        
    }

    public void PlantBombRed()
    {
        if(amountOfRedMines > 0)
        {
            highlightTiles();
            m_MyBomb = Bomb.RED;
            amountOfRedMines--;
        }
        
    }

    public void PlantBombWhite()
    {
        if(amountOfWhiteMines > 0)
        {
            highlightTiles();
            m_MyBomb = Bomb.WHITE;
            amountOfWhiteMines--;
        }
        
    }


    void UpdateDeck(int carteToDeckId, int carteToHandId)
    {
        Carte toRemove;

        foreach (Carte carte in gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand)
        {
            Debug.Log("carte.cardId = " + carte.cardName + " carteId = " + carteToDeckId);
            if (carte.cardId == carteToDeckId)
            {
                Debug.Log("it's true");
                toRemove = carte;
                gameDeck.GetComponent<GestionCartes>().allCards.Add(carte);
                gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand.Remove(toRemove);
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
                gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand.Add(toRemove);
                break;
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

    void SendPlayerID(string PlayerName, int ID)
    {
        Debug.LogFormat("master client sending event to players : {0} will be player {1}", PlayerName, ID);
        byte evCode = 0; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
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
        PlayerMouvement[] players = GameObject.FindObjectsOfType<PlayerMouvement>();
        if(players.Length > 1)
        {
            Debug.Log("master client sending that the game Start");
            byte evCode = 1; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
            object[] content = new object[] { }; // Array contains the target position and the IDs of the selected units

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
            raiseEventOptions.Receivers = ReceiverGroup.All;
            SendOptions sendOptions = new SendOptions();
            sendOptions.DeliveryMode = DeliveryMode.Reliable;
            PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
        } 
    }

    public void SendPlayerhasDraw(Carte myCard)
    {
        Debug.LogWarning("SendPlayerhasDraw");
        Debug.Log("A player have draw");
        byte evCode = 2; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { PlayerID, myCard.cardName, hand.Count }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    public void SendDeckHasChange(Carte drop, Carte get)
    {
        Debug.LogWarning("SendDeckHasChange");
        m_MyActionPhase = m_Action.Wait;

        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
        Debug.Log("A player have draw");
        byte evCode = 3; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { drop.cardId, get.cardId }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    /// <summary>
    /// This method sends a message to other players that he's/she's waiting to be on action phase .
    /// </summary>
    void SendMouvementDone()
    {
        Debug.LogWarning("send action done");
        m_MyActionPhase = m_Action.Wait;
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);

        byte evCode = 4; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { gameObject.name, 2 }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);

    }

    /// <summary>
    /// This method sends a message to other players that he's/she's waiting to be on action phase.
    /// </summary>
    void SendActionPhase()
    {
        Debug.LogWarning("SendActionPhase");
        byte evCode = 5; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] {name }; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.All;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    /// <summary>
    /// This method sends a message to other players that that you did an action.
    /// </summary>
    void SendActionDone()
    {
        m_MyActionPhase = m_Action.Wait;
        Debug.LogWarning("SendActionDone");

        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
        byte evCode = 6; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { transform.GetChild(0).transform.position, FOV, "Je sais pas encore",name, PlayerID }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    /// <summary>
    /// This method sends a message to other players that you drop a mine on a tile.
    /// </summary>
    /// <param name="bombState"> the type of the bomb </param>
    /// <param name="tileName"> the name of the tile /his position </param>
    void SendDropMine(Bomb bombState, string tileName)
    {
        Debug.LogWarning("SendDropMine");

        int indexOfMine;

        switch (bombState)
        {
            case Bomb.RED:
                indexOfMine = 0;
                break;

            case Bomb.BLACK:
                indexOfMine = 1;
                break;

            case Bomb.WHITE:
                indexOfMine = 2;
                break;

            case Bomb.Nothing:
                indexOfMine = 3;
                break;

            default:
                indexOfMine = -1;
                break;
        }

        byte evCode = 7; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { indexOfMine, tileName, this.name }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    /// <summary>
    /// This method sends a message to other players that she/he can change his action phase.
    /// </summary>
    void SendChangeTurn()
    {
        //Debug.LogWarning("SendChangeTurn" + interactTile.transform.position);

        m_MyActionPhase = m_Action.Wait;

        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
        byte evCode = 8; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { interactTile.transform.position, FOV, "Je sais pas encore", name, PlayerID }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.All;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    /// <summary>
    /// This method sends a message to other players that she/he trigger one of your mine.
    /// </summary>
    /// <param name="bombState"> the type of the bomb</param>
    /// <param name="tileName"> the name of the tile / his position </param>
    /// <param name="bombOwner"> the owner of the mine</param>
    void SendBombTrigger(Mine.m_BombState bombState, string tileName, string bombOwner)
    {
        int indexOfMine;

        switch (bombState)
        {
            case Mine.m_BombState.RED:
                indexOfMine = 0;
                break;

            case Mine.m_BombState.BLACK:
                indexOfMine = 1;
                break;

            case Mine.m_BombState.WHITE:
                indexOfMine = 2;
                break;

            case Mine.m_BombState.Nothing:
                indexOfMine = 3;
                break;

            default:
                indexOfMine = -1;
                break;
        }

        Debug.LogWarning("SendBombTrigger");

        byte evCode = 9; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { indexOfMine, tileName, bombOwner }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    /// <summary>
    /// This method sends a message to other players that a card has been unlock.
    /// </summary>
    /// <param name="cardName">
    ///     The name of the card
    /// </param>
    void SendUnlockCard(string cardName)
    {
        Debug.LogWarning("SendUnlockCard");

        byte evCode = 10; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { cardName }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    /// <summary>
    /// This method sends a message to other players that he found the treasure
    /// </summary>
    void SendPlayerGetTreasure()
    {
        Debug.LogWarning("SendPlayerGetTreasure");

        byte evCode = 11; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { this.gameObject.name }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.All;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    #endregion

    #region Affichage
    /// <summary>
    /// Update the game board for a player
    /// </summary>
    /// <param name="position"> this is the position of the opponent who mask some of yours tiles </param>
    /// <param name="radius"> this corresponds to the lenght of his FOV</param>
    /// <param name="tag"> the tag</param>
    void UpdateBoard(Vector3 position, float radius, string tag)
    {
        Debug.Log("je modifie le plateau");
        foreach (Collider item in Physics.OverlapSphere(position, radius))
        {
            if (item.gameObject.GetComponent<CellData>() != null && m_Neighbours.Count > 0)
            {
                Debug.Log("il possède un celldata");
                if (m_Neighbours.Contains(item.gameObject))
                {
                    Debug.Log("il est partagé par les deux joueurs");
                }
                else
                {
                    Debug.Log("il n'est pas partagé");
                    item.gameObject.GetComponent<CellData>().HideTile(gameObject.name);
                }
            }
            else
            {
                Debug.Log("il n'a pas de celldata");
            }
        }
    }

    #endregion

    #region Button
    /// <summary>
    /// This method checks if the chest who you're looking for is in the 8 tiles around you.
    ///     <remarks>
    ///         This method is called during the action phase.
    ///     </remarks>
    /// </summary>
    public void CheckIsNearChest()
    {
        if (m_MyBomb != Bomb.Nothing)
        {
            foreach (var item in m_Neighbours)
            {
                item.GetComponent<CellData>().ShowTile(gameObject.name);
                item.GetComponent<CellData>().CanPlantBomb = false;
            }
        }

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
        Debug.LogError(view.Owner.NickName);
        if (view.IsMine && gameObject.name.Contains(view.Owner.NickName))
        {
            m_MyActionPhase = m_Action.Wait;
            SendActionDone();
        }
    }

    /// <summary>
    /// This method checks if the time where you contain the chest who you're looking for
    ///     <remarks>
    ///         This method is called during the action phase.
    ///     </remarks>
    /// </summary>
    public void CheckIsChest()
    {
        if (m_MyBomb != Bomb.Nothing)
        {
            foreach (var item in m_Neighbours)
            {
                item.GetComponent<CellData>().ShowTile(gameObject.name);
                item.GetComponent<CellData>().CanPlantBomb = false;
            }
        }

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
            if (!result)
            {
                m_MyActionPhase = m_Action.Wait;
                SendActionDone();
            }
            else
                SendPlayerGetTreasure();
        }
    }


    /// <summary>
    /// This method is call when a player don't want to make an action during the action phase
    /// </summary>
    public void PassTurn()
    {
        if(m_MyBomb != Bomb.Nothing)
        {
            foreach (var item in m_Neighbours)
            {
                item.GetComponent<CellData>().ShowTile(gameObject.name);
                item.GetComponent<CellData>().CanPlantBomb = false;
            }
        }

        Debug.Log("Je passe");
        Debug.Log(view.Owner.NickName + "  " + gameObject.name +"   " + (view.IsMine && gameObject.name.Contains(view.Owner.NickName)));
        if (view.IsMine && gameObject.name.Contains(view.Owner.NickName))
        {
            m_MyActionPhase = m_Action.Wait;
            SendActionDone();
        }
    }

    /// <summary>
    /// Start the game when the master client decide
    /// </summary>
    public void GameStart()
    {
        Debug.Log("Start Game");
        
        if (view.IsMine)
        {
            SendStart();
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
            case 0:
                data = (object[])photonEvent.CustomData;
                string playerName = (string)data[0];
                int playerID = (int)data[1];
                SetId(playerName, playerID);
                break;

            case 1:
                data = (object[])photonEvent.CustomData;
                LaunchGame();
                break;

            case 2:
                data = (object[])photonEvent.CustomData;
                playerID = (int)data[0];
                string handID = (string)data[1];
                int nbCardInHand = (int)data[2];

                ModifDeck(handID);
                if (PlayerID == playerID + 1 && nbCardInHand == 2)
                {
                    MakeHand();
                }
                break;

            case 3:
                data = (object[])photonEvent.CustomData;
                int carteToDeckId = (int)data[0];
                int carteToHandId = (int)data[1];

                UpdateDeck(carteToDeckId, carteToHandId);
                break;

            case 4:
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[0];
                int phase = (int)data[1];
                Debug.Log("mouvement " + playerName);
                ChangePhase(playerName, phase, true);
                break;

            case 5:
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[0];
                Debug.Log("reçu 5 de la part de +" + playerName);
                m_MyActionPhase = m_Action.Action;
                m_Canvas.UpdateInterface(m_MyActionPhase, hand);
                break;

            case 6:
                //ChangeTurn();
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[3];

                ChangePhase(playerName, 2, false);

                break;

            case 7:
                data = (object[])photonEvent.CustomData;
                int typeofMine = (int)data[0];
                string tileName = (string)data[1];
                string ownerName = (string)data[2];

                switch (typeofMine)
                {
                    case 0:
                        GameObject.Find(tileName).transform.GetChild(0).GetComponent<CellData>().UpdateBombState(Bomb.RED, ownerName);
                        break;
                    case 1:
                        GameObject.Find(tileName).transform.GetChild(0).GetComponent<CellData>().UpdateBombState(Bomb.BLACK, ownerName);
                        break;
                    case 2:
                        GameObject.Find(tileName).transform.GetChild(0).GetComponent<CellData>().UpdateBombState(Bomb.WHITE, ownerName);
                        break;
                    case 3:
                        GameObject.Find(tileName).transform.GetChild(0).GetComponent<CellData>().UpdateBombState(Bomb.Nothing, ownerName);
                        break;
                    default:
                        GameObject.Find(tileName).transform.GetChild(0).GetComponent<CellData>().UpdateBombState(Bomb.Nothing, ownerName);
                        break;
                }
                ChangeTurn();
                break;

            case 8:
                data = (object[])photonEvent.CustomData;
                Vector3 pos = (Vector3)data[0];
                float radius = (float)data[1];
                string tag = (string)data[2];
                playerName = (string)data[3];
                int playerId = (int)data[4];

                if (playerId != this.PlayerID)
                {
                    UpdateBoard(pos, radius, tag);
                }
                    
                if (m_MyActionPhase == m_Action.Wait && interactTile != null)
                    SendChangeTurn();

                m_MyActionPhase = m_Action.Mouvement;
                m_Canvas.UpdateInterface(m_MyActionPhase, hand);
                break;

            case 9:
                data = (object[])photonEvent.CustomData;
                int typeOfMine = (int)data[0];
                tileName = (string)data[1];
                ownerName = (string)data[2];

                Debug.LogWarning(tileName + "   " + ownerName + "   " + typeOfMine);
                GameObject tileGO = GameObject.Find(tileName).transform.GetChild(0).gameObject;

                if (ownerName == this.name)
                {
                    Debug.Log("trigger");
                    BombTrigger(tileGO.GetComponent<CellData>());
                }
                //tileGO.GetComponent<CellData>().ResetTile();       
                break;

            case 10:
                data = (object[])photonEvent.CustomData;
                string cardName = (string)data[0];

                foreach (Carte card in gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand)
                {
                    if (card.cardName == cardName)
                    {
                        card.CanTurnCard = true;
                    }
                }
                break;

            case 11:
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[0];
                m_MyActionPhase = m_Action.End;
                m_Canvas.UpdateInterface(m_MyActionPhase, hand);
                m_Canvas.ShowWinner(playerName);
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

    #region Physics
    private void physicsCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDistance, groundMask);
        if (!isGrounded && velocity.x < 0)
        {
            velocity.x = -1.5f;
        }

        RaycastHit hit;
        Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, interactDistance, groundMask);

        Color color = hit.collider != null ? Color.green : Color.red;

        Debug.DrawRay(cam.transform.position, cam.transform.forward * interactDistance, color);

        if(m_MyActionPhase == m_Action.Mouvement)
        {
            if (hit.collider != null && hit.collider.gameObject.GetComponent<CellData>().State == CellData.m_State.Show)
            {
                    if (tile != null && tile != hit.collider.gameObject)
                    {
                        tile.GetComponent<CellData>().UnHighlightTile(this.name);
                    }
                    tile = hit.collider.gameObject;
                    tile.GetComponent<CellData>().HighlightTile(this.name);
            }
            else if(hit.collider != null)
            {
                if (tile != null)
                    tile.GetComponent<CellData>().UnHighlightTile(this.name);
                tile = null;
            }
        }
        else if(m_MyActionPhase == m_Action.Action && m_MyBomb != Bomb.Nothing)
        {
            if (hit.collider != null && hit.collider.gameObject.GetComponent<CellData>().State == CellData.m_State.Show && m_Neighbours.Contains(hit.collider.gameObject))
            {

                    if (tile != null && tile != hit.collider.gameObject)
                    {
                        tile.GetComponent<CellData>().HighlightTile(this.name);
                    }
                    tile = hit.collider.gameObject;
                    tile.GetComponent<CellData>().UnHighlightTile(this.name);
                
            }
            else if (hit.collider != null)
            {
                if (tile != null)
                    tile.GetComponent<CellData>().HighlightTile(this.name);
                tile = null;
            }
        }
        
    }

    #endregion

    #region GETTER && SETTER
    public TMP_Text InfoText { get => infoText; set => infoText = value; }
    public NetworkUi Canva { get => m_Canvas; set => m_Canvas = value; }
    public int NbCardToDraw { get => nbCardToDraw; set => nbCardToDraw = value; }
    public PhotonView View { get => view; set => view = value; }
    public Grid Grid { get => grid; set => grid = value; }
    public GridGen Terrain { get => terrain; set => terrain = value; }

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
            //Gizmos.DrawSphere(new Vector3(grid.LocalToCell(transform.position).x + offset.x, grid.LocalToCell(transform.position).y + offset.y), 1.2f);
            Gizmos.DrawRay(transform.GetChild(0).transform.position, Vector3.forward * 1.2f);
        }
    }
    #endregion
}

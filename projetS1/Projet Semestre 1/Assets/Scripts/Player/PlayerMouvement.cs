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
    [SerializeField] GameObject redMineGO;
    [SerializeField] int amountOfRedMines = 2;

    [SerializeField] GameObject whiteMineGO;
    [SerializeField] int amountOfWhiteMines = 3;

    [SerializeField] GameObject blackMineGO;
    [SerializeField] int amountOfBlackMines = 3;

    [Tooltip("Indique la catégorie de la bombe que le joueur utilise")]
    [SerializeField] private Bomb m_MyBomb = Bomb.Nothing;

    [Space]
    [Header("Card")]
    public bool canDraw = false;
    public List<Carte> hand;
    public GameObject handGO;
    [SerializeField] private GameObject gameDeck;

    [Space]
    [Header("Lantern")]
    [SerializeField] private GameObject lantern;
    [SerializeField] private GameObject lanternGO;
    private List<GameObject> terrainLantern = new List<GameObject>();
    private GameObject myLantern;
    private List<Vector3> lanterns = new List<Vector3>();

    [Space]
    [Header("Canvas")]
    public float tempsAffichageMessage = 2f;
    private TMP_Text infoText;
    private NetworkUi m_Canvas;

    [Space]
    [Header("Other")]
    [SerializeField] private float TimeCardAvailable = 50.0f;
    [SerializeField] private bool turnCard = false;

    [Space]
    [Header("Color")]
    [SerializeField] private List<Color> playerColor;
    [SerializeField] private Color myColor;

    [Space]
    [Header ("Player Sound")]
    [FMODUnity.EventRef] public string footsteps = "";
    FMOD.Studio.EventInstance footstepsInstance;
    [FMODUnity.EventRef] public string Shovel_empty = "";
    FMOD.Studio.EventInstance Shovel_emptyInstance;
    [FMODUnity.EventRef] public string Shovel_treasure = "";
    FMOD.Studio.EventInstance Shovel_treasureInstance;
    [FMODUnity.EventRef] public string lantern_sound = "";
    FMOD.Studio.EventInstance lantern_soundInstance;

    [Space]
    [Header("Mine Sound")]
    [FMODUnity.EventRef] public string Music_drop_mine = "";
    FMOD.Studio.EventInstance Music_drop_mineInstance;
    [FMODUnity.EventRef] public string Music_mine_spy = "";
    FMOD.Studio.EventInstance Music_mine_spyInstance;
    [FMODUnity.EventRef] public string Music_mine_catch = "";
    FMOD.Studio.EventInstance Music_mine_catchInstance;
    [FMODUnity.EventRef] public string Music_mine_lock = "";
    FMOD.Studio.EventInstance Music_mine_lockInstance;

    [Space]
    [Header("Sound")]
    [FMODUnity.EventRef] public string reveal = "";
    FMOD.Studio.EventInstance revealInstance;
    
    //public FMODUnity.StudioEventEmitter myEmit;//recupère



    public enum m_Action { Mouvement, Action, Wait, Feedback, End }
    public enum Bomb { RED, BLACK, WHITE, Nothing }

    #endregion

    #region Awake || Start

    // Start is called before the first frame update
    void Start()
    {
        

        characterController = GetComponent<CharacterController>();
        view = GetComponent<PhotonView>();
        m_Canvas = GameObject.Find("Launcher").GetComponent<NetworkUi>();

        for (int i = 0; i < GameObject.FindObjectsOfType<changeColor>().Length; i++)
        {
            GameObject.FindObjectsOfType<changeColor>()[i].ChangeColor(playerColor[i]);
        }

        if (view.IsMine)
        {
            NetworkPlayer.LocalPlayerInstance = this.gameObject;
            myColor = playerColor[1];//transform.GetChild(3).GetChild(0).gameObject.GetComponent<changeColor>().myColor;
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
        if (view.IsMine)
            physicsCheck();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (view.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                Canva.gamePanel.SetActive(!Canva.gamePanel.active);
            }

            if (Input.GetKeyDown(KeyCode.F12) && transform.GetChild(3).GetChild(0).gameObject.active)
            {
                SendActionDone();
            }
            else if (Input.GetKeyDown(KeyCode.F12) && !transform.GetChild(3).GetChild(0).gameObject.active)
            {
                SendMouvementDone();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                Vector3 chestPos = terrain.GetChest().gameObject.transform.position;
                GameObject chest = Instantiate(lanternGO, chestPos, Quaternion.identity);
                chest.GetComponent<changeColor>().ChangeColor(Color.yellow);
                chest.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                terrain.ShowAllCell();
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                Canva.showPos();
            }

            if (Input.GetKeyDown(KeyCode.Space) && m_MyActionPhase == m_Action.End)
            {
                Canva.Display();
                Canva.DestroyAllCardsDisplay();
                if (myLantern == null)
                {
                    m_MyActionPhase = m_Action.Mouvement;
                    Canva.UpdatePhaseFeedBack(m_MyActionPhase);
                    //SendMouvementDone();
                }
                else
                {
                    m_MyActionPhase = m_Action.Wait;
                    Canva.UpdatePhaseFeedBack(m_MyActionPhase);
                }

            }

            if (Input.GetKeyDown(KeyCode.Space) && m_MyActionPhase == m_Action.Feedback)
            {
                SendFeedbackDone();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                Canva.ShowCards(hand);
            }

            if (PlayerID == 0 && gameDeck.GetComponent<GestionCartes>().ready && hand.Count == 0)
            {
                Debug.Log("Les cartes sont générer" + gameDeck.GetComponent<GestionCartes>().ready);
                MakeHand();
            }

            if (isGrounded && view.IsMine && m_MyActionPhase != m_Action.End)
            {
                //interactTile = tile;
                FpsMove();
                if (tile != null && (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Fire1")) && m_MyActionPhase == m_Action.Mouvement)
                {

                    UseFov();
                    highlightTiles();
                    SendMouvementDone();
                }

                if (m_MyActionPhase == m_Action.Action)
                {
                    SetMyMine();
                }

                if (tile != null && Input.GetButtonDown("Fire2") && m_MyActionPhase == m_Action.Action)
                {
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
        m_Neighbours.Clear();

        Vector3 pos = new Vector3(grid.LocalToCell(tile.transform.position).x + offset.x, 0, grid.LocalToCell(tile.transform.position).y + offset.y);
        myLantern = Instantiate(lantern, new Vector3(pos.x, pos.y + 1.1f, pos.z), Quaternion.identity);
        myLantern.GetComponent<changeColor>().ChangeColor(myColor);
        terrainLantern.Add(myLantern);
        FMODUnity.RuntimeManager.PlayOneShot(lantern_sound, transform.position); 

        transform.GetChild(3).GetChild(0).gameObject.SetActive(false);

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
        FMODUnity.RuntimeManager.PlayOneShot(reveal, transform.position);

    }

    void SetMyMine()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && amountOfRedMines > 0)
        {
            Canva.Sound();
            if (m_MyBomb == Bomb.RED)
            {
                Canva.SetActionUI(m_MyBomb, Bomb.Nothing);
                m_MyBomb = Bomb.Nothing;
            }
            else
            {
                Canva.SetActionUI(m_MyBomb, Bomb.RED);
                m_MyBomb = Bomb.RED;

            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && amountOfBlackMines > 0)
        {
            Canva.Sound();
            if (m_MyBomb == Bomb.BLACK)
            {
                Canva.SetActionUI(m_MyBomb, Bomb.Nothing);
                m_MyBomb = Bomb.Nothing;
            }
            else
            {
                Canva.SetActionUI(m_MyBomb, Bomb.BLACK);
                m_MyBomb = Bomb.BLACK;

            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && amountOfWhiteMines > 0)
        {
            Canva.Sound();
            if (m_MyBomb == Bomb.WHITE)
            {
                Canva.SetActionUI(m_MyBomb, Bomb.Nothing);
                m_MyBomb = Bomb.Nothing;
            }
            else
            {
                Canva.SetActionUI(m_MyBomb, Bomb.WHITE);
                m_MyBomb = Bomb.WHITE;

            }
        }
    }
    void bombImact(CellData item)
    {
        List<Mine> affect = new List<Mine>();
        foreach (Mine mine in item.Listbomb)
        {
            if (mine.BombOwner != gameObject.name)
            {
                affect.Add(mine);
            }
        }
        if (affect.Count > 0)
        {
            foreach (Mine elt in affect)
            {
                FMODUnity.RuntimeManager.PlayOneShot(Music_mine_spy, transform.position);

                MineFeedBack(elt.BombState);
                elt.SetBurst(item);
                SendBombTrigger(elt.BombState, item.gameObject.transform.parent.name, elt.BombOwner);
                item.ResetTile(elt);
            }
        }
    }

    void MineFeedBack(Mine.m_BombState elt)
    {
        switch (elt)
        {
            case Mine.m_BombState.RED:
                GameObject.FindObjectOfType<Feedback>().quiAffiche = Feedback.Receiver.give;
                GameObject.FindObjectOfType<Feedback>().mineAffiche = Feedback.MineType.spy;
                break;
            case Mine.m_BombState.BLACK:
                GameObject.FindObjectOfType<Feedback>().quiAffiche = Feedback.Receiver.give;
                GameObject.FindObjectOfType<Feedback>().mineAffiche = Feedback.MineType.draw;
                break;
            case Mine.m_BombState.WHITE:
                GameObject.FindObjectOfType<Feedback>().quiAffiche = Feedback.Receiver.give;
                GameObject.FindObjectOfType<Feedback>().mineAffiche = Feedback.MineType.reveal;
                break;
            case Mine.m_BombState.Nothing:
                break;
            default:
                break;
        }

        GameObject.FindObjectOfType<Feedback>().SortUi();
    }

    void BombTrigger(CellData bomb)
    {
        FMODUnity.RuntimeManager.PlayOneShot(Music_mine_spy, transform.position);

        List<Mine> bombtoReset = new List<Mine>();
        foreach (var item in bomb.Listbomb)
        {
            if (item.BombOwner == gameObject.name)
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
        m_MyActionPhase = m_Action.End;
        GameObject.FindObjectOfType<Feedback>().quiAffiche = Feedback.Receiver.receive;
        GameObject.FindObjectOfType<Feedback>().mineAffiche = Feedback.MineType.spy;
        GameObject.FindObjectOfType<Feedback>().SortUi();
        yield return new WaitForSeconds(TimeCardAvailable);
        Canva.showAllCard(gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand);
        Canva.UpdatePhaseFeedBack(m_MyActionPhase);
    }

    void BLACKtrigger()
    {
        GameObject.FindObjectOfType<Feedback>().quiAffiche = Feedback.Receiver.receive;
        GameObject.FindObjectOfType<Feedback>().mineAffiche = Feedback.MineType.draw;
        GameObject.FindObjectOfType<Feedback>().SortUi();
        DrawCard();
    }

    void WHITEtrigger()
    {
        GameObject.FindObjectOfType<Feedback>().quiAffiche = Feedback.Receiver.receive;
        GameObject.FindObjectOfType<Feedback>().mineAffiche = Feedback.MineType.reveal;
        GameObject.FindObjectOfType<Feedback>().SortUi();

        foreach (Carte item in hand)
        {
            if (item.State == Carte.CardState.LOCK)
            {
                item.State = Carte.CardState.UNLOCK;
                SendUnlockCard(item.cardName);
                Canva.ShowCards(hand);
                return;
            }
        }
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Start");
        m_Canvas.StartGameUI();
        GestionCartes deck = gameDeck.GetComponent<GestionCartes>();
        terrain = GameObject.FindObjectOfType<GridGen>();

        m_MyActionPhase = m_Action.Mouvement;
        Canva.UpdatePhaseFeedBack(m_MyActionPhase);
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
    }

    void PlayerAction()
    {
        if (interactTile != null)
        {
            if (interactTile.GetComponent<CellData>().CanPlantBomb && m_MyBomb != Bomb.Nothing)
            {
                foreach (var item in m_Neighbours)
                {
                    //item.GetComponent<CellData>().ShowTile(gameObject.name);
                    item.GetComponent<CellData>().CanPlantBomb = false;
                }

                GameObject mine = SpawnBomB(interactTile.transform.position);
                interactTile.GetComponent<CellData>().PlantBomb(m_MyBomb, gameObject.name, mine);

                FMODUnity.RuntimeManager.PlayOneShot(Music_drop_mine, transform.position);

                SendDropMine(m_MyBomb, interactTile.gameObject.transform.parent.name);

                Canva.SetActionUI(m_MyBomb, Bomb.Nothing);

                switch (m_MyBomb)
                {
                    case Bomb.RED:
                        Canva.UpdateNbUsemine(m_MyBomb, amountOfRedMines);
                        amountOfRedMines--;
                        break;

                    case Bomb.BLACK:
                        Canva.UpdateNbUsemine(m_MyBomb, amountOfBlackMines);
                        amountOfBlackMines--;
                        break;

                    case Bomb.WHITE:
                        Canva.UpdateNbUsemine(m_MyBomb, amountOfWhiteMines);
                        amountOfWhiteMines--;
                        break;

                    default:
                        break;
                }


                m_MyBomb = Bomb.Nothing;
                SendActionDone();
            }
            else if (m_Neighbours.Contains(interactTile) && m_MyBomb == Bomb.Nothing && interactTile.GetComponent<CellData>().CanPlantBomb)
            {
                interactTile.GetComponent<CellData>().Dig();
                Debug.Log("il n'y a pas d'erreru");
                if (interactTile.GetComponent<CellData>().isTreasure && interactTile.GetComponent<CellData>() != null)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(Shovel_treasure, transform.position);
                    SendPlayerGetTreasure(interactTile.transform.parent.gameObject);
                    m_MyActionPhase = m_Action.End;
                }
                else
                {
                    //FMODUnity.RuntimeManager.PlayOneShot(Shovel_empty, transform.position);
                    SendActionDone();
                }
            }
        }
    }

    GameObject SpawnBomB(Vector3 tilePos)
    {
        switch (m_MyBomb)
        {
            case Bomb.RED:
                Debug.Log("Rouge");
                return Instantiate(redMineGO, new Vector3(interactTile.transform.position.x, interactTile.transform.position.y + 0.05f, interactTile.transform.position.z), Quaternion.Euler(-90f, 0f, 0f));
                break;
            case Bomb.BLACK:
                Debug.Log("Noir");
                return Instantiate(blackMineGO, new Vector3(interactTile.transform.position.x, interactTile.transform.position.y + 0.05f, interactTile.transform.position.z), Quaternion.Euler(-90f, 0f, 0f));
                break;
            case Bomb.WHITE:
                Debug.Log("Blanche");
                return Instantiate(whiteMineGO, new Vector3(interactTile.transform.position.x, interactTile.transform.position.y + 0.05f, interactTile.transform.position.z), Quaternion.Euler(-90f, 0f, 0f));
                break;
            case Bomb.Nothing:
                break;
            default:
                break;
        }
        return null;
    }

    void ChangePhase(string playerName, int playerPhase, m_Action actionPhase)
    {
        switch (playerPhase)
        {
            case 0:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.Mouvement;
                Debug.Log("Player is Mouvement");
                break;

            case 1:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.Action;
                Debug.Log("Player is Action");
                break;

            case 2:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.Wait;
                Debug.Log("Player is wait");
                break;

            case 3:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.Feedback;
                Debug.Log("Player is Feedback");
                break;

            case 4:
                GameObject.Find(playerName).GetComponent<PlayerMouvement>().m_MyActionPhase = m_Action.End;
                Debug.Log("Player is End");
                break;
            default:
                break;
        }

        switch (actionPhase)
        {
            case m_Action.Mouvement:
                ChangeTurn();
                break;
            case m_Action.Action:
                ActionPhase();
                break;
            case m_Action.Wait:
                break;
            case m_Action.Feedback:
                FeedbackPhase();
                break;
            case m_Action.End:
                break;
            default:
                break;
        }
        /*if (actionPhase)
            ActionPhase();
        else
            ChangeTurn();*/
    }

    void ActionPhase()
    {
        Debug.Log("action");
        PlayerMouvement[] players = GameObject.FindObjectsOfType<PlayerMouvement>();
        foreach (PlayerMouvement player in players)
        {
            if (player.m_MyActionPhase != m_Action.Wait)
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

    void FeedbackPhase()
    {
        Debug.Log("FeedBack");
        PlayerMouvement[] players = GameObject.FindObjectsOfType<PlayerMouvement>();
        foreach (PlayerMouvement player in players)
        {
            if (player.m_MyActionPhase != m_Action.Wait)
            {
                return;
            }
        }
        if (view.IsMine)
        {
            Debug.Log("action");
            SendFeedbackPhase();
        }
    }

    void ChangeTurn()
    {
        PlayerMouvement[] players = GameObject.FindObjectsOfType<PlayerMouvement>();
        foreach (PlayerMouvement player in players)
        {
            if (player.m_MyActionPhase != m_Action.Wait)
            {
                Debug.Log("pas tous le monde a joué");
                return;
            }
        }
        if (view.IsMine)
        {
            Debug.Log("Mouvement");
            SendChangeTurn();
        }
            
        
    }

    public void MakeHand()
    {
        for (int y = 0; y < nbCardToDraw; y++)
        {
            if (gameDeck.GetComponent<GestionCartes>().allCards.Count > 0)
                DrawCard();
        }
        if (hand.Count == 2)
        {
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
        //cardDraw.Create();
        SendPlayerhasDraw(cardDraw);
        Canva.DisplayACard(null, cardDraw);


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
    }

    void ChangeCard(Carte cardToDrop)
    {

        int newCardIndex = Random.Range(0, gameDeck.GetComponent<GestionCartes>().allCards.Count);
        Carte drop = cardToDrop;

        gameDeck.GetComponent<GestionCartes>().allCards.Add(drop);
        gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand.Remove(drop);
        hand.Remove(drop);
        Destroy(drop.ingameDisplay);

        Carte get = DrawCard();

        SendDeckHasChange(drop, get);
    }

    void ChangeMyCard(RaycastHit hit)
    {
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
        if (amountOfRedMines > 0)
        {
            highlightTiles();
            m_MyBomb = Bomb.RED;
            amountOfRedMines--;
        }

    }

    public void PlantBombWhite()
    {
        if (amountOfWhiteMines > 0)
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
            if (carte.cardId == carteToDeckId)
            {
                toRemove = carte;
                gameDeck.GetComponent<GestionCartes>().allCards.Add(carte);
                gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand.Remove(toRemove);
                break;
            }
        }

        foreach (Carte carte in gameDeck.GetComponent<GestionCartes>().allCards)
        {
            if (carte.cardId == carteToHandId)
            {
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
        if (players.Length > 1)
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
        Canva.UpdatePhaseFeedBack(m_MyActionPhase);

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
        Canva.UpdatePhaseFeedBack(m_MyActionPhase);
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);

        byte evCode = 4; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { gameObject.name, 2, myLantern.transform.position }; // Array contains the target position and the IDs of the selected units

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
        object[] content = new object[] { name }; // Array contains the target position and the IDs of the selected units
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
        transform.GetChild(3).GetChild(0).gameObject.SetActive(true);
        foreach (var item in terrainLantern)
        {
            Destroy(item);
        }
        terrainLantern.Clear();

        m_MyActionPhase = m_Action.Wait;
        Debug.LogWarning("SendActionDone");
        Canva.UpdatePhaseFeedBack(m_MyActionPhase);
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
        byte evCode = 6; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { transform.GetChild(0).transform.position, FOV, "Je sais pas encore", name, PlayerID }; // Array contains the target position and the IDs of the selected units

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
        Debug.LogWarning("SendChangeTurn");

        m_MyActionPhase = m_Action.Wait;
        Canva.UpdatePhaseFeedBack(m_MyActionPhase);

        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
        byte evCode = 8; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { }; // Array contains the target position and the IDs of the selected units

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
    void SendPlayerGetTreasure(GameObject tile)
    {
        Debug.LogWarning("SendPlayerGetTreasure");
        Debug.LogError(tile.name);
        m_MyActionPhase = m_Action.End;
        Canva.UpdatePhaseFeedBack(m_MyActionPhase);
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
        m_Canvas.ShowWinner(this.name);

        byte evCode = 11; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { this.gameObject.name, tile.name }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    void SendFeedbackPhase()
    {
        Debug.LogWarning("SendFeedbackPhase");
        byte evCode = 12; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { name }; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.All;
        SendOptions sendOptions = new SendOptions();
        sendOptions.DeliveryMode = DeliveryMode.Reliable;
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    void SendFeedbackDone()
    {
        m_MyActionPhase = m_Action.Wait;
        Debug.LogWarning("SendFeedbackDone");
        Canva.UpdatePhaseFeedBack(m_MyActionPhase);
        m_Canvas.UpdateInterface(m_MyActionPhase, hand);
        byte evCode = 13; // Custom Event 1: Used as "MoveUnitsToTargetPosition" event
        object[] content = new object[] { transform.GetChild(0).transform.position, FOV, "Je sais pas encore", name, PlayerID }; // Array contains the target position and the IDs of the selected units

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
        raiseEventOptions.Receivers = ReceiverGroup.Others;
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
    void UpdateBoard()
    {
        foreach (Vector3 lanterne in lanterns)
        {
            foreach (Collider item in Physics.OverlapSphere(lanterne, FOV))
            {
                if (item.gameObject.GetComponent<CellData>() != null && m_Neighbours.Count > 0)
                {
                    if (m_Neighbours.Contains(item.gameObject))
                    {
                        Debug.Log("il est partagé par les deux joueurs");
                    }
                    else
                    {
                        item.gameObject.GetComponent<CellData>().HideTile(gameObject.name);
                    }
                }
                else
                {
                    Debug.Log("il n'a pas de celldata");
                }
            }
        }
        lanterns.Clear();
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

        StartCoroutine(m_Canvas.ShowInformation(result, false));
        if (view.IsMine && gameObject.name.Contains(view.Owner.NickName))
        {
            m_MyActionPhase = m_Action.Wait;
            Canva.UpdatePhaseFeedBack(m_MyActionPhase);
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
        StartCoroutine(m_Canvas.ShowInformation(result, true));

        if (view.IsMine)
        {
            if (!result)
            {
                m_MyActionPhase = m_Action.Wait;
                Canva.UpdatePhaseFeedBack(m_MyActionPhase);
                SendActionDone();
            }
            //else
            //SendPlayerGetTreasure();
        }
    }


    /// <summary>
    /// This method is call when a player don't want to make an action during the action phase
    /// </summary>
    public void PassTurn()
    {
        if (m_MyBomb != Bomb.Nothing)
        {
            foreach (var item in m_Neighbours)
            {
                item.GetComponent<CellData>().ShowTile(gameObject.name);
                item.GetComponent<CellData>().CanPlantBomb = false;
            }
        }

        if (view.IsMine && gameObject.name.Contains(view.Owner.NickName))
        {
            m_MyActionPhase = m_Action.Wait;
            Canva.UpdatePhaseFeedBack(m_MyActionPhase);
            SendActionDone();
        }
    }

    /// <summary>
    /// Start the game when the master client decide
    /// </summary>
    public void GameStart()
    {

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
                Debug.Log("SetID");
                break;

            case 1:
                data = (object[])photonEvent.CustomData;
                Debug.Log("start");
                LaunchGame();
                break;

            case 2:
                data = (object[])photonEvent.CustomData;
                playerID = (int)data[0];
                string handID = (string)data[1];
                int nbCardInHand = (int)data[2];
                Debug.Log("draw");
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
                Debug.Log("DeckChange");
                UpdateDeck(carteToDeckId, carteToHandId);
                break;

            case 4:
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[0];
                int phase = (int)data[1];
                Vector3 pos = (Vector3)data[2];
                Debug.LogWarning("il a fait le move");
                if (playerName != this.name)
                {
                    GameObject lanterne = Instantiate(lantern, pos, Quaternion.identity);
                    lanterne.GetComponent<changeColor>().ChangeColor(playerColor[0]);
                    terrainLantern.Add(lanterne);
                    lanterns.Add(lanterne.transform.position);
                    lanterne.SetActive(false);
                }

                ChangePhase(playerName, phase, m_Action.Feedback);
                break;

            case 5:
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[0];
                Debug.Log("reçu 5 de la part de +" + playerName);
                Debug.Log("ActionPhase");
                m_MyActionPhase = m_Action.Action;
                Canva.UpdatePhaseFeedBack(m_MyActionPhase);
                m_Canvas.UpdateInterface(m_MyActionPhase, hand);

                break;

            case 6:
                //ChangeTurn();
                Debug.Log("ActionDone");
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[3];
                ChangePhase(playerName, 2, m_Action.Mouvement);

                break;

            case 7:
                data = (object[])photonEvent.CustomData;
                int typeofMine = (int)data[0];
                string tileName = (string)data[1];
                string ownerName = (string)data[2];
                Debug.Log("DropMine");
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
                //ChangeTurn();
                break;

            case 8:
                Debug.Log("ChangeTurn");
                data = (object[])photonEvent.CustomData;
                m_MyActionPhase = m_Action.Mouvement;
                Canva.UpdatePhaseFeedBack(m_MyActionPhase);
                break;

            case 9:
                Debug.Log("Bomb Trigger");
                data = (object[])photonEvent.CustomData;
                int typeOfMine = (int)data[0];
                tileName = (string)data[1];
                ownerName = (string)data[2];
                GameObject tileGO = GameObject.Find(tileName).transform.GetChild(0).gameObject;

                if (ownerName == this.name)
                {
                    Debug.Log("trigger");
                    BombTrigger(tileGO.GetComponent<CellData>());
                }
                //tileGO.GetComponent<CellData>().ResetTile();       
                break;

            case 10:
                Debug.Log("Unlock");
                data = (object[])photonEvent.CustomData;
                string cardName = (string)data[0];

                foreach (Carte card in gameDeck.GetComponent<GestionCartes>().allCardsInPlayerHand)
                {
                    if (card.cardName == cardName)
                    {
                        card.State = Carte.CardState.UNLOCK;
                    }
                }
                break;

            case 11:
                Debug.Log("chest");
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[0];
                string chestName = (string)data[1];
                if (this.name != playerName)
                    GameObject.Find(chestName).transform.GetChild(0).GetComponent<CellData>().Dig();
                m_MyActionPhase = m_Action.End;
                Canva.UpdatePhaseFeedBack(m_MyActionPhase);
                m_Canvas.UpdateInterface(m_MyActionPhase, hand);
                m_Canvas.ShowWinner(playerName);
                break;

            case 12:
                Debug.Log("FeedBack");
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[0];
                Debug.Log("reçu 12 de la part de +" + playerName);
                m_MyActionPhase = m_Action.Feedback;
                Canva.UpdatePhaseFeedBack(m_MyActionPhase);
                m_Canvas.UpdateInterface(m_MyActionPhase, hand);
                foreach (var item in terrainLantern)
                {
                    item.SetActive(true);
                }

                UpdateBoard();
                break;

            case 13:
                Debug.Log("FeedBack Done");
                data = (object[])photonEvent.CustomData;
                playerName = (string)data[3];
                ChangePhase(playerName, 2, m_Action.Action);
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

        if (hit.collider != null && hit.collider.gameObject.GetComponent<CellData>().State == CellData.m_State.Show)
        {
            interactTile = hit.collider.gameObject;
            Canva.DiplayPos(interactTile.transform.parent.gameObject.name);
            Canva.CanInteract();
        }
        else
        {
            interactTile = null;
            Canva.DiplayPos("");
            Canva.CantInteract();
        }

        /*if (hit.collider != null && hit.collider.gameObject.GetComponent<CellData>().State == CellData.m_State.Show)
        {
            interactTile = hit.collider.gameObject;
            foreach (var item in GameObject.FindObjectsOfType<PlayerMouvement>())
            {
                if(item.interactTile != null)
                    Canva.CanInteract();
            }
            
        }
        else
        {
            int index = 0;
            interactTile = null;
            foreach (var item in GameObject.FindObjectsOfType<PlayerMouvement>())
            {
                if (item.interactTile == null)
                    index++;
            }
            if(index == GameObject.FindObjectsOfType<PlayerMouvement>().Length)
                Canva.CantInteract();
        }*/

        if (m_MyActionPhase == m_Action.Mouvement)
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
            else if (hit.collider != null)
            {
                if (tile != null)
                    tile.GetComponent<CellData>().UnHighlightTile(this.name);
                tile = null;
            }
        }
        else if (m_MyActionPhase == m_Action.Action && m_MyBomb != Bomb.Nothing)
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
    public GameObject LanternGO { get => lanternGO; set => lanternGO = value; }

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
            //Gizmos.DrawRay(transform.GetChild(0).transform.position, Vector3.forward * 1.2f);
        }
    }
    #endregion
}

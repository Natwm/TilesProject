using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class NetworkUi : MonoBehaviourPunCallbacks
{
    [Header("Affichage message")]
    [SerializeField]float m_TempsAffichageMessage = 3f;

    [Header("Launch Canvas")]
    public GameObject startPanel;
    public TMP_Text roomInfos;
    public TMP_InputField nicknameField;
    public TMP_InputField seedField;
    public Button connectButton;
    public TMP_InputField sizeX;
    public TMP_InputField sizeY;

    [Space]
    [Header("Waiting Canvas")]
    public GameObject waitingPanel;
    public Button LaunchButton;
    public Image BlackSceen;

    [Space]
    [Header("Game Canvas")]
    public GameObject gamePanel;
    public TMP_Text turnInfos;
    public TMP_Text textInfos;
    public TMP_Text handInfos;
    public Button nearButton;
    public Button chestButton;
    public Button passButton;
    public Button bombButton1;
    public Button bombButton2;
    public Button bombButton3;




    [Space]
    [Header("Game Canvas V2")]
    public Image Action_Lantern;
    public Image Action_Dig;

    public Image pointeur;

    [Space]

    public Image Mine_Spy;
    public GameObject nbUse_Mine_Spy;
    public TMP_Text nb_Mine_Spy;

    [Space]

    public Image Mine_NewCard;
    public GameObject nbUse_Mine_NewCard;
    public TMP_Text nb_Mine_NewCard;

    [Space]

    public Image Mine_Reveal;
    public GameObject nbUse_Mine_Reveal;
    public TMP_Text nb_Mine_Reveal;

    [Space]

    public Image Action_Cards;
    public float rescale = 1.25f;



    [Space]

    public GameObject cardHolder;
    public GameObject allCardHolder;
    public GameObject cardDisplay;
    public GameObject allCardDisplay;
    public GameObject phaseDisplay;

    [Space]
    [Header (" Images ")]
    public Sprite lockImage;
    public Sprite movePhaseImage;
    public Sprite actionPhaseImage;
    public Sprite waitPhaseImage;
    public Sprite endPhaseImage;
    public Sprite feedbackPhaseImage;


    [Space]
    [Header(" Color ")]
    public Color colorMineSelect;
    public Color pointeurValid;
    public Color pointeurInValid;

    [Space]
    [Header("UI Sound")]
    [FMODUnity.EventRef] public string UI_selection = ""; 
    FMOD.Studio.EventInstance UI_selectionInstance;


    Launcher networkManager;
    public bool isOn = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        networkManager = FindObjectOfType<Launcher>();
    }

    private void Start()
    {
        nicknameField.text = networkManager.nickname;
        nicknameField.ActivateInputField();
    }

    // Update is called once per frame
    void Update()
    {
        roomInfos.text = "";
        UpdateRoomUI();
    }

    #region UI Simon

    public void SetActionUI(PlayerMouvement.Bomb currentTypeofMine, PlayerMouvement.Bomb newTypeofMine)
    {
        switch (currentTypeofMine)
        {
            case PlayerMouvement.Bomb.RED:
                Mine_Spy.transform.localScale = new Vector3(Mine_Spy.transform.localScale.x / rescale, Mine_Spy.transform.localScale.y / rescale, Mine_Spy.transform.localScale.z);
                Mine_Spy.color = Color.white;
                break;

            case PlayerMouvement.Bomb.BLACK:
                Mine_NewCard.transform.localScale = new Vector3(Mine_NewCard.transform.localScale.x / rescale, Mine_NewCard.transform.localScale.y / rescale, Mine_NewCard.transform.localScale.z);
                Mine_NewCard.color = Color.white;
                break;

            case PlayerMouvement.Bomb.WHITE:
                Mine_Reveal.transform.localScale = new Vector3(Mine_Reveal.transform.localScale.x / rescale, Mine_Reveal.transform.localScale.y / rescale, Mine_Reveal.transform.localScale.z);
                Mine_Reveal.color = Color.white;
                break;

            case PlayerMouvement.Bomb.Nothing:
                break;
            default:
                break;
        }

        switch (newTypeofMine)
        {
            case PlayerMouvement.Bomb.RED:
                Mine_Spy.transform.localScale = new Vector3(Mine_Spy.transform.localScale.x * rescale, Mine_Spy.transform.localScale.y * rescale, Mine_Spy.transform.localScale.z);
                Mine_Spy.color = colorMineSelect;
                break;
            case PlayerMouvement.Bomb.BLACK:
                Mine_NewCard.transform.localScale = new Vector3(Mine_NewCard.transform.localScale.x * rescale, Mine_NewCard.transform.localScale.y * rescale, Mine_NewCard.transform.localScale.z);
                Mine_NewCard.color = colorMineSelect;
                break;
            case PlayerMouvement.Bomb.WHITE:
                Mine_Reveal.transform.localScale = new Vector3(Mine_Reveal.transform.localScale.x * rescale, Mine_Reveal.transform.localScale.y * rescale, Mine_Reveal.transform.localScale.z);
                Mine_Reveal.color = colorMineSelect;
                break;
            case PlayerMouvement.Bomb.Nothing:
                break;
            default:
                break;
        }
    }

    public void UpdateNbUsemine(PlayerMouvement.Bomb currentTypeofMine, int nbUse)
    {
        switch (currentTypeofMine)
        {
            case PlayerMouvement.Bomb.RED:
                nbUse_Mine_Spy.transform.GetChild(nbUse - 1).gameObject.SetActive(false);
                break;

            case PlayerMouvement.Bomb.BLACK:
                nbUse_Mine_NewCard.transform.GetChild(nbUse - 1).gameObject.SetActive(false);
                break;

            case PlayerMouvement.Bomb.WHITE:
                nbUse_Mine_Reveal.transform.GetChild(nbUse - 1).gameObject.SetActive(false);
                break;

            case PlayerMouvement.Bomb.Nothing:
                break;
            default:
                break;
        }
    }

    public void DisplayCard(List<Carte> hand)
    {
        for (int i = 0; i < hand.Count; i++)
        {
            DisplayACard(cardHolder.transform.GetChild(i).gameObject, hand[i]);
        }
    }

    public void showMyCard(List<Carte> allCards)
    {
        DestroyCardsDisplay();
        for (int i = 0; i < allCards.Count; i++)
        {
            DisplayACard(null,allCards[i]);
        }
    }

    public void showAllCard(List<Carte> allCards)
    {
        if(isOn == false)
        {
            for (int i = 0; i < allCards.Count; i++)
            {
                DisplayCards(null, allCards[i]);
            }
            isOn = true;
        }
        
    }

    public void DestroyAllCardsDisplay()
    {
        if(isOn == true)
        {
            for (int i = 0; i < allCardHolder.transform.childCount; i++)
            {
                Destroy(allCardHolder.transform.GetChild(i).gameObject);
            }
            isOn = false;
        }
        
    }

    public void DestroyCardsDisplay()
    {
        for (int i = 0; i < cardHolder.transform.childCount; i++)
        {
            Destroy(cardHolder.transform.GetChild(i).gameObject);
        }
    }

    public void DisplayAllCard(Carte toPrint)
    {
        GameObject carte = Instantiate(allCardDisplay);
        carte.transform.parent = allCardHolder.transform;
        carte.transform.GetChild(0).GetComponent<Image>().sprite = toPrint.front;

        Debug.LogError(toPrint.State);

        if(toPrint.State == Carte.CardState.UNLOCK)
        {
            carte.transform.GetChild(1).GetComponent<Image>().sprite = toPrint.back;
        }
        else
        {
            carte.transform.GetChild(1).GetComponent<Image>().sprite = lockImage;
        }
       
        carte.transform.localScale = Vector3.one;
    }

    public void DisplayACard(GameObject card, Carte toPrint)
    {
        if(card == null)
        {
            GameObject carte = Instantiate(cardDisplay);
            carte.transform.parent = cardHolder.transform;
            carte.transform.GetChild(0).GetComponent<Image>().sprite = toPrint.front;
            if (toPrint.State == Carte.CardState.UNLOCK)
            {
                carte.transform.GetChild(1).GetComponent<Image>().sprite = toPrint.back;
            }
            else
            {
                carte.transform.GetChild(1).GetComponent<Image>().sprite = lockImage;
            }

            carte.transform.localScale = Vector3.one;
        }
        else
        {
            card.transform.GetChild(0).GetComponent<Image>().sprite = toPrint.front;
            if (toPrint.State == Carte.CardState.UNLOCK)
            {
                card.transform.GetChild(1).GetComponent<Image>().sprite = toPrint.back;
            }
            else
            {
                card.transform.GetChild(1).GetComponent<Image>().sprite = lockImage;
            }
  
        }
        
    }

    public void DisplayCards(GameObject card, Carte toPrint)
    {
        if (card == null)
        {
            GameObject carte = Instantiate(allCardDisplay);
            carte.transform.parent = allCardHolder.transform;
            carte.transform.GetChild(0).GetComponent<Image>().sprite = toPrint.front;
            if (toPrint.State == Carte.CardState.UNLOCK)
            {
                carte.transform.GetChild(1).GetComponent<Image>().sprite = toPrint.back;
            }
            else
            {
                carte.transform.GetChild(1).GetComponent<Image>().sprite = lockImage;
            }

            carte.transform.localScale = Vector3.one;
        }
        else
        {
            card.transform.GetChild(0).GetComponent<Image>().sprite = toPrint.front;
            if (toPrint.State == Carte.CardState.UNLOCK)
            {
                card.transform.GetChild(1).GetComponent<Image>().sprite = toPrint.back;
            }
            else
            {
                card.transform.GetChild(1).GetComponent<Image>().sprite = lockImage;
            }

        }

    }

    public void ShowCards(List<Carte>Hand)
    {
        cardHolder.SetActive(!cardHolder.active);
        if(cardHolder.active == true)
        {
            showMyCard(Hand);
        }

    }

    public void UpdatePhaseFeedBack(PlayerMouvement.m_Action phase)
    {
        Debug.Log("UpdatePhaseFeedBack");
        switch (phase)
        {
            case PlayerMouvement.m_Action.Mouvement:
                phaseDisplay.GetComponent<Image>().sprite = movePhaseImage;
                break;

            case PlayerMouvement.m_Action.Action:
                phaseDisplay.GetComponent<Image>().sprite = actionPhaseImage;
                break;

            case PlayerMouvement.m_Action.Wait:
                phaseDisplay.GetComponent<Image>().sprite = waitPhaseImage;
                break;

            case PlayerMouvement.m_Action.Feedback:
                phaseDisplay.GetComponent<Image>().sprite = feedbackPhaseImage;
                break;

            case PlayerMouvement.m_Action.End:
                phaseDisplay.GetComponent<Image>().sprite = endPhaseImage;
                break;

            default:
                break;
        }
    }

    public void CanInteract()
    {
        pointeur.color = pointeurValid;
    }

    public void CantInteract()
    {
        pointeur.color = pointeurInValid;
    }
    #endregion

    public void SetGameUI(PlayerMouvement player, bool isMaster)
    {
        Initializedbutton(player);
        LaunchButton.gameObject.SetActive(isMaster);
        //BlackSceen.gameObject.SetActive(true);
        waitingPanel.SetActive(true);
    }

    public void StartGameUI()
    {
        waitingPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    void Initializedbutton (PlayerMouvement player) {
        LaunchButton.onClick.AddListener(player.SendStart);

        nearButton.onClick.AddListener(player.CheckIsNearChest);

        chestButton.onClick.AddListener(player.CheckIsChest);

        passButton.onClick.AddListener(player.PassTurn);

        bombButton1.onClick.AddListener(player.PlantBombRed);

        bombButton2.onClick.AddListener(player.PlantBombBlack);

        bombButton3.onClick.AddListener(player.PlantBombWhite);
    }

    public bool UpdateInterface(PlayerMouvement.m_Action action, List<Carte> hand)
    {

        if (action != PlayerMouvement.m_Action.Action)
        {
            nearButton.gameObject.SetActive(false);
            chestButton.gameObject.SetActive(false);
            passButton.gameObject.SetActive(false);
            bombButton1.gameObject.SetActive(false);
            bombButton2.gameObject.SetActive(false);
            bombButton3.gameObject.SetActive(false);
        }
        else
        {
            nearButton.gameObject.SetActive(true);
            chestButton.gameObject.SetActive(true);
            passButton.gameObject.SetActive(true);
            bombButton1.gameObject.SetActive(true);
            bombButton2.gameObject.SetActive(true);
            bombButton3.gameObject.SetActive(true);
        }

        if (action == PlayerMouvement.m_Action.Mouvement)
            turnInfos.text = "C'est votre tour !";
        else if (action == PlayerMouvement.m_Action.Action)
            turnInfos.text = "Faites une action";
        else
            turnInfos.text = "Ce n'est pas votre tour";

        return true;
    }

    void UpdateRoomUI()
    {
        if (PhotonNetwork.InRoom)
        {

            string text = "<b>PLAYERS IN ROOM : </b>\n";

            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                text += "#" + player.Value.NickName + "\n";
            }
            roomInfos.text += text;
        }
    }
    
    public void ShowWinner( string playerName)
    {
        textInfos.text = playerName + " a le trésor";
    }

    public IEnumerator ShowInformation(bool isfind, bool isForChest)
    {
        Debug.LogWarning("message " + isfind + " second " + isForChest);
        if (isfind && isForChest)
        {
            Debug.Log("Vous avez trouvé(e) le trésor");
            textInfos.text = "Vous avez trouvé(e) le trésor";
        }
        else if (!isfind && isForChest)
        {
            Debug.Log("Vous n'avez pas trouvé(e) le trésor");
            textInfos.text = "Vous n'avez pas trouvé(e) le trésor";
        }
        else if (isfind && !isForChest)
        {
            Debug.Log("Le trésor est autour de vous !");
            textInfos.text = "Le trésor est autour de vous !";
        }
        else
        {
            Debug.Log("Le trésor n'est pas autour de vous !");
            textInfos.text = "Le trésor n'est pas autour de vous !";
        }

        yield return new WaitForSeconds(m_TempsAffichageMessage);
        textInfos.text = "";
    }

    public void OnNicknameUpdated(string newNickname)
    {
        Debug.LogFormat("nickname updated to {0}", newNickname);
        networkManager.SetPlayerName(newNickname);

        //// TO CHANGE FOR HANDLING OTHER PLAYERS
        //FindObjectOfType<PlayerCursor>().UpdateName(networkManager.nickname);
    }

    public void OnSeedUpdate(string newSeed)
    {
        Debug.LogFormat("seed updated to {0}", newSeed);
        networkManager.GameSeed = Int32.Parse(newSeed);
    }

    public void OnSizeUpdate(int sizeX, int sizeY)
    {
        Debug.LogFormat("size updated to {0} | {1}", sizeX, sizeY);
        networkManager.GameSize = new Vector2Int(sizeX, sizeY);
    }

    public void OnConnectButton()
    {
        FMODUnity.RuntimeManager.PlayOneShot(UI_selection, transform.position);
        OnNicknameUpdated(nicknameField.text);
        OnSeedUpdate(seedField.text);
        OnSizeUpdate(Int32.Parse(sizeX.text), Int32.Parse(sizeY.text));
        networkManager.Connect();
    }

    public override void OnConnected()
    {
        // hide connect button and input field
       /* connectButton.gameObject.SetActive(false);
        nicknameField.transform.parent.gameObject.SetActive(false);
        seedField.transform.parent.gameObject.SetActive(false);*/
        startPanel.SetActive(false);
    }

    public void Sound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(UI_selection, transform.position);
    }
}

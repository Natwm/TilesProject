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
    public GameObject cardHolder;

    Launcher networkManager;

    
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
        textInfos.text = playerName + " has find the treasure !";
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

    public void OnConnectButton()
    {
        OnNicknameUpdated(nicknameField.text);
        OnSeedUpdate(seedField.text);
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
}

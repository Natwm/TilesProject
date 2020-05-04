using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

public class NetworkUi : MonoBehaviourPunCallbacks
{
    public TMP_Text roomInfos;
    public TMP_InputField nicknameField;
    public TMP_InputField seedField;
    public Button connectButton;

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
        connectButton.gameObject.SetActive(false);
        nicknameField.transform.parent.gameObject.SetActive(false);
        seedField.transform.parent.gameObject.SetActive(false);
    }
}

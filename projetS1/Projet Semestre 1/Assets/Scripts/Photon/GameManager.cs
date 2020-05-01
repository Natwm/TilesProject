using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;


public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject canvasPrefab;

    PhotonView view;

    void Start()
    {
        if (NetworkPlayer.LocalPlayerInstance == null)
        {
            Debug.LogFormat("GAME MANAGER START");
            Vector3 pos = new Vector3(0, 0, -1);
            GameObject playerGO = PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity);
            //GameObject canvasGo = PhotonNetwork.Instantiate(canvasPrefab.name, Vector3.zero, Quaternion.identity);
            /*GameObject canvasGo = Instantiate(canvasPrefab, Vector3.zero, Quaternion.identity);

           // canvasGo.name += "_" + canvasGo.GetComponent<PhotonView>().Owner.NickName;

            Transform ButtonContainer = canvasGo.transform.GetChild(0).gameObject.transform;

            //Player creation

            NetworkPlayer.LocalPlayerInstance = playerGO;
            PlayerMouvement playerscript = playerGO.GetComponent<PlayerMouvement>();

            //Canvas button Creation
            Button canvaButtonCheckNear = ButtonContainer.GetChild(0).GetComponent<Button>();
            canvaButtonCheckNear.onClick.AddListener(playerscript.CheckIsNearChest);

            Button canvaButtonCheckChest = ButtonContainer.GetChild(1).GetComponent<Button>();
            canvaButtonCheckChest.onClick.AddListener(playerscript.CheckIsChest);

            Button canvaButtonCheckPass = ButtonContainer.GetChild(2).GetComponent<Button>();
            canvaButtonCheckPass.onClick.AddListener(playerscript.PassTurn);

            //Player Canvas
            TMP_Text infoText = canvasGo.transform.GetChild(1).GetComponent<TMP_Text>();
            playerscript.InfoText = infoText;
            playerscript.Canva = canvasGo;*/

            NetworkPlayer.LocalPlayerInstance = playerGO;

        }
    }
}

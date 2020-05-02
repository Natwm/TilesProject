using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject gridPrefab;

    private bool isSent;

    PhotonView view;

    private void Awake()
    {
    }

    void Start()
    {
        GameObject board = Instantiate(gridPrefab, new Vector2(1,1), Quaternion.identity);
        board.transform.parent = GameObject.Find("Grid").transform;

        if (NetworkPlayer.LocalPlayerInstance == null)
        {
            //Debug.LogFormat("GAME MANAGER START");
            Vector3 pos = new Vector3(0, 0, -1);
            GameObject playerGO = PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity);
            NetworkPlayer.LocalPlayerInstance = playerGO;

        }
    }

}

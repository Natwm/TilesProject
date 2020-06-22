﻿using System.Collections;
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
    public GameObject deckPrefab;
    public GameObject handPrefabs;

    private NetworkUi canvas;

    public bool cardIsCreate;

    int nbplayer = 0;

    void Start()
    {
        GameObject board = Instantiate(gridPrefab, new Vector2(1, 1), Quaternion.identity);
        board.transform.parent = GameObject.Find("Grid").transform;

        GameObject deck = Instantiate(deckPrefab, Vector3.zero, Quaternion.identity);
        //GameObject deck = PhotonNetwork.Instantiate(deckPrefab.name, Vector3.zero, Quaternion.identity);


        if (canvas == null)
            canvas = GameObject.Find("Launcher").GetComponent<NetworkUi>();

        if (NetworkPlayer.LocalPlayerInstance == null)
        {
            nbplayer++;
            //Debug.LogFormat("GAME MANAGER START");
            Vector3 pos = new Vector3(1, 2, nbplayer + 2f);
            GameObject playerGO = PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity);
            //playerGO.transform.Rotate(-90f, 0, 0);

            Camera.main.gameObject.transform.position = playerGO.transform.position;
            Camera.main.gameObject.transform.position = new Vector3(Camera.main.gameObject.transform.position.x, Camera.main.gameObject.transform.position.y + 0.5f, Camera.main.gameObject.transform.position.z);
            Camera.main.gameObject.AddComponent<CamController>();
            Camera.main.gameObject.transform.parent = playerGO.transform;
            Camera.main.gameObject.GetComponent<CamController>().playerPos = playerGO;
            playerGO.transform.GetChild(1).parent = Camera.main.gameObject.transform;
            //Camera.main.gameObject.transform.GetChild(0).position = new Vector3(-0.4f, -0.3f, 0.5f);

            playerGO.GetComponent<PlayerMouvement>().cam = Camera.main;
            NetworkPlayer.LocalPlayerInstance = playerGO;

        }
    }

    private void Update()
    {
        GridGen board = GameObject.Find("GridHolder(Clone)").GetComponent<GridGen>();
        if (!cardIsCreate && board != null && canvas.textInfos.isActiveAndEnabled)
        {
            cardIsCreate = GameObject.FindObjectOfType<GestionCartes>().GenerateCards();
            //DrawCard(board.gameObject.GetComponent<GestionCartes>().allCards);
        }
    }



}

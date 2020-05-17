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
    public GameObject deckPrefab;
    public GameObject handPrefabs;

    private NetworkUi canvas;

    [SerializeField] int nbCardToDraw = 2;

    bool cardIsCreate;

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
            //Debug.LogFormat("GAME MANAGER START");
            Vector3 pos = new Vector3(0, 0, -1);
            GameObject playerGO = PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity);
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

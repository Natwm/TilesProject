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

    private NetworkUi canvas;

    [SerializeField] int nbCardToDraw = 2;

    bool cardIsCreate;

    void Start()
    {
        GameObject board = Instantiate(gridPrefab, new Vector2(1,1), Quaternion.identity);
        board.transform.parent = GameObject.Find("Grid").transform;

        if(canvas == null)
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
        if (board != null && canvas.textInfos.isActiveAndEnabled && !cardIsCreate)
        {
            cardIsCreate = board.gameObject.GetComponent<GestionCartes>().GenerateCards();
            DrawCard(board.gameObject.GetComponent<GestionCartes>().allCards);
        }
    }

    private void DrawCard(List<Carte> deck)
    {
        PlayerMouvement[] players = FindObjectsOfType<PlayerMouvement>();
        for (int x = 0; x < players.Length; x++)
        {
            for (int y = 0; y < nbCardToDraw; y++)
            {
                int idCardTodraw = Random.Range(0, deck.Count);
                Carte my1 = deck[idCardTodraw];
                deck.RemoveAt(idCardTodraw);
                players[x].hand.Add(my1);
                Debug.Log(idCardTodraw + " " + players[x].name);
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerData : MonoBehaviour
{
    public int MasterSeed;
    public int commonSeed;
    public bool isMaster;
    System.Random newRandom;

    public Photon.Realtime.Player myPlayer;
    PhotonView playerView;
    // Start is called before the first frame update
    void Start()
    {
        myPlayer = PhotonNetwork.LocalPlayer;
        playerView = GetComponent<PhotonView>();
        if (myPlayer.IsMasterClient)
        {
            //Je set la seed qui sera utilisée par tout les joueurs si je suis le master
            isMaster = true;
            MasterSeed = Random.Range(0,5000);
            commonSeed = MasterSeed;
            playerView.RPC("PassSeed",RpcTarget.OthersBuffered,MasterSeed);
        }
        else
        {
            SetSeed();
        }
        
        
    }

    [PunRPC]
    public void PassSeed(int seedToPass)
    {
        commonSeed = seedToPass;
    }

    public void SetSeed()
    {
        Random.seed = commonSeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            Debug.Log(commonSeed);
        }
    }
}

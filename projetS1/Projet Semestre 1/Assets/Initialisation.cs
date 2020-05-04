using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Initialisation : MonoBehaviour
{
    GridGen iniGrid;
    public PlayerData PlayerData;
    PhotonView player;
    void Start()
    {
        iniGrid = GameObject.Find("GridHolder").GetComponent<GridGen>();
        
    }

    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesBehaviours : MonoBehaviour
{
    [SerializeField] private bool isChest;

    public bool IsChest { get => isChest; set => isChest = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void callColor()
    {

        if (IsChest)
        {
            Debug.Log("win");
            transform.GetChild(0).GetComponent<Renderer>().material.color = Color.yellow;
        }
        else
        {
            transform.GetChild(0).GetComponent<CellData>().ShowTile();
        }
    }
}

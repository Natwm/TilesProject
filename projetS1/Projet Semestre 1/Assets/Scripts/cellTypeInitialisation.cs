using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cellTypeInitialisation : MonoBehaviour
{
    Sprite[] allCellSprites;
    public string Chemin;
    public enum cellType {blank,water,cloud,grass,rock,tree}
    public bool debug = true;
    // Start is called before the first frame update
    void Start()
    {
        allCellSprites = Resources.LoadAll<Sprite>(Chemin);
    }

    public void GetRandomCellType(cellData currentCell)
    {
        
        int randomIndex = UnityEngine.Random.Range(0,allCellSprites.Length);
        Sprite randomSpriteFromList = allCellSprites[randomIndex];
        cellType tempType = (cellType)System.Enum.Parse(typeof(cellType), randomSpriteFromList.name);
        currentCell.cellType = tempType;
        currentCell.assignedSprite = randomSpriteFromList;
    }
}

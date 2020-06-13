using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cellTypeInitialisation : MonoBehaviour
{
    Sprite[] allCellSprites;
    Sprite[] allHiddenCellSprites;
    public string TypeChemin;
    public string HiddenChemin;
    public enum cellType {blank,water,cloud,grass,rock,tree}
    public bool debug = true;
    [Header("Shader References")]
    public string TextureRef;
    public string AmplitudeFloatRef;

    //Animation Variables
    GameObject shovelHolder;
    public int animationITeration;
    GameObject HoleTile;
    GameObject HolePlane;
    GameObject treasure;
    // Start is called before the first frame update
    void Start()
    {
        allCellSprites = Resources.LoadAll<Sprite>(TypeChemin);
        allHiddenCellSprites = Resources.LoadAll<Sprite>(HiddenChemin);
        shovelHolder = Resources.Load<GameObject>("ShovelHolder");
        HoleTile = Resources.Load<GameObject>("HoleTile");
        HolePlane = Resources.Load<GameObject>("HolePlane");
        treasure = Resources.Load<GameObject>("TreasureBox");
    }

    public void GetRandomCellType(cellData currentCell)
    {
        currentCell.SetResources(shovelHolder,animationITeration,HoleTile,HolePlane,treasure);
        currentCell.setCellReferences(TextureRef,AmplitudeFloatRef);
        int randomIndex = UnityEngine.Random.Range(0,allCellSprites.Length);
        int randomIndex2 = UnityEngine.Random.Range(0,allHiddenCellSprites.Length);
        Sprite randomSpriteFromList = allCellSprites[randomIndex];
        Sprite randomHidden = allHiddenCellSprites[randomIndex2];
        cellType tempType = (cellType)System.Enum.Parse(typeof(cellType), randomSpriteFromList.name);
        currentCell.cellType = tempType;
        currentCell.assignedSprite = randomSpriteFromList;
        currentCell.hiddenSprite = randomHidden;
    }
}

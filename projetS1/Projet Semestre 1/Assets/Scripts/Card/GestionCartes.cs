using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder (1000)]
public class GestionCartes : MonoBehaviour
{
    GridGen grid;
    public List<CellData> chestTiles;
    public List<Carte> allCards;
    public CellData[] nearChest;
    public CellData chest;
    public List<CardReader> allCardsDisplay;

    // Start is called before the first frame update
    void Start()
    {

        grid = FindObjectOfType<GridGen>();

    }


    public bool GenerateCards()
    {
        allCards = new List<Carte>();
        allCardsDisplay = new List<CardReader>();
        foreach (CellData item in chestTiles)
        {

            CellData[] nearbyCells = new CellData[9];
            nearbyCells = GetAdjCells(item);
            for (int i = 0; i < nearbyCells.Length; i++)
            {
                //PENSER A CHANGER QUAND ON AURA REMIS EN UN SEUL GO;
                Carte newCarte = new Carte(nearbyCells[i], i);
                allCards.Add(newCarte);
                allCardsDisplay.Add(newCarte.ingameDisplay.GetComponentInChildren<CardReader>());
            }
        }
        return allCards.Count > 0 ;
    }


    public CellData[] GetAdjCells(CellData baseCell)
    {
        List<CellData> adjCell = new List<CellData>();

        foreach (Collider item in Physics.OverlapSphere(baseCell.gameObject.transform.position,1.2f))
        {
            adjCell.Add(item.gameObject.GetComponent<CellData>());
        }
        nearChest = adjCell.ToArray();
        return adjCell.ToArray();
    }

    public Carte FindCardFromID(int _id)
    {
        Carte search = null;
        foreach (Carte item in allCards)
        {
            if (_id == item.cardId)
            {
                search = item;
            }
        }
        return search;
    }

}


[System.Serializable]
public class Carte
{

    public int cardId;
    public string cardName;
    public Sprite front;
    public Sprite back;
    public GameObject cardRenderer;
    public GameObject ingameDisplay;
    public CardReader cardInterface;
    public CellData cardCell;
    public CellData chestCell;
    public cellTypeInitialisation.cellType cardType;
    public enum directionFromChest { up, down, left, right, upleft, upright, downleft, downright }
    public directionFromChest cardDirection;
    public enum visibleFace { front, back };
    public visibleFace currentFace;

    public Carte(CellData _cell, int _id)
    {
        cardCell = _cell;
        cardType = cardCell.cellType;
        cardName = cardType.ToString() + " " + cardCell.gameObject.transform.parent.name;
        cardId = _id;
        front = cardCell.assignedSprite;
        cardDirection = (directionFromChest)cardId;
        back = Resources.Load<Sprite>("sprite/directions/" + cardDirection.ToString());
        cardRenderer = Resources.Load<GameObject>("sprite/carte");
        currentFace = visibleFace.front;
        ingameDisplay = GameObject.Instantiate(cardRenderer);
        ingameDisplay.gameObject.transform.parent = GameObject.Find("Card Holder").transform;
        cardInterface = ingameDisplay.GetComponentInChildren<CardReader>();
        cardInterface.Initialisation();
        cardInterface.UpdateCard(this);
    }

}
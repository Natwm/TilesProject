using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestionCartes : MonoBehaviour
{
    GridGen grid;
    public List<CellData> chestTiles;
    public List<Carte> allCards;
    public List<Carte> allCardsInPlayerHand;
    public CellData[] nearChest;
    public CellData chest;
    public List<CardReader> allCardsDisplay;


    public bool ready = false;

    // Start is called before the first frame update
    void Start()
    {
        grid = FindObjectOfType<GridGen>();
        allCardsInPlayerHand = new List<Carte>();
    }

    public void d(int id)
    {
        Debug.Log(allCards.Count);
        allCards.RemoveAt(id);
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
                //allCardsDisplay.Add(newCarte.ingameDisplay.GetComponentInChildren<CardReader>());
            }
        }
        ready = true;
        return allCards.Count > 0 ;
    }


    public CellData[] GetAdjCells(CellData baseCell)
    {
        CellData[] adjCell = new CellData[8];
        //Y VERTICAL X HORIZONTAL 
        Vector3Int upCellVector = new Vector3Int(baseCell.gridPos.x, baseCell.gridPos.y + 1, baseCell.gridPos.z);
        Vector3Int downCellVector = new Vector3Int(baseCell.gridPos.x, baseCell.gridPos.y - 1, baseCell.gridPos.z);
        Vector3Int leftCellVector = new Vector3Int(baseCell.gridPos.x - 1, baseCell.gridPos.y, baseCell.gridPos.z);
        Vector3Int rightCellVector = new Vector3Int(baseCell.gridPos.x + 1, baseCell.gridPos.y, baseCell.gridPos.z);
        Vector3Int upLeftCellVector = new Vector3Int(baseCell.gridPos.x - 1, baseCell.gridPos.y + 1, baseCell.gridPos.z);
        Vector3Int upRightCellVector = new Vector3Int(baseCell.gridPos.x + 1, baseCell.gridPos.y + 1, baseCell.gridPos.z);
        Vector3Int downLeftCellVector = new Vector3Int(baseCell.gridPos.x - 1, baseCell.gridPos.y - 1, baseCell.gridPos.z);
        Vector3Int downRightCellVector = new Vector3Int(baseCell.gridPos.x + 1, baseCell.gridPos.y - 1, baseCell.gridPos.z);
        foreach (CellData item in grid.allCell)
        {
            if (item.gridPos == upCellVector)
            {

                adjCell[0] = item;
            }
            else if (item.gridPos == downCellVector)
            {
                adjCell[1] = item;
            }
            else if (item.gridPos == leftCellVector)
            {
                adjCell[2] = item;
            }
            else if (item.gridPos == rightCellVector)
            {
                adjCell[3] = item;
            }
            else if (item.gridPos == upLeftCellVector)
            {
                adjCell[4] = item;
            }
            else if (item.gridPos == upRightCellVector)
            {
                adjCell[5] = item;
            }
            else if (item.gridPos == downLeftCellVector)
            {
                adjCell[6] = item;
            }
            else if (item.gridPos == downRightCellVector)
            {
                adjCell[7] = item;
            }
        }
        nearChest = adjCell;
        return adjCell;
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
    [SerializeField]private CardState state = CardState.LOCK;

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
    public enum CardState { LOCK, UNLOCK };
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
        currentFace = visibleFace.back;
    }

    public void Create()
    {
        ingameDisplay = GameObject.Instantiate(cardRenderer);
        ingameDisplay.gameObject.transform.SetParent(GameObject.Find("Card Holder").transform);
        cardInterface = ingameDisplay.GetComponentInChildren<CardReader>();
        cardInterface.Initialisation();
        cardInterface.UpdateCard(this);
    }

    public void CanTurn()
    {
        State = CardState.UNLOCK;
    }

    public CardState State { get => state; set => state = value; }

}
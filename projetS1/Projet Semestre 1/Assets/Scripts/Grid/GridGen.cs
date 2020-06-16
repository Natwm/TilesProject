using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GridGen : MonoBehaviour
{
    [Space]
    [Header("Param")]
    public Vector2Int gridSize;
    public GameObject gridObject;
    BoxCollider gridObjectCollider;
    public CellData[] allCell;
    Vector3 realPos = Vector3.zero;
    public int chestNumber;
    public int m_seed;
    private GestionCartes cardScript;
    public Vector3 originCorner;
    public Vector3 farCorner;

    [Space]
    [Header("Debug")]
    public cellTypeInitialisation.cellType typeToSearch;
    List<CellData> temporaryListOfCells;
    Color cellBaseCol;

    private void Awake()
    {
        Debug.Log(GameObject.Find("Launcher").GetComponent<Launcher>().GameSeed);
        Random.seed = GameObject.Find("Launcher").GetComponent<Launcher>().GameSeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        cardScript = GameObject.FindObjectOfType<GestionCartes>();

        allCell = new CellData[gridSize.x * gridSize.y];
        gridObjectCollider = gridObject.transform.GetChild(0).gameObject.GetComponent<BoxCollider>();
        CreateGrid();

        cellBaseCol = gridObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().sharedMaterial.color;
    }

    // X correspond à l'horizontal Y correspond au vertical
    public void CreateGrid()
    {
        Vector2Int gridPos = new Vector2Int(1, 1);
        GameObject objToSpawn;
        float shiftSize = gridObjectCollider.size.x;
        int incrémentIndex = 0;
        //incrément Y


        for (int i = 0; i < gridSize.y; i++)
        {
            //incrément X
            for (int n = 0; n < gridSize.x; n++)
            {

                if (i == 0 && n == 0)
                {
                    originCorner = realPos;
                    originCorner.x -= shiftSize;
                    originCorner.y -= shiftSize;
                }
                if (i == gridSize.y - 1 && n == gridSize.x - 1)
                {
                    farCorner = realPos;
                }
                objToSpawn = GameObject.Instantiate(gridObject, realPos, new Quaternion(0f, 0f, 0f, 0f));
                objToSpawn.transform.parent = gameObject.transform;
                objToSpawn.name = gridPos.ToString();
                objToSpawn.transform.Rotate(new Vector3(90, 0, 0));
                Vector3Int tempPos = new Vector3Int(gridPos.x, gridPos.y, 0);
                CellData tempCellData = objToSpawn.transform.GetChild(0).gameObject.GetComponent<CellData>();
                tempCellData.gridPos = tempPos;
                allCell[incrémentIndex] = tempCellData;
                //Incrément des variables X
                gridPos.x++;
                realPos.x += shiftSize;
                incrémentIndex++;
            }
            //Incrément des variables Y
            gridPos.y++;
            realPos.z += shiftSize;
            //Remise à 0 des x
            realPos.x = 0;
            gridPos.x = 1;
        }
        SpawnChest(chestNumber);
    }

    //Va chercher x cases aléatoire pour leur assigner le chest. 
    //IMPORTANT : Pour l'instant, la logique de génération de cartes peut fonctionner avec plusieurs chests mais j'ai pas encore testé.
    public void SpawnChest(int numberOfChest)
    {
        cardScript.chestTiles = new List<CellData>();
        for (int i = 0; i < numberOfChest; i++)
        {
            Vector3Int chestPos = new Vector3Int(Random.Range(2, gridSize.x), Random.Range(2, gridSize.x), 0);
            //Debug.Log(chestPos);
            foreach (CellData item in allCell)
            {
                if (chestPos == item.gridPos)
                {
                    item.isTreasure = true;
                    cardScript.chestTiles.Add(item);
                }
            }
        }
    }


    // Retourne une liste de toute les cases d'un type
    public List<CellData> GetAllCellFromType(cellTypeInitialisation.cellType typeToCheck)
    {
        List<CellData> listOfAllObjects = new List<CellData>();
        foreach (CellData item in allCell)
        {
            if (item.cellType == typeToCheck)
            {
                listOfAllObjects.Add(item);
            }
        }
        return listOfAllObjects;
    }

    //Fonction de débug. Passe la couleur de toute les cases d'un type en jaune
    public void HighlightTypeOfCell(cellTypeInitialisation.cellType type)
    {
        ResetTiles();
        temporaryListOfCells = new List<CellData>();
        temporaryListOfCells = GetAllCellFromType(type);
        foreach (CellData item in temporaryListOfCells)
        {
            item.ShowTile("");
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene("Simon");
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            HighlightTypeOfCell(typeToSearch);
        }
        if (Input.GetKeyUp(KeyCode.T))
        {
            foreach (CellData item in allCell)
            {
                if (item.isTreasure)
                {
                    item.objMesh.material.color = new Color(255f, 255f, 0, 0.50f);
                }
            }
        }
    }


    //Permet de repasser les cases dans leur couleur initiale
    void ResetTiles()
    {
        if (temporaryListOfCells != null)
        {
            foreach (CellData item in temporaryListOfCells)
            {
                item.objMesh.material.color = item.baseCol;
            }
        }
    }

}

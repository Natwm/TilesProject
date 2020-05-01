using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridGen : MonoBehaviour
{
    [Space]
    [Header("Param")]
    public Vector2Int gridSize;
    public GameObject gridObject;
    BoxCollider2D gridObjectCollider2D;
    public cellData[] allCell;
    Vector3 realPos = Vector3.zero;
    public int chestNumber;
    
    [Space]
    [Header("Debug")]
    public cellTypeInitialisation.cellType typeToSearch;
    List<cellData> temporaryListOfCells;
    Color cellBaseCol;

    // Start is called before the first frame update
    void Start()
    {
        allCell = new cellData[gridSize.x*gridSize.y];
        gridObjectCollider2D = gridObject.transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>();
        CreateGrid();
        cellBaseCol = gridObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().sharedMaterial.color;
    }

    // X correspond à l'horizontal Y correspond au vertical
    public void CreateGrid()
    {
        Vector2Int gridPos = new Vector2Int(1,1);
        GameObject objToSpawn;
        float shiftSize = gridObjectCollider2D.size.x;
        int incrémentIndex = 0;
        //incrément Y
        for (int i = 0; i < gridSize.y; i++)
        {
            //incrément X
            for (int n = 0; n < gridSize.x; n++)
            {
                objToSpawn = GameObject.Instantiate(gridObject,realPos,new Quaternion (0f,0f,180f,0f));
                objToSpawn.transform.parent = gameObject.transform;
                objToSpawn.name = gridPos.ToString();
                Vector3Int tempPos = new Vector3Int(gridPos.x, gridPos.y,0);
                cellData tempCellData = objToSpawn.transform.GetChild(0).gameObject.GetComponent<cellData>();
                tempCellData.gridPos = tempPos;
                allCell[incrémentIndex] = tempCellData;
                //Incrément des variables X
                gridPos.x++;
                realPos.x += shiftSize;
                incrémentIndex++;
            }
            //Incrément des variables Y
            gridPos.y++;
            realPos.y += shiftSize;
            //Remise à 0 des x
            realPos.x = 0;
            gridPos.x = 1;
        }
        SpawnChest(chestNumber);
    }

    public void SpawnChest(int numberOfChest)
    {

        for (int i = 0; i < numberOfChest; i++)
        {

            Vector3Int chestPos = new Vector3Int(Random.Range(2,gridSize.x), Random.Range(2, gridSize.x),0);
            Debug.Log(chestPos);
            foreach (cellData item in allCell)
            {
                if (chestPos == item.gridPos)
                {
                    item.isTreasure = true;
                }
            }
        }
    }
    public List<cellData> GetAllCellFromType(cellTypeInitialisation.cellType typeToCheck)
    {
        List<cellData> listOfAllObjects = new List<cellData>();
        foreach (cellData item in allCell)
        {
            if (item.cellType == typeToCheck)
            {
                listOfAllObjects.Add(item);
            }
        }
        return listOfAllObjects;
    }

    public void HighlightTypeOfCell(cellTypeInitialisation.cellType type)
    {
        ResetTiles();
        temporaryListOfCells = new List<cellData>();
        temporaryListOfCells = GetAllCellFromType(type);
        foreach (cellData item in temporaryListOfCells)
        {
            item.objMesh.material.color = new Color(0,255f,0, 0.50f);
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
            foreach (cellData item in allCell)
            {
                if (item.isTreasure)
                {
                    item.objMesh.material.color = new Color(255f, 255f, 0, 0.50f);
                }
            }
        }
    }

    void ResetTiles()
    {
        
        if (temporaryListOfCells != null)
        {
            foreach (cellData item in temporaryListOfCells)
            {
                item.objMesh.material.color = item.baseCol;
            }
        }
    }
}

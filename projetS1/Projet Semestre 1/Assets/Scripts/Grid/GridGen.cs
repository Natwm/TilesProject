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
    PoissonInterface trees;
    public CellData[] allCell;
    Vector3 realPos = Vector3.zero;
    public int chestNumber;
    public GestionCartes cardScript;
    public Vector3 originCorner;
    public Vector3 farCorner;
    ParticleSystem TopLeavesPs;
    GameObject TopLeaves;
    [Space]
    [Header("Debug")]
    public cellTypeInitialisation.cellType typeToSearch;
    List<CellData> temporaryListOfCells;
    Color cellBaseCol;
    GameObject[] AllLandmarks;
    GameObject Tiles;
    GameObject AllBorders;
    GameObject Forest;
    // Start is called before the first frame update

    private void Awake()
    {
        Random.seed = GameObject.Find("Launcher").GetComponent<Launcher>().GameSeed;
    }

    void Start()
    {
        Tiles = new GameObject();
        Tiles.name = "Cases";
        AllBorders = new GameObject();
        AllBorders.name = "Bordures";
        AllLandmarks = Resources.LoadAll<GameObject>("prefabs/landmarks");
        allCell = new CellData[gridSize.x*gridSize.y];
        gridObjectCollider = gridObject.transform.GetChild(0).gameObject.GetComponent<BoxCollider>();
        GameObject TopLeavesInst = Resources.Load<GameObject>("prefabs/TopLeaves");
        TopLeaves = Instantiate(TopLeavesInst);
        TopLeavesPs = TopLeaves.GetComponent<ParticleSystem>();
        GameObject Insttrees = Resources.Load<GameObject>("prefabs/PoissonDiscSampling");
        GameObject actualPs = Instantiate(Insttrees);
        actualPs.transform.SetPositionAndRotation(new Vector3(-0.5f,-0.5f,0),actualPs.transform.rotation);
        trees = actualPs.GetComponent<PoissonInterface>();
        trees.LoadResources();
        CreateGrid();
        Forest = trees.ActualForest();
        Tiles.transform.parent = transform;
        AllBorders.transform.parent = transform;
        Forest.transform.SetPositionAndRotation(gridObject.transform.position, Quaternion.identity/*gridObject.transform.rotation*/);
        Forest.transform.parent = transform;
        TopLeaves.transform.parent = transform;
        transform.SetPositionAndRotation(transform.position,Quaternion.Euler(90,0,0));

        
    }

    
    // X correspond à l'horizontal Y correspond au vertical
    public void CreateGrid()
    {
        cardScript = GameObject.FindObjectOfType<GestionCartes>();
        Vector2Int gridPos = new Vector2Int(1,1);
        GameObject objToSpawn;
        Vector3 RealGridPos;
        int ParticleSpawnRate;
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
                if (i == gridSize.y -1 && n == gridSize.x -1)
                {
                    farCorner = realPos;
                }
                objToSpawn = GameObject.Instantiate(gridObject,realPos,new Quaternion (0f,0f,180f,0f));
                objToSpawn.transform.parent = Tiles.transform;
                objToSpawn.name = gridPos.ToString();
                Vector3Int tempPos = new Vector3Int(gridPos.x, gridPos.y,0);
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
            realPos.y += shiftSize;
            //Remise à 0 des x
            realPos.x = 0;
            gridPos.x = 1;
        }
        ParticleSpawnRate = (gridSize.x * gridSize.y) / 10;
        RealGridPos = new Vector3(gridSize.x * gridObject.transform.localScale.x, gridSize.y * gridObject.transform.localScale.y,0);
        var psRate = TopLeavesPs.emission;
        psRate.rateOverTime = ParticleSpawnRate;
        var psShape = TopLeavesPs.shape;
        psShape.scale = new Vector3(RealGridPos.x, RealGridPos.y,0);
        TopLeaves.transform.SetPositionAndRotation(new Vector3((RealGridPos.x/2)-1,(RealGridPos.y/2)-1,-5),TopLeaves.transform.rotation);
        GenerateForest(RealGridPos);
       // GenerateLandmark();
        SpawnChest(chestNumber);

    }

    public void GenerateLandmark()
    {
        int randomLandmark = Random.Range(0, AllLandmarks.Length);
        Vector3 randomGridPos = new Vector3(gridSize.x*gridObject.transform.localScale.x/2, 1,gridSize.y * gridObject.transform.localScale.y/2);
        GameObject landmark = Instantiate(AllLandmarks[randomLandmark]);
        landmark.transform.SetPositionAndRotation(randomGridPos,landmark.transform.rotation);

    }

    public void GenerateForest(Vector3 RealGridSize)
    {
        GameObject borderObject = Resources.Load<GameObject>("prefabs/Border");
        GameObject[] borderTiles = new GameObject[4];
        Vector3 halfBorderSize;
        Vector2 TextureTiling;
        BoxCollider borderBox;
        Vector3 treePos;
        for (int i = 0; i < 4; i++)
        {
            GameObject instanciateObj = Instantiate(borderObject);
            //Bas
            if (i == 0)
            {
                TextureTiling = new Vector2(gridSize.x,10);
                borderTiles[i] = instanciateObj;
                borderTiles[i].name = "Down Border";
                borderTiles[i].transform.localScale = new Vector3(RealGridSize.x*0.1f ,1, 1);
                halfBorderSize = borderTiles[i].GetComponent<BoxCollider>().bounds.size;
                borderBox = borderTiles[i].GetComponent<BoxCollider>();
                borderTiles[i].GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex",TextureTiling);
                borderTiles[i].transform.SetPositionAndRotation(new Vector3(RealGridSize.x/2-1,Mathf.Round(-1-halfBorderSize.y*0.75f),0),borderTiles[i].transform.rotation);
                treePos = new Vector3(borderTiles[i].transform.position.x - borderBox.size.x* borderTiles[i].transform.localScale.x/2, Mathf.Round(borderTiles[i].transform.position.y+halfBorderSize.y/2)-1.5f);
                trees.SpawnTrees(new Vector2(RealGridSize.x,3),borderTiles[i],treePos,1);
            }
            //Haut
            else if (i == 1)
            {
                TextureTiling = new Vector2(gridSize.x, 10);
                borderTiles[i] = instanciateObj;
                borderTiles[i].transform.localScale = new Vector3(RealGridSize.x * 0.1f, 1, 1);
                borderTiles[i].name = "Up Border";
                halfBorderSize = borderTiles[i].GetComponent<BoxCollider>().bounds.size;
                borderBox = borderTiles[i].GetComponent<BoxCollider>();
                borderTiles[i].GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", TextureTiling);
                borderTiles[i].transform.SetPositionAndRotation(new Vector3(RealGridSize.x / 2-1, Mathf.Round(RealGridSize.y-1 + halfBorderSize.y * 0.75f), 0), borderTiles[i].transform.rotation);
                treePos = new Vector3(borderTiles[i].transform.position.x - borderBox.size.x * borderTiles[i].transform.localScale.x / 2, Mathf.Round(borderTiles[i].transform.position.y - halfBorderSize.y / 2)-1.5f );
                trees.SpawnTrees(new Vector2(RealGridSize.x, 3), borderTiles[i], treePos,1);
            }
            //Gauche
            else if (i == 2)
            {
                TextureTiling = new Vector2(10, gridSize.y);
                borderTiles[i] = instanciateObj;
                borderTiles[i].transform.localScale = new Vector3(1, 1, RealGridSize.y * 0.1f);
                borderTiles[i].name = "Left Border";
                halfBorderSize = borderTiles[i].GetComponent<BoxCollider>().bounds.size;
                borderBox = borderTiles[i].GetComponent<BoxCollider>();
                borderTiles[i].GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", TextureTiling);
                borderTiles[i].transform.SetPositionAndRotation(new Vector3(Mathf.Round(-1 - halfBorderSize.y * 0.75f), RealGridSize.y / 2 - 1 , 0), borderTiles[i].transform.rotation);
                treePos = new Vector3(Mathf.Round(borderTiles[i].transform.position.x - halfBorderSize.x / 2) + 3.5f,borderTiles[i].transform.position.y - (borderBox.size.z/2 * borderTiles[i].transform.localScale.z ),0);
                trees.SpawnTrees(new Vector2(3,RealGridSize.y), borderTiles[i], treePos,1);
            }
            //Droite
            else if (i == 3)
            {
                TextureTiling = new Vector2(10, gridSize.y);
                borderTiles[i] = instanciateObj;
                borderTiles[i].transform.localScale = new Vector3(1, 1, RealGridSize.y * 0.1f);
                borderTiles[i].name = "Right Border";
                halfBorderSize = borderTiles[i].GetComponent<BoxCollider>().bounds.size;
                borderBox = borderTiles[i].GetComponent<BoxCollider>();
                borderTiles[i].GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", TextureTiling);
                borderTiles[i].transform.SetPositionAndRotation(new Vector3(Mathf.Round(RealGridSize.y-1 + halfBorderSize.y *0.75f), RealGridSize.y / 2-1, 0), borderTiles[i].transform.rotation);
                treePos = new Vector3(Mathf.Round(borderTiles[i].transform.position.x - halfBorderSize.x / 2) - 2.5f, borderTiles[i].transform.position.y - (borderBox.size.z / 2 * borderTiles[i].transform.localScale.z), 0);
                trees.SpawnTrees(new Vector2(3, RealGridSize.y), borderTiles[i], treePos,1);
            }
            borderTiles[i].GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(Random.Range(0f,100f), Random.Range(0f, 100f)));
            borderTiles[i].transform.parent = AllBorders.transform;
        }
    }
    //Va chercher x cases aléatoire pour leur assigner le chest. 
    //IMPORTANT : Pour l'instant, la logique de génération de cartes peut fonctionner avec plusieurs chests mais j'ai pas encore testé.
    public void SpawnChest(int numberOfChest)
    {
        cardScript.chestTiles = new List<CellData>();
        for (int i = 0; i < numberOfChest; i++)
        {

            Vector3Int chestPos = new Vector3Int(Random.Range(2,gridSize.x), Random.Range(2, gridSize.x),0);
            
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
            item.ShowGraphics();
        }
    }

    //Permet de repasser les cases dans leur couleur initiale
    void ResetTiles()
    {
        
        if (temporaryListOfCells != null)
        {
            foreach (CellData item in temporaryListOfCells)
            {
                item.HideGraphics();
            }
        }
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellData : MonoBehaviour
{
    cellTypeInitialisation masterCell;
    public cellTypeInitialisation.cellType cellType = cellTypeInitialisation.cellType.blank;
    public Sprite assignedSprite;
    public Sprite hiddenSprite;
    public float Glow_Amplitude;
    public bool isTreasure;
    public Vector3Int gridPos;
    public MeshRenderer objMesh;
    public Color baseCol;
    string shaderTextureRef;
    string amplitudeShaderRef;
    public ParticleSystem leavesBurst;

    //Code de nathan
    [SerializeField] private bool canPlantMine = false;
    [SerializeField] private m_State state = m_State.Hide;
    [SerializeField] private List<Mine> m_ListMine = new List<Mine>();
    public enum m_State { Hide, Show }

    // Dig Variables
    GameObject shovelAnimation;
    ParticleSystem dirtBurst;
    GameObject HoleTile;
    float shiftAmount = -1;
    float planeShift = 0.1f;
    float gradualDig = 0.1f;
    public int index = 0;
    public int maxIterationStep;
    GameObject HolePlane;
    public GameObject instanciatedHolePlane;
    Vector3 planePos;
    GameObject treasureBox;
    GameObject TreasureInstance;
    // Start is called before the first frame update
    void Start()
    {
        masterCell = FindObjectOfType<cellTypeInitialisation>();
        masterCell.GetRandomCellType(this);
        
        objMesh = gameObject.GetComponent<MeshRenderer>();
        if (masterCell.debug)
        {
            objMesh.material.SetTexture("_MainTex", assignedSprite.texture);
             
        }
        HideGraphics();    
    }

    public void Bomb(string playerName)
    {
        if (m_ListMine.Count == 0)
        {
            //????
            objMesh.material.color = Color.yellow;
            canPlantMine = true;
        }
        else
        {
            foreach (Mine item in m_ListMine)
            {
                if (item.BombOwner != playerName)
                {
                    ///????
                    objMesh.material.color = Color.yellow;
                    canPlantMine = true;
                }
            }
        }
    }

    public void PlantBomb(PlayerMouvement.Bomb type, string owner)
    {
        Mine newMine = new Mine();
        newMine.BombOwner = owner;
        switch (type)
        {
            case PlayerMouvement.Bomb.RED:
                objMesh.material.color = Color.red;
                newMine.BombState = Mine.m_BombState.RED;
                break;

            case PlayerMouvement.Bomb.BLACK:
                objMesh.material.color = Color.yellow;
                newMine.BombState = Mine.m_BombState.BLACK;
                break;

            case PlayerMouvement.Bomb.WHITE:
                objMesh.material.color = Color.magenta;
                newMine.BombState = Mine.m_BombState.WHITE;
                break;

            case PlayerMouvement.Bomb.Nothing:
                newMine.BombState = Mine.m_BombState.Nothing;
                break;

            default:
                break;
        }
        m_ListMine.Add(newMine);
    }

    public void UpdateBombState(PlayerMouvement.Bomb type, string owner)
    {
        Debug.Log(" UpdateBombState " + owner);
        Mine enemyMine = new Mine();
        enemyMine.BombOwner = owner;
        switch (type)
        {
            case PlayerMouvement.Bomb.RED:
                enemyMine.BombState = Mine.m_BombState.RED;
                break;

            case PlayerMouvement.Bomb.BLACK:
                enemyMine.BombState = Mine.m_BombState.BLACK;
                break;

            case PlayerMouvement.Bomb.WHITE:
                enemyMine.BombState = Mine.m_BombState.WHITE;
                break;

            case PlayerMouvement.Bomb.Nothing:
                enemyMine.BombState = Mine.m_BombState.Nothing;
                break;

            default:
                break;
        }
        AddToList(enemyMine);

    }

    public void ResetTile(Mine toReset)
    {
        Listbomb.Remove(toReset);
    }

    public void ResetTileMine(Mine toReset)
    {
        Listbomb.Remove(toReset);
        state = m_State.Hide;
        objMesh.material.color = Color.black;
    }

    public bool ShowTile(string player)
    {
        if (m_ListMine.Count == 0)
        {
            ShowGraphics();
        }
        else
        {
            List<Mine> canChange = new List<Mine>();
            foreach (Mine item in m_ListMine)
            {
                if (item.BombOwner == player)
                {
                    canChange.Add(item);
                }
            }
            if (canChange.Count == 0)
            {
                ShowGraphics();
            }
        }

        return true;
    }

    public void HighlightTile(string playerName)
    {
        if (state == m_State.Show && Listbomb.Count == 0)
        {
            objMesh.material.color = Color.yellow;
        }
        else
        {
            foreach (Mine item in Listbomb)
            {
                if (playerName == item.BombOwner)
                    return;
            }
            objMesh.material.color = Color.yellow;
        }
    }

    public void UnHighlightTile(string playerName)
    {
        if (state == m_State.Show && Listbomb.Count == 0)
        {
            objMesh.material.color = Color.white;
        }
        else
        {
            foreach (Mine item in Listbomb)
            {
                if (playerName == item.BombOwner)
                    return;
            }
            objMesh.material.color = Color.white;
        }
    }

    public bool HideTile(string player)
    {

        if (state == m_State.Show && m_ListMine.Count == 0)
        {
            HideGraphics();
        }
        else if (state == m_State.Show)
        {
            List<Mine> canChange = new List<Mine>();
            foreach (Mine item in m_ListMine)
            {
                if (item.BombOwner == player)
                {
                    canChange.Add(item);
                }
            }
            if (canChange.Count == 0)
            {
                HideGraphics();
                
            }
        }

        return true;
    }

    public bool IsBomb()
    {
        return m_ListMine.Count > 0;
    }

    private void AddToList(Mine mine)
    {
        foreach (Mine item in Listbomb)
        {
            if (mine.BombOwner == item.BombOwner)
                return;
        }
        Listbomb.Add(mine);
    }

    #region GRAPHICS
    public void setCellReferences(string textureRef,string amplitudeRef)
    {
        shaderTextureRef = textureRef;
        amplitudeShaderRef = amplitudeRef;
    }
    public void HideGraphics()
    {
        objMesh.material.SetTexture(shaderTextureRef,hiddenSprite.texture);
        objMesh.material.SetFloat(amplitudeShaderRef, 0);
        state = m_State.Hide;
    }

    public void ShowGraphics()
    {
        objMesh.material.SetFloat(amplitudeShaderRef, 2f);
        objMesh.material.SetTexture(shaderTextureRef, assignedSprite.texture);
        ParticleSystem currentBurst = Instantiate(leavesBurst);
        currentBurst.transform.SetPositionAndRotation(transform.position,currentBurst.transform.localRotation);
        state = m_State.Show;
    }
    public void Dig()
    {
        if (isTreasure && TreasureInstance == null)
        {
            Vector3 treasurePos = new Vector3(transform.position.x,transform.position.y,0.5f);
            TreasureInstance = Instantiate(treasureBox);
            TreasureInstance.transform.SetPositionAndRotation(treasurePos, treasureBox.transform.rotation);
        }
        //Débug
        if (index == 0)
        {
            planePos = new Vector3(transform.position.x, transform.position.y, planeShift);
            objMesh.enabled = false;
            GameObject hole = Instantiate(HoleTile);
            instanciatedHolePlane = Instantiate(HolePlane);
            instanciatedHolePlane.transform.SetPositionAndRotation(planePos,instanciatedHolePlane.transform.rotation);
            ShowGraphics();
            Vector3 holePos = new Vector3(transform.position.x,transform.position.y,transform.position.z);
            hole.transform.SetPositionAndRotation(holePos,hole.transform.rotation);
        }

        if (index < maxIterationStep)
        {
            GameObject newShovel = Instantiate(shovelAnimation);
            Vector3 currPos = new Vector3(transform.position.x, transform.position.y + 1.2f, shiftAmount * 2);
            newShovel.transform.SetPositionAndRotation(currPos, newShovel.transform.rotation);
            newShovel.transform.parent = gameObject.transform;
            newShovel.transform.GetChild(1).GetComponent<Animator>().Play("Dig", 0);
            shiftAmount += gradualDig;
            
            index++;
        }
        if (index == maxIterationStep)
        {
            treasureBox.transform.GetChild(0).GetComponent<Animator>().Play("FoundBox");
        }
        
        
    }

    public void UpdatePlane()
    {
        planePos.z += gradualDig;
        instanciatedHolePlane.transform.SetPositionAndRotation(planePos,instanciatedHolePlane.transform.rotation);
    }
    public void SetResources(GameObject shovel, int maxAnimationIteration, GameObject holeTile, GameObject holePlane,GameObject treasure)
    {
        shovelAnimation = shovel;
        maxIterationStep = maxAnimationIteration;
        HoleTile = holeTile;
        HolePlane = holePlane;
        treasureBox = treasure;
    }
    #endregion

    #region GETTER && SETTER
    public bool CanPlantBomb { get => canPlantMine; set => canPlantMine = value; }
    public m_State State { get => state; set => state = value; }
    public List<Mine> Listbomb { get => m_ListMine; set => m_ListMine = value; }
    #endregion
}

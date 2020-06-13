using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellData : MonoBehaviour
{
    cellTypeInitialisation masterCell;
    public cellTypeInitialisation.cellType cellType = cellTypeInitialisation.cellType.blank;
    public Sprite assignedSprite;
    public bool isTreasure;
    public Vector3Int gridPos;
    public MeshRenderer objMesh;
    public Color baseCol;

    [SerializeField] private bool canPlantBomb = false;
    [SerializeField] private m_State state = m_State.Hide;
    
    [SerializeField] private List<Mine> m_Listbomb = new List<Mine>(); 
    



    public enum m_State { Hide, Show }


    // Start is called before the first frame update
    void Start()
    {
        masterCell = FindObjectOfType<cellTypeInitialisation>();
        masterCell.GetRandomCellType(this);
        //Debug.Log("a");
        if (objMesh == null)
            objMesh = gameObject.GetComponent<MeshRenderer>();

        if (masterCell.debug)
        {
            objMesh.material.SetTexture("_MainTex", assignedSprite.texture);
            baseCol = objMesh.material.color;
        }

        objMesh.material.color = Color.black;
    }

    public void Bomb(string playerName)
    {
        if (m_Listbomb.Count == 0)
        {
            objMesh.material.color = Color.yellow;
            canPlantBomb = true;
        }
        else
        {
            foreach (Mine item in m_Listbomb)
            {
                if(item.BombOwner != playerName)
                {
                    objMesh.material.color = Color.yellow;
                    canPlantBomb = true;
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
        m_Listbomb.Add(newMine);
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

    public bool ShowTile( string player)
    {
        if (m_Listbomb.Count == 0)
        {
            state = m_State.Show;
            objMesh.material.color = Color.white;
        }
        else
        {
            List<Mine> canChange = new List<Mine>();
            foreach (Mine item in m_Listbomb)
            {
                if (item.BombOwner == player)
                {
                    canChange.Add(item);
                }
            }
            if (canChange.Count == 0)
            {
                state = m_State.Show;
                objMesh.material.color = Color.white;
            }
        }

        return true;
    }

    public bool HideTile(string player)
    {

        if (state == m_State.Show && m_Listbomb.Count == 0)
        {
            objMesh.material.color = Color.black;
            state = m_State.Hide;
        }
        else if(state == m_State.Show)
        {
            List<Mine> canChange = new List<Mine>();
            foreach (Mine item in m_Listbomb)
            {
                if (item.BombOwner == player)
                {
                    canChange.Add(item);
                }
            }
            if(canChange.Count == 0)
            {
                objMesh.material.color = Color.black;
                state = m_State.Hide;
            }
        }

        return true;
    }

    public bool IsBomb()
    {
        return m_Listbomb.Count > 0;
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

    #region GETTER && SETTER
    public bool CanPlantBomb { get => canPlantBomb; set => canPlantBomb = value; }
    public m_State State { get => state; set => state = value; }
    public List<Mine> Listbomb { get => m_Listbomb; set => m_Listbomb = value; }
    #endregion*/
}

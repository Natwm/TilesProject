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
    [SerializeField] private m_BombState bombState = m_BombState.Nothing;
    [SerializeField] private string bombOwner;



    public enum m_State { Hide, Show }
    public enum m_BombState { RED, BLACK, WHITE, Nothing };

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

    public void Bomb()
    {
        objMesh.material.color = Color.yellow;
        canPlantBomb = true;
    }

    public void PlantBomb(PlayerMouvement.Bomb type, string owner)
    {
        bombOwner = owner;
        switch (type)
        {
            case PlayerMouvement.Bomb.RED:
                objMesh.material.color = Color.red;
                bombState = m_BombState.RED;
                break;

            case PlayerMouvement.Bomb.BLACK:
                objMesh.material.color = Color.yellow;
                bombState = m_BombState.BLACK;
                break;

            case PlayerMouvement.Bomb.WHITE:
                objMesh.material.color = Color.magenta;
                bombState = m_BombState.WHITE;
                break;

            case PlayerMouvement.Bomb.Nothing:
                bombState = m_BombState.Nothing;
                break;

            default:
                break;
        }

    }

    public void UpdateBombState(PlayerMouvement.Bomb type, string owner)
    {
        Debug.Log(owner);
        bombOwner = owner;
        switch (type)
        {
            case PlayerMouvement.Bomb.RED:
                bombState = m_BombState.RED;
                break;

            case PlayerMouvement.Bomb.BLACK:
                bombState = m_BombState.BLACK;
                break;

            case PlayerMouvement.Bomb.WHITE:
                bombState = m_BombState.WHITE;
                break;

            case PlayerMouvement.Bomb.Nothing:
                bombState = m_BombState.Nothing;
                break;

            default:
                break;
        }
    }

    public void ResetTile()
    {
        Debug.Log("reset " + gameObject.name +"  at position : " + gridPos);
        bombOwner = "";
        bombState = m_BombState.Nothing;
        state = m_State.Show;
        objMesh.material.color = Color.white;
    }

    public bool ShowTile( string player)
    {
        if (bombState == m_BombState.Nothing || player != bombOwner)
        {
            state = m_State.Show;
            objMesh.material.color = Color.white;
        }
        return true;
    }

    public bool HideTile()
    {
        if (state == m_State.Show && bombState == m_BombState.Nothing )
        {
            objMesh.material.color = Color.black;
            state = m_State.Hide;
        }
        return true;
    }

    public bool IsBomb()
    {
        return bombState != m_BombState.Nothing;
    }

    #region GETTER && SETTER
    public m_BombState BombState { get => bombState; set => bombState = value; }
    public bool CanPlantBomb { get => canPlantBomb; set => canPlantBomb = value; }
    public m_State State { get => state; set => state = value; }
    public string BombOwner { get => bombOwner; set => bombOwner = value; }
    #endregion
}

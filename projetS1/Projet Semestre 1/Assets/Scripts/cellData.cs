using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cellData : MonoBehaviour
{
    cellTypeInitialisation masterCell;
    public cellTypeInitialisation.cellType cellType = cellTypeInitialisation.cellType.blank;
    public Sprite assignedSprite;

    public bool isTreasure;
    public Vector3Int gridPos;
    public MeshRenderer objMesh;
    public Color baseCol;

    // Start is called before the first frame update
    void Start()
    {
        masterCell = FindObjectOfType<cellTypeInitialisation>();
        masterCell.GetRandomCellType(this);

        objMesh = gameObject.GetComponent<MeshRenderer>();
        if (masterCell.debug)
        {
            objMesh.material.SetTexture("_MainTex", assignedSprite.texture);
            baseCol = objMesh.material.color; 
        }

        objMesh.material.color = Color.black;

    }

    public void ShowTile() {
        objMesh.material.color = Color.white;
        objMesh.material.SetTexture("_MainTex", assignedSprite.texture);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cellData : MonoBehaviour
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
        HideTile();
        

    }
    public void setCellReferences(string textureRef,string amplitudeRef)
    {
        shaderTextureRef = textureRef;
        amplitudeShaderRef = amplitudeRef;
    }
    public void HideTile()
    {
        objMesh.material.SetTexture(shaderTextureRef,hiddenSprite.texture);
        objMesh.material.SetFloat(amplitudeShaderRef, 0);
    }

    public void ShowTile()
    {
        objMesh.material.SetFloat(amplitudeShaderRef, 2f);
        objMesh.material.SetTexture(shaderTextureRef, assignedSprite.texture);
        ParticleSystem currentBurst = Instantiate(leavesBurst);
        currentBurst.transform.SetPositionAndRotation(transform.position,currentBurst.transform.localRotation);
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
            ShowTile();
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shrubRender : MonoBehaviour
{
    PoissonInterface poisson;
    public Sprite shrubToRender;
    MeshRenderer shrubMeshRender1;
    MeshRenderer shrubMeshRender2;
    public Sprite[] shrubChoice;
    // Start is called before the first frame update
    void Start()
    {
       // shrubChoice = new Sprite[poisson.shrubs.Length];
        
        shrubMeshRender1 = transform.GetChild(0).GetComponent<MeshRenderer>();
        shrubMeshRender2 = transform.GetChild(1).GetComponent<MeshRenderer>();
    }

   public void setShrub()
    {/*
        poisson = FindObjectOfType<PoissonInterface>();
        shrubChoice = poisson.shrubs;
        int RandomShrub = Random.Range(0, shrubChoice.Length-1);
        shrubToRender = shrubChoice[RandomShrub];
        shrubMeshRender1.material.mainTexture = shrubToRender.texture;
        shrubMeshRender2.material.mainTexture = shrubToRender.texture;
    */
    } 
}

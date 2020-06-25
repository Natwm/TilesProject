using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeMenu : MonoBehaviour
{
    public MeshRenderer[] treeMats;
    Sprite[] treeTex;

    // Start is called before the first frame update
    void Start()
    {
        treeTex = Resources.LoadAll<Sprite>("textures/trees");
        for (int i = 0; i < transform.childCount; i++)
        {
            
            int RandomTree = Random.Range(0, treeTex.Length);
            treeMats[i].material.SetTexture("_MainTex", treeTex[RandomTree].texture);
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

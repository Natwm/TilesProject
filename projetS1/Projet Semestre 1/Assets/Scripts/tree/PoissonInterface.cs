using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonInterface : MonoBehaviour
{
    GridGen grid;
    GameObject leavesPS;
    public float radius;
    public GameObject[] spawnOnpoint;
    public Texture2D[] treeTexture;
    public int rejectionSamples = 30;
    public float minSize;
    public float maxSize;
    List<Vector2> points;
    Vector2 regionSize;
    bool Landmark;
    // Start is called before the first frame update
    void Start()
    {
        
     
    }
    public void LoadResources()
    {
        spawnOnpoint = Resources.LoadAll<GameObject>("mesh/trees");
        treeTexture = Resources.LoadAll<Texture2D>("textures/trees");
        grid = FindObjectOfType<GridGen>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            SpawnTrees(grid.farCorner,gameObject,gameObject.transform.position,1.5f);
        }
    }
    public GameObject ActualForest()
    {
        
        GameObject forest = new GameObject();
        forest.transform.SetPositionAndRotation(new Vector3(-1, -1, 0), forest.transform.rotation);
        forest.name = "Forest";
        SpawnTrees(grid.farCorner, forest, gameObject.transform.position, 1.5f);
        return forest;
        
    }

    public void SpawnTrees(Vector2 inputSize,GameObject holder,Vector3 holderPosition,float radius)
    {
       
        regionSize = inputSize;
        points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
        GameObject subholder = new GameObject();
        
        
        
        foreach (Vector2 item in points)
        {
            
            float randomRotation = Random.Range(0f, 360f);
            int randomTree = Random.Range(0, spawnOnpoint.Length);
            int randomTexture = Random.Range(0, treeTexture.Length);
            float size = Random.Range(minSize, maxSize);
            GameObject test;
            Vector3 translatedPos = new Vector3(item.x, item.y, 0);
            test = Instantiate(spawnOnpoint[randomTree]);
            test.transform.SetPositionAndRotation(translatedPos, test.transform.rotation);
            test.transform.localScale = new Vector3(size, size, 50);
            test.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", treeTexture[randomTexture]);
            
            
            
            
            
            
            test.transform.Rotate(Vector3.forward, randomRotation);
            test.transform.parent = subholder.transform;
            
        }
        
        subholder.transform.SetPositionAndRotation(holderPosition, subholder.transform.rotation);
        subholder.transform.parent = holder.transform;
    }

}

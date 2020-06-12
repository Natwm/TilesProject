using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Camera main;
    // Start is called before the first frame update
    void Start()
    {
        main = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPostition = new Vector3(main.transform.position.x,
                                       this.transform.position.y+90,
                                       main.transform.position.z);
        this.transform.LookAt(targetPostition,Vector3.up);
        
    }
}

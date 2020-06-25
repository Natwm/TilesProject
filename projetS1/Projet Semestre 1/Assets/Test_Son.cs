using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Son : MonoBehaviour
{


    FMODUnity.StudioEventEmitter TestEvent;
    
    // Start is called before the first frame update
    void Start()
    {
        TestEvent = GetComponent<FMODUnity.StudioEventEmitter>();
    }

    // Update is called once per frame
  
}

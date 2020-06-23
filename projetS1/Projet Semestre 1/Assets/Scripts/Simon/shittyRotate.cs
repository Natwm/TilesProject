using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shittyRotate : MonoBehaviour
{
    public bool rotate;
    [Range(0.0f, 10.0f)]
    public float shittyRotateSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rotate)
        {
            transform.parent.transform.Rotate(Vector3.forward, shittyRotateSpeed);
        }
    }
    public void ToggleRotate()
    {
        if (!rotate)
        {
            rotate = true;
        }
        else
        {
            rotate = false;
        }
    }
}

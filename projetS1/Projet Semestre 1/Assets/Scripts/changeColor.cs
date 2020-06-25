using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeColor : MonoBehaviour
{
    public GameObject basecolor;
    public GameObject topColor;
    public GameObject handleColor;
    public Color myColor;


    public void ChangeColor(Color color)
    {
        basecolor.GetComponent<Renderer>().material.color = color;
        handleColor.GetComponent<Renderer>().material.color = color;
        topColor.GetComponent<Renderer>().material.color = color;

        myColor = color;
    }
}

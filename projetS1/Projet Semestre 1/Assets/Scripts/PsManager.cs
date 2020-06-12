using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsManager : MonoBehaviour
{
    ParticleSystem burst;
    cellData currentCell;
    bool hasCalled;
    // Start is called before the first frame update
    void Start()
    {
        burst = gameObject.transform.parent.GetChild(0).GetComponent<ParticleSystem>();
        currentCell = gameObject.transform.parent.transform.parent.GetComponent<cellData>();
    }
    public void PlayParticles()
    {
        if (burst != null)
        {
            burst.Play();
        }
    }

    public void StepAnim()
    {
        if (!hasCalled)
        {

            for (int i = 0; i < 1; i++)
            {
                currentCell.Dig();
            }
                   
            Destroy(this.gameObject);
            hasCalled = true;
        }

    }
   public void PlaneUpdate()
    {
        if (currentCell.index < currentCell.maxIterationStep)
        {
            currentCell.UpdatePlane();
        }
        else
        {
            Destroy(currentCell.instanciatedHolePlane);
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

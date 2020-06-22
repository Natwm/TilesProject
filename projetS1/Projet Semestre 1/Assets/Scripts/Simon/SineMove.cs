using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMove : MonoBehaviour
{
    public bool sine = false;
    public float sinWave;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sine)
        {
            sinWave = Mathf.Clamp(Mathf.Sin(2* Time.time),-0.1f,0.1f);
            Vector3 sineMove = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            sineMove.z += sinWave/100;
            Mathf.Clamp(sineMove.z, -0.1f, 0.1f);
            transform.SetPositionAndRotation(sineMove, transform.localRotation);
            
        }
        
    }
    public void ToggleSin()
    {
        if (!sine)
        {
            gameObject.GetComponent<Animator>().enabled = false;
            sine = true;
        }
    }
}

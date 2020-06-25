using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSonMenu : MonoBehaviour
{
    FMODUnity.StudioEventEmitter myEmit;

    [FMODUnity.EventRef]
    public string iWantToPlayCustomEvent;

    FMOD.Studio.EventInstance mySoundToPlay;
    // Start is called before the first frame update
    void Start()
    {
        myEmit = GetComponent<FMODUnity.StudioEventEmitter>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.K))
        {
            mySoundToPlay = FMODUnity.RuntimeManager.CreateInstance(iWantToPlayCustomEvent);
            myEmit.Event = iWantToPlayCustomEvent;
            myEmit.Play();

        }
    }
    public void playSound()
    {
        if (myEmit != null)
        {
            myEmit.Play();
        }
        
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSonMenu : MonoBehaviour
{
   public FMODUnity.StudioEventEmitter myEmit;

    [FMODUnity.EventRef]
    public string iWantToPlayCustomEvent;

    FMOD.Studio.EventInstance mySoundToPlay;
    // Start is called before the first frame update
   
   
    public void playSound()
    {
        if (myEmit != null)
        {
            myEmit.Play();
        }
        
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Feedback : MonoBehaviour
{
    ParticleSystem psBurst;
    public enum Receiver {receive,give}
    public enum MineType {spy,draw,reveal}
    public Receiver quiAffiche;
    public MineType mineAffiche;
    public Sprite[] allDynamicSprites;

    string loadIcon;
    public Image icon;
    Sprite iconSprite;

    public Image topText;
    Sprite topSprite;
    string loadTop;

    public Image bottomText;
    Sprite bottomSprite;
    string loadBottom;

    Animator feedbackAnim;
    public Image fond;
    // Start is called before the first frame update
    void Start()
    {
        psBurst = transform.GetChild(0).GetComponent<ParticleSystem>();
        allDynamicSprites = Resources.LoadAll<Sprite>("ui/minefeedback/dynamic");
        feedbackAnim = GetComponent<Animator>();
        feedbackAnim.enabled = false;

        fond.enabled = false;
        icon.enabled = false;
        topText.enabled = false;
        bottomText.enabled = false;

    }

    public void UiBurst()
    {
        psBurst.Play();
    }

    public void SortUi()
    {
        icon.enabled = true;
        topText.enabled = true;
        bottomText.enabled = true;
        fond.enabled = true;
        loadIcon ="";
        loadTop = "";
        loadBottom = "";

        string type = mineAffiche.ToString();
        string receiver = quiAffiche.ToString();

        loadIcon = type + "_icon";
        loadTop = type + "_" + receiver + "_top";
        loadBottom = type + "_" + receiver + "_bottom";

        foreach (Sprite item in allDynamicSprites)
        {
            if (item.name == loadIcon)
            {
                iconSprite = item;
            }
            if (item.name == loadTop)
            {
                topSprite = item;
            }
            if (item.name == loadBottom)
            {
                bottomSprite = item;
            }
        }

        icon.sprite = iconSprite;
        icon.SetNativeSize();
        topText.sprite = topSprite;
        topText.SetNativeSize();
        bottomText.sprite = bottomSprite;
        bottomText.SetNativeSize();
        feedbackAnim.enabled = true;
        feedbackAnim.Play("Apparition_Base",0,0);

    }

    private void Update()
    {
       
    }

    public void ResetAnim()
    {
       // feedbackAnim.enabled = false;
    }
}

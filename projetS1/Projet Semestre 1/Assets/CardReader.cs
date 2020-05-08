using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardReader : MonoBehaviour
{
    SpriteRenderer mainIcon;
    public TextMeshPro description;
    [SerializeField]
    public Carte cardToRead;

    public void Initialisation()
    {
        mainIcon = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        description = gameObject.transform.GetChild(1).GetComponent<TextMeshPro>();
    }
    public void UpdateCard(Carte carteToPass)
    {
        if (mainIcon == null || description == null)
        {
            Initialisation();
        }
        cardToRead = carteToPass;
        SetFace(carteToPass.currentFace);
        description.SetText(carteToPass.cardType.ToString() + '\n' + "id : " + carteToPass.cardId + '\n' + carteToPass.cardDirection.ToString());
    }

    // Set the face of the card to the param enum
    public void SetFace(Carte.visibleFace currentFace)
    {
        switch (currentFace)
        {
            case Carte.visibleFace.back :
                {
                    mainIcon.sprite = cardToRead.back;
                    break;
                }
            case Carte.visibleFace.front:
                {
                    mainIcon.sprite = cardToRead.front;
                    break;
                }
        }
    }

    // Flip the card
    public void SwitchFace(Carte.visibleFace currentFace)
    {
        switch (currentFace)
        {
            case Carte.visibleFace.front:
                {
                    cardToRead.currentFace = Carte.visibleFace.back;
                    SetFace(currentFace);
                    break;
                }
            case Carte.visibleFace.back:
                {
                    cardToRead.currentFace = Carte.visibleFace.front;
                    SetFace(currentFace);
                    break;
                }
        }
    }
   
}

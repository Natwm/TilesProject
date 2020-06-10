using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mine 
{
    [SerializeField] private string bombOwner;
    [SerializeField] private m_BombState bombState = m_BombState.Nothing;

    public string BombOwner { get => bombOwner; set => bombOwner = value; }
    public m_BombState BombState { get => bombState; set => bombState = value; }

    public enum m_BombState { RED, BLACK, WHITE, Nothing };

}

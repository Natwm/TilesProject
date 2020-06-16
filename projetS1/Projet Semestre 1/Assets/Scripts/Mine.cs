using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mine 
{
    [SerializeField] private string m_MineOwner;
    [SerializeField] private m_BombState m_MineState = m_BombState.Nothing;

    public string BombOwner { get => m_MineOwner; set => m_MineOwner = value; }
    public m_BombState BombState { get => m_MineState; set => m_MineState = value; }

    public enum m_BombState { RED, BLACK, WHITE, Nothing };

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mine 
{
    [SerializeField] private string m_MineOwner;
    [SerializeField] private GameObject mineGO = null;
    [SerializeField] private m_BombState m_MineState = m_BombState.Nothing;
    [SerializeField] private GameObject MineBurst;
    ParticleSystem MineParticle;
    public Mine(string owner, GameObject mineGO)
    {
        m_MineOwner = owner;
        mineGO = MineGO;
        MineBurst = Resources.Load<GameObject>("prefabs/mineburst");
        MineParticle = MineBurst.GetComponent<ParticleSystem>();
    }
    public Mine(string owner)
    {
        m_MineOwner = owner;
        MineBurst = Resources.Load<GameObject>("prefabs/mineburst");
        MineParticle = MineBurst.GetComponent<ParticleSystem>();
    }

    public enum m_BombState { RED, BLACK, WHITE, Nothing };

    #region GRAPHICS
    public void SetBurst(CellData celltoburst)
    {
        
        GameObject setBurst = GameObject.Instantiate(MineBurst);
        setBurst.transform.SetPositionAndRotation(celltoburst.gameObject.transform.position, setBurst.transform.rotation);
        Debug.Log("Mine Burst");
        MineParticle.Play();
    }

    #endregion 

    #region GETTER && SETTER
    public string BombOwner { get => m_MineOwner; set => m_MineOwner = value; }
    public m_BombState BombState { get => m_MineState; set => m_MineState = value; }
    public GameObject MineGO { get => mineGO; set => mineGO = value; }
    #endregion

}

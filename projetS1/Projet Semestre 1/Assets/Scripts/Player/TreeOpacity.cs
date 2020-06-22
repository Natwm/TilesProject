using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOpacity : MonoBehaviour
{
    public Vector3 tree;
    public float treeDist;
    public float clampedTreeDist;

    [Header("Tree Opacity")]
    public List<GameObject> treeInVision;
    public List<MeshRenderer> treeMat;
    public Vector3 triggerCenter;
    public AnimationCurve opacityModifier;

    // Update is called once per frame
    void Update()
    {
        CastOpacity();
        TreeOpacityRegulator();
    }

    public void TreeOpacityRegulator()
    {
        triggerCenter = gameObject.transform.position;


        treeDist = Vector2.Distance(new Vector2(1, triggerCenter.y), new Vector2(1, tree.y));
        clampedTreeDist = Mathf.Lerp(0, 1, treeDist / 2f);

        foreach (MeshRenderer item in treeMat)
        {
            if (item != null)
            {
                float treeDistance = Vector2.Distance(new Vector2(1, triggerCenter.y), new Vector2(1, item.transform.position.y));
                float clampedTreeDistance = Mathf.Lerp(0, 1, treeDistance / 2f);
                float alphaTree = opacityModifier.Evaluate(clampedTreeDistance);
                Color alpha = new Color(item.material.color.r, item.material.color.g, item.material.color.b, alphaTree);
                item.material.color = alpha;
            }
        }
    }

    void CastOpacity()
    {
        foreach (MeshRenderer item in treeMat)
        {
            if (item != null)
            {
                float treeDistance = Vector2.Distance(new Vector2(1, triggerCenter.y), new Vector2(1, item.transform.position.y));
                float clampedTreeDistance = Mathf.Lerp(0, 1, treeDistance / 2f);
                float alphaTree = opacityModifier.Evaluate(clampedTreeDistance);
                Color alpha = new Color(item.material.color.r, item.material.color.g, item.material.color.b, 255);
                item.material.color = alpha;
            }
        }

        treeInVision.Clear() ;
        treeMat.Clear();
        
        foreach (Collider tree in Physics.OverlapSphere(transform.position,treeDist))
        {
            if (tree.gameObject.CompareTag("Trees"))
            {
                treeInVision.Add(tree.gameObject);
                treeMat.Add(tree.gameObject.GetComponent<MeshRenderer>());
            }
        }
    }
}

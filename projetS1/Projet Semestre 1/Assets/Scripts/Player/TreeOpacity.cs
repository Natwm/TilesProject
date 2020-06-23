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
    public SphereCollider CamCollider;

    private void Start()
    {
        CamCollider = GetComponent<SphereCollider>();
    }
    // Update is called once per frame
    void Update()
    {
        TreeOpacityRegulation();
    }

    public void TreeOpacityRegulation()
    {
        triggerCenter = CamCollider.bounds.center;
        Vector2 playerTransform = transform.parent.transform.position;

        //treeDist = Vector2.Distance(new Vector2(1, triggerCenter.z), new Vector2(1, tree.z));
        //clampedTreeDist = Mathf.Lerp(0, 1, treeDist / 2f);

        foreach (MeshRenderer item in treeMat)
        {
            if (item != null)
            {
                float treeDistance = Vector2.Distance(playerTransform, item.transform.position);
                float clampedTreeDistance = Mathf.Lerp(0, 1, treeDistance / 2f);
                float alphaTree = opacityModifier.Evaluate(clampedTreeDistance);
                Color alpha = new Color(item.material.color.r, item.material.color.g, item.material.color.b, alphaTree);
                item.material.color = alpha;
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Trees")
        {
            treeInVision.Add(other.gameObject);
            treeMat.Add(other.GetComponent<MeshRenderer>());
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Trees")
        {
            other.GetComponent<MeshRenderer>().material.SetColor("_MainTex", new Color(1, 1, 1, 1));
            treeInVision.Remove(other.gameObject);

            treeMat.Remove(other.GetComponent<MeshRenderer>());

        }
    }
}

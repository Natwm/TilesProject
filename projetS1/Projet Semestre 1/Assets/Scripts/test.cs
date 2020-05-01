using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public Grid grid;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity);
            Debug.Log(grid.LocalToCell(hit.collider.gameObject.transform.position));
            //Debug.Log("pointeur sur camera "+Camera.main.ScreenToWorldPoint(Input.mousePosition));
            //Vector3 a = grid.GetCellCenterLocal(new Vector3Int((int)Camera.main.ScreenToWorldPoint(Input.mousePosition).x, (int)Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0));

        }
    }
}

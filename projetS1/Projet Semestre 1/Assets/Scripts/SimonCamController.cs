using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonCamController : MonoBehaviour
{
    public Vector3 tree;
    float treeDist;
    public float clampedTreeDist;
    public float movementlimit;
    private float xRotation = 0f;
    Transform cameraHolderRotation;
    private float mouseSensitivity = 100f;
    [Header("Movement")]
    public AnimationCurve mousePanModifier;
    public float mouseSpeed;
    public float scrollSpeed;
    public float rotationSpeed;
    public float Screenborder;
    public bool moving = false;
    GridGen grid;
    float Mousetime;
    BoxCollider CamCollider;

    [Header("Keybord Movement")]
    public AnimationCurve panSpeedModifier;
    public float panSpeed;
    float time;

    [Header("Tree Opacity")]
    public List<GameObject> treeInVision;
    public List<MeshRenderer> treeMat;
    public Vector3 triggerCenter;
    public AnimationCurve opacityModifier;

    
    public enum Player { player0,player1,player2,player3,player4};
    public Player currentPlayer = Player.player0;
    Camera playerCamera;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        CameraAxisMovement();
        RotationsCaméra();
        AxisMouseMovement();
        TreeOpacity();
        if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            currentPlayer++;
            LayerAssignation();
        }
        if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            currentPlayer--;
            LayerAssignation();
        }
    }
    public void Initialize()
    {
        treeInVision = new List<GameObject>();
        cameraHolderRotation = GetComponentInChildren<Transform>();
        grid = FindObjectOfType<GridGen>();
        grid.farCorner.x += movementlimit;
        grid.farCorner.y += movementlimit;
        grid.originCorner.x -= movementlimit;
        grid.originCorner.y -= movementlimit;
        CamCollider = GetComponent<BoxCollider>();
        playerCamera = gameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<Camera>();
        if (currentPlayer != Player.player0)
        {
            LayerAssignation();
            
        }
        
    }
    public void LayerAssignation()
    {
        switch (currentPlayer)
        {
            case Player.player0:
                {
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 11);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 12);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 13);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 14);
                    break;
                }
            case Player.player1:
                {
                    //Show
                    playerCamera.cullingMask |= LayerMask.NameToLayer("player1");
                    //Hide
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 12);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 13);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 14);
                    break;
                }
            case Player.player2:
                {
                    //Show
                    playerCamera.cullingMask |= LayerMask.NameToLayer("player2");
                    //Hide
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 11);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 13);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 14);
                    break;
                }
            case Player.player3:
                {
                    //Show
                    playerCamera.cullingMask |= LayerMask.NameToLayer("player3");
                    //Hide
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 11);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 12);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 14);
                    break;
                }
            case Player.player4:
                {
                    //Show
                    playerCamera.cullingMask |= LayerMask.NameToLayer("player4");
                    //Hide
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 11);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 12);
                    playerCamera.cullingMask = playerCamera.cullingMask ^ (1 << 13);
                    break;
                }
        }
    }

    public void RotationsCaméra()
    {
    if (Input.GetKey(KeyCode.Mouse2))
    {
        mouveCam();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            cameraHolderRotation.Rotate(Vector3.forward * panSpeedModifier.Evaluate(time)*rotationSpeed);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            cameraHolderRotation.Rotate(Vector3.back * panSpeedModifier.Evaluate(time)*rotationSpeed);
        }
    }

    private void mouveCam()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -75f, 75f);

        
        cameraHolderRotation.Rotate(Vector3.forward * mouseX);
    }

    private void AxisMouseMovement()
    {
        Vector3 mouse = Input.mousePosition;
        Vector3 posMouse = transform.position;
        Mousetime += Time.deltaTime;
        if (mouse.y >= Screen.height - Screenborder)
        {
            moving = true;
            posMouse.y += mousePanModifier.Evaluate(Mousetime) * mouseSpeed;
        }
         if (mouse.y <= Screenborder)
        {
            moving = true;
            posMouse.y -= mousePanModifier.Evaluate(Mousetime) * mouseSpeed;
        }
         if (mouse.x <= Screenborder)
        {
            moving = true;
            posMouse.x -= mousePanModifier.Evaluate(Mousetime) * mouseSpeed;
        }
         if (mouse.x >= Screen.width - Screenborder)
        {
            moving = true;
            posMouse.x += mousePanModifier.Evaluate(Mousetime) * mouseSpeed;
        }
        if (mouse.x <= Screen.width - Screenborder && mouse.x >= Screenborder && mouse.y >= Screenborder && mouse.y <= Screen.height - Screenborder)
        {
            moving = false;
            Mousetime = 0;
        }
        /*
        else
        {
            moving = false;
            Mousetime = 0;
        }*/
        posMouse.x = Mathf.Clamp(posMouse.x, grid.originCorner.x, grid.farCorner.x);
        posMouse.y = Mathf.Clamp(posMouse.y, grid.originCorner.y, grid.farCorner.y);
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        posMouse.z += scroll * Time.deltaTime * scrollSpeed;
        transform.position = posMouse;
    }

    private void CameraAxisMovement()
    {
        Vector3 pos = transform.position;        
        time += Time.deltaTime;
       
        if (Input.GetKey(KeyCode.Z))
        {
            
            pos.y += panSpeedModifier.Evaluate(time)*panSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            
            pos.y -= panSpeedModifier.Evaluate(time)*panSpeed;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            
            pos.x -= panSpeedModifier.Evaluate(time)*panSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            
            pos.x += panSpeedModifier.Evaluate(time)*panSpeed;
        }
        if (!Input.anyKey)
        {
            
                time = 0;

        }
        pos.x = Mathf.Clamp(pos.x, grid.originCorner.x, grid.farCorner.x);
        pos.y = Mathf.Clamp(pos.y, grid.originCorner.y, grid.farCorner.y);
        transform.position = pos;
        
    }

    public void TreeOpacity()
    {
         triggerCenter = CamCollider.bounds.center;
        

        treeDist = Vector2.Distance(new Vector2(1, triggerCenter.y), new Vector2(1, tree.y));
        clampedTreeDist = Mathf.Lerp(0, 1, treeDist/2f); 
        
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
            treeInVision.Remove(other.gameObject);
            
            treeMat.Remove(other.GetComponent<MeshRenderer>());
            
        }
    }
}

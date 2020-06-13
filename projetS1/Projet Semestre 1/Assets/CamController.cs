using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public float movementlimit;
    private float xRotation = 0f;
    Transform cameraHolderRotation;
    private float mouseSensitivity = 100f;
    [Header("Mouse Movement")]
    public AnimationCurve mousePanModifier;
    public float mouseSpeed;
    public float scrollSpeed;
    public float rotationSpeed;
    public float Screenborder;
    public bool moving = false;
    GridGen grid;
    float Mousetime;
    
    
    [Header("Keybord Movement")]
    public AnimationCurve panSpeedModifier;
    public float panSpeed;
    float time;
    // Start is called before the first frame update
    void Start()
    {
        cameraHolderRotation = GetComponentInChildren<Transform>();
        grid = FindObjectOfType<GridGen>();
        GameObject orCorner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject farCorner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        grid.farCorner.x += movementlimit;
        grid.farCorner.y += movementlimit;
        grid.originCorner.x -= movementlimit;
        grid.originCorner.y -= movementlimit;
        orCorner.transform.SetPositionAndRotation(grid.originCorner,Quaternion.identity);
        farCorner.transform.SetPositionAndRotation(grid.farCorner, Quaternion.identity);

    }

    // Update is called once per frame
    void Update()
    {
        CameraAxisMovement();
        RotationsCaméra();
        AxisMouseMovement();
        
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

}

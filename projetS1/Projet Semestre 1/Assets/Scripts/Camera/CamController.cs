using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{

    [Header("Movement Variable")]
    [SerializeField]
    private float mouseSensitivity = 100f;
    private float xRotation = 0f;

    [Header("Game Object")]
    public GameObject playerPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playerPos.GetComponent<PlayerMouvement>().View.IsMine)
            mouveCam();
    }

    private void mouveCam()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -75f, 75f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerPos.transform.Rotate(Vector3.up * mouseX);
    }
}

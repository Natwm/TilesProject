using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool drawDebugRaycasts;

    [Space]

    [Header("Component")]
    private CharacterController characterController;
    public Camera cam;

    [Space]

    [Header("Environment Check Properties")]
    public Transform groundCheck;
    public LayerMask groundMask;
    [SerializeField] private float groundDistance;
    [SerializeField] private bool isGrounded;

    [Space]

    [Header("Movement Variable")]
    [SerializeField]
    private float speedMouvement = 10.0f;
    [SerializeField]
    private float gravity = -9.51f;

    Vector3 velocity;

    public Collider[] tes;
    public Vector3 offset;

    public GameObject tile;

    public PlayerMouvement onlineMethods;

    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        onlineMethods = GetComponent<PlayerMouvement>();
    }
    private void Update()
    {
        if(onlineMethods.View.IsMine)
            physicsCheck();
    }

    void FixedUpdate()
    {
        if (isGrounded && onlineMethods.View.IsMine)
        {
            Move();
            if(Interact() && Input.GetKeyDown(KeyCode.E))
            {
                onlineMethods.UseFov();
            }
        }
    }

    private void Move()
    {
        float moveVertical = Input.GetAxis("Vertical");
        float moveHorizontal = Input.GetAxis("Horizontal");

        Vector3 mouvement = (transform.right * moveHorizontal + transform.forward * moveVertical) * speedMouvement;
        characterController.Move(mouvement * Time.deltaTime);

        velocity.z -= gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    bool Interact()
    {
        if(tile != null)
        {
            tile.GetComponent<CellData>().ShowTile(gameObject.name);
            tile = null;
        }

        RaycastHit hit;
        Physics.Raycast(groundCheck.transform.position, Vector3.forward, out hit, groundDistance);
        Debug.LogWarning(hit.collider.gameObject == null);
        /*if (hit.collider.gameObject != null )
        {
            if(hit.collider.gameObject.GetComponent<CellData>() != null)
            {
                if (hit.collider.gameObject.GetComponent<CellData>().State == CellData.m_State.Show)
                {
                    tile = hit.collider.gameObject;
                    return hit.collider.gameObject.GetComponent<CellData>().HighlightTile();
                }
            }          
        }*/
        return false;
    }


    #region Physics
    private void physicsCheck()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.x < 0)
        {
            velocity.x = -1.5f;
        }
    }

    #endregion

    #region Getter && Setter

    #endregion
}

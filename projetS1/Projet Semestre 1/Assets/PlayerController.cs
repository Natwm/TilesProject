using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool drawDebugRaycasts;
    [Header("Status")]
    private bool state;


    [Space]

    [Header("Component")]
    private CharacterController characterController;
    public Camera cam;

    [Space]

    [Header("Environment Check Properties")]
    public Transform goundCheck;
    public LayerMask groundMask;
    public LayerMask interactMask;
    [SerializeField] private float groundDistance;
    [SerializeField] private bool isGrounded;
    [SerializeField] private float grabDistance;
    [SerializeField] private float eyeHeight = 1.5f;

    [Space]

    [Header("Movement Variable")]
    [SerializeField]
    private float speedMouvement = 10.0f;
    [SerializeField]
    private float gravity = -9.51f;

    GameObject interact;

    Vector3 velocity;



    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        physicsCheck();

    }

    void FixedUpdate()
    {
        if (state)
        {
            Move();
            
        }

    }

    private void Move()
    {
        float moveVertical = Input.GetAxis("Vertical");
        float moveHorizontal = Input.GetAxis("Horizontal");

        Vector3 mouvement = (transform.right * moveHorizontal + transform.forward * moveVertical) * speedMouvement;
        characterController.Move(mouvement * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }


    #region Physics
    private void physicsCheck()
    {
        isGrounded = Physics.CheckSphere(goundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1.5f;
        }
        RaycastHit hit;

        Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, grabDistance, interactMask);

        Color color = hit.collider != null ? Color.green : Color.red;
        Debug.DrawRay(cam.transform.position, cam.transform.forward * grabDistance, color);
        if (hit.collider != null)
        {
            Interact = hit.collider.gameObject;
        }
        else
        {
            Interact = null;
        }
    }

    #endregion

    #region Getter && Setter
    public bool State { get => state; set => state = value; }
    public GameObject Interact { get => interact; set => interact = value; }
    #endregion
}

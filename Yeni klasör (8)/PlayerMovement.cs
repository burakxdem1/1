using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float groundDrag = 5f;

    [Header("Limit Movement")]
    [SerializeField] private bool limitW;
    [SerializeField] private bool limitS;
    [SerializeField] private bool limitD;
    [SerializeField] private bool limitA;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float crouchYScale = 0.5f;
    private float startYScale;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;

    [Header("Slope Handler")]
    [SerializeField] private float maxAngle = 40;
    public RaycastHit slopeHit;

    [Header("Keybinds")]
    private KeyCode sprintKey = KeyCode.LeftShift;
    private KeyCode crouchKey = KeyCode.LeftControl;

    [SerializeField] private Transform orientation;

    private float horizontalInput;
    private float verticallInput;

    private Vector3 moveDir;

    private Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        crouching,
        sprinting
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        GroundCheck();
        WallCheck();

        MyInput();
        SpeedControl();
        StateHandler();

        GroundDragApply();

        Debug.Log(state);
        Debug.Log(WallCheck());
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void StateHandler()
    {
        // Mode - Crouching
        if (isGrounded && Input.GetKey(crouchKey)) 
        { 
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (isGrounded && Input.GetKey(sprintKey) && Input.GetAxisRaw("Vertical") > 0 && !WallCheck()) 
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        } 
        // Mode - Walking
        else if (isGrounded) 
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
    }
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticallInput = Input.GetAxisRaw("Vertical");

        // start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
        }

        if (Input.GetKey(crouchKey)) {
            // Yavaþça crouch boyutuna in
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(transform.localScale.y, crouchYScale, Time.deltaTime * 10f), transform.localScale.z);

        } 
        else {
            // Yavaþça eski boyuta dön
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(transform.localScale.y, startYScale, Time.deltaTime * 10f), transform.localScale.z);
        }
    }

    private void MovePlayer()
    {
        moveDir = orientation.forward * verticallInput + orientation.right * horizontalInput;

        // Engel kontrolü yap ve giriþleri kýsýtla
        if (WallCheck()) {
            if (limitW) // Ýleri engel
            {
                state = MovementState.walking;
                verticallInput = Mathf.Clamp(verticallInput, -1, 0); // W tuþunu engelle (ileri)
            }
            if (limitS) // Geri engel
            {
                state = MovementState.walking;
                verticallInput = Mathf.Clamp(verticallInput, 0, 1); // S tuþunu engelle (geri)
            }
            if (limitD) // Sað engel
            {
                state = MovementState.walking;
                horizontalInput = Mathf.Clamp(horizontalInput, -1, 0); // D tuþunu engelle (sað)
            }
            if (limitA) // Sol engel
            {
                state = MovementState.walking;
                horizontalInput = Mathf.Clamp(horizontalInput, 0, 1); // A tuþunu engelle (sol)
            }
        }

        if (OnSlope()) 
        {
            rb.AddForce(GetSlopeMoveDir() * moveSpeed * 20f, ForceMode.Force);

            // add small force to player to stand on ground
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        } 
        else 
        {
            rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // turn gravity while on slope
        rb.useGravity = !OnSlope();
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
    }

    private void GroundDragApply()
    {
        if (isGrounded) 
        {
            rb.linearDamping = groundDrag;
        } else 
        {
            rb.linearDamping = 5f;
        }
    }

    private void SpeedControl()
    {
        // limit speed on slope
        if (OnSlope()) 
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // limit speed on ground
        else 
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > moveSpeed) {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) 
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDir()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    public bool OnSlopeReturn()
    {
        return OnSlope();
    }

    public bool WallCheck()
{
    float rayDistance = 1.5f; // Iþýn mesafesi
    Vector3 origin = transform.position;

    // Ýleri yön (W)
    bool forwardRay = Physics.Raycast(origin, orientation.forward, rayDistance, whatIsGround);

    // Geri yön (S)
    bool backwardRay = Physics.Raycast(origin, -orientation.forward, rayDistance, whatIsGround);

    // Sað yön (D)
    bool rightRay = Physics.Raycast(origin, orientation.right, rayDistance, whatIsGround);

    // Sol yön (A)
    bool leftRay = Physics.Raycast(origin, -orientation.right, rayDistance, whatIsGround);

        limitW = forwardRay;
        limitS = backwardRay;
        limitD = rightRay;
        limitA = leftRay;

    // Debug için ýþýnlarý çizelim
    Debug.DrawRay(origin, orientation.forward * rayDistance, Color.red);  // Ýleri
    Debug.DrawRay(origin, -orientation.forward * rayDistance, Color.blue);  // Geri
    Debug.DrawRay(origin, orientation.right * rayDistance, Color.green);  // Sað
    Debug.DrawRay(origin, -orientation.right * rayDistance, Color.yellow);  // Sol

    // Eðer herhangi bir ýþýn bir engelle çarpýyorsa true döndür
    return forwardRay || backwardRay || rightRay || leftRay;

    }

    public float ReturnHorizontalInput()
    { 
        return horizontalInput; 
    }
    public float ReturnVerticalInput()
    { 
        return verticallInput; 
    }
}

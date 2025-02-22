using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensX = 400;
    [SerializeField] private float sensY = 400f;
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Camera playerCamera;

    [Header("FOV Settings")]
    [SerializeField] private float walkFov = 50f;
    [SerializeField] private float sprintFov = 70f;
    [SerializeField] private float crouchFov = 40f;
    private float currentFov;

    [SerializeField] private float cameraSpeed = 40f;

    [Header("Head Bobbing Settings")]
    [SerializeField] private float walkingBobbingSpeed = 12f; // Walking Sallanma miktarý
    [SerializeField] private float walkingBobbingAmount = 0.3f; // Walking Sallanma miktarý
    [SerializeField] private float sprintingBobbingSpeed = 18f; // Sprinting Sallanma miktarý
    [SerializeField] private float sprintingBobbingAmount = 0.7f; // Sprinting Sallanma miktarý
    [SerializeField] private float crouchingBobbingSpeed = 8f; // Sprinting Sallanma miktarý
    [SerializeField] private float crouchingBobbingAmount = 0.15f; // Sprinting Sallanma miktarý
    private float bobbingSpeed;
    private float bobbingAmount;
    private float bobTimer;
    private Vector3 startPosition;

    private float xRotation;
    private float yRotation;
    private float currentXRotation;
    private float currentYRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        startPosition = transform.localPosition;

        currentFov = playerCamera.fieldOfView;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleHeadBobbing();
        HandleFov();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Yumuþak geçiþ için Lerp kullan
        currentYRotation = Mathf.LerpAngle(currentYRotation, yRotation, Time.deltaTime * cameraSpeed);
        currentXRotation = Mathf.LerpAngle(currentXRotation, xRotation, Time.deltaTime * cameraSpeed);

        // Kameranýn dönüþünü uygula
        transform.rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);
        orientation.rotation = Quaternion.Euler(0, currentYRotation, 0);

    }

    private void HandleHeadBobbing()
    {
        bool isMoving = Mathf.Abs(playerMovement.ReturnHorizontalInput()) > 0.1f || Mathf.Abs(playerMovement.ReturnVerticalInput()) > 0.1f; float slopeAngle = 0f;

        if (playerMovement.WallCheck())
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition, Time.deltaTime * bobbingSpeed);
            bobTimer = 0; // Hareket sýfýrlandýðýnda bobbing sýfýrlansýn
            return;
        }

        if (playerMovement.OnSlopeReturn()) {
            slopeAngle = Vector3.Angle(Vector3.up, playerMovement.slopeHit.normal); // Eðimin açýsýný al
        }

        float slopeMultiplier = 1f + (slopeAngle / 40f); // 0° eðimde 1x, 40° eðimde 2x olacak
        float slopeBobSpeedMultiplier = Mathf.Lerp(1f, 0.2f, slopeAngle / 40f); // Eðim arttýkça bobbing speed yavaþlar

        if (playerMovement.state == PlayerMovement.MovementState.crouching) 
        {
            bobbingSpeed = crouchingBobbingSpeed;
            bobbingAmount = crouchingBobbingAmount;
        }
        else if (playerMovement.state == PlayerMovement.MovementState.walking) 
        {
            bobbingSpeed = walkingBobbingSpeed;
            bobbingAmount = walkingBobbingAmount;
        } 
        else if (playerMovement.state == PlayerMovement.MovementState.sprinting) 
        {
            bobbingSpeed = sprintingBobbingSpeed;
            bobbingAmount = sprintingBobbingAmount;
        }

        if (isMoving) {
            bobTimer += Time.deltaTime * bobbingSpeed * slopeBobSpeedMultiplier; // Eðim açýsýna baðlý yavaþlama

            float bobOffsetY = Mathf.Sin(bobTimer) * bobbingAmount;
            float bobOffsetX = Mathf.Cos(bobTimer / 2) * bobbingAmount * slopeMultiplier; // Eðim açýsýna göre artan sað-sol bobbing

            float tiltAngle = Mathf.Cos(bobTimer) * bobbingAmount * 5f; // Eðilme miktarý bobbing'e baðlý

            Vector3 targetPosition = startPosition + new Vector3(bobOffsetX, bobOffsetY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 5f);

            // Kameranýn eðilmesini uygula
            Quaternion targetRotation = Quaternion.Euler(xRotation, yRotation, tiltAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        } else {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition, Time.deltaTime * bobbingSpeed);
            bobTimer = Mathf.Lerp(bobTimer, 0, Time.deltaTime * 5f);

            // Eðilmeyi sýfýrla
            Quaternion targetRotation = Quaternion.Euler(xRotation, yRotation, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }
    private void HandleFov()
    {
        bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;

        // Sprint yapýyorsa FOV'yi artýr, deðilse normale döndür
        if (playerMovement.state == PlayerMovement.MovementState.crouching) 
        {
            currentFov = Mathf.Lerp(currentFov, crouchFov, Time.deltaTime * 5f); // Yavaþça sprint FOV'sine geç
        } 
        else if (playerMovement.state == PlayerMovement.MovementState.sprinting && isMoving) 
        {
            currentFov = Mathf.Lerp(currentFov, sprintFov, Time.deltaTime * 5f); // Yavaþça sprint FOV'sine geç

            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(xRotation + 5f, yRotation, 0), Time.deltaTime * 5f);
        } 
        else if (playerMovement.state == PlayerMovement.MovementState.walking || !isMoving) 
        {
            currentFov = Mathf.Lerp(currentFov, walkFov, Time.deltaTime * 5f); // Yavaþça yürüyüþ FOV'sine dön
        }

        playerCamera.fieldOfView = currentFov; // FOV'yi kameraya uygula
    }
}

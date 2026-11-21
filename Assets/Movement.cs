using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterController))]
public class FastWASDMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 10f;
    public float sprintMultiplier = 2f;

    [Header("Vertical Movement")]
    public bool enableVerticalMovement = true;
    public float verticalSpeed = 5f;

    [Header("Camera Relative")]
    public bool moveRelativeToCamera = true;
    public bool keepHorizontalOnly = true;

    [Header("Physics")]
    public float gravity = -9.81f;
    public bool pushObjects = true;
    public float pushPower = 2.0f;

    private CharacterController characterController;
    private XROrigin xrOrigin;
    private Transform cameraTransform;
    private float verticalVelocity = 0f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.linearVelocity = pushDir * walkSpeed * 0.5f * pushPower;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        xrOrigin = GetComponent<XROrigin>();

        if (xrOrigin != null && xrOrigin.Camera != null)
            cameraTransform = xrOrigin.Camera.transform;
        else
            cameraTransform = Camera.main?.transform;

        if (characterController == null)
            Debug.LogError("FastWASDMovement: Missing CharacterController on XR Origin!");
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float inputX = 0f;
        float inputZ = 0f;
        float inputY = 0f;

        if (keyboard.wKey.isPressed) inputZ += 1f;
        if (keyboard.sKey.isPressed) inputZ -= 1f;
        if (keyboard.aKey.isPressed) inputX -= 1f;
        if (keyboard.dKey.isPressed) inputX += 1f;

        if (enableVerticalMovement)
        {
            if (keyboard.qKey.isPressed) inputY -= 1f;
            if (keyboard.eKey.isPressed) inputY += 1f;
        }

        float speed = walkSpeed;
        if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
            speed *= sprintMultiplier;

        Vector3 moveDirection;

        if (moveRelativeToCamera && cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            if (keepHorizontalOnly)
            {
                forward.y = 0f;
                right.y = 0f;
            }

            forward.Normalize();
            right.Normalize();

            moveDirection = forward * inputZ + right * inputX;
        }
        else
        {
            moveDirection = new Vector3(inputX, 0f, inputZ);
        }

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        Vector3 horizontalMove = moveDirection * speed;
        float verticalMove = 0f;

        if (enableVerticalMovement && Mathf.Abs(inputY) > 0.01f)
        {
            verticalMove = inputY * verticalSpeed;
            verticalVelocity = 0f;
        }
        else
        {
            if (characterController.isGrounded)
                verticalVelocity = -1f;
            else
                verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalMove = horizontalMove;
        finalMove.y = verticalMove + verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    float runMultiplier = 3.0f;
    float crouchMultiplier = 0.5f;

    PlayerMovement playerMovement;
    CharacterController characterController;
    Animator animator;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 currentCrouchMovement;
    Vector3 cameraRelativeMovement;
    Vector3 appliedMovement;

    bool isMoving;
    bool isRunPressed;
    bool isWalking;
    bool isRunning;
    bool isJumping = false;
    bool isJumpPressed = false;
    bool isCrouchPressed;
    bool isDashing;
    bool isAttacking = false;
    bool isAttackPressed;

    int zero = 0;
    int dashAttempts;
    float rotationFactorPerFrame = 30.0f;
    float groundedGravity = -0.05f;
    float gravity = -9.8f;
    float initialJumpVelo;
    float maxJumpHeight = 1.0f;
    float maxJumpTime = 1.0f;
    float dashStartTime;
    float dashSpeed = 30f;

    private void Awake()
    {
        playerMovement = new PlayerMovement();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        playerMovement.CharacterControls.Movement.started += onMovementInput;
        playerMovement.CharacterControls.Movement.canceled += onMovementInput;
        playerMovement.CharacterControls.Movement.performed += onMovementInput;
        playerMovement.CharacterControls.Run.started += onRun;
        playerMovement.CharacterControls.Run.canceled += onRun;
        playerMovement.CharacterControls.Run.performed += onRun;
        playerMovement.CharacterControls.Jump.started += onJump;
        playerMovement.CharacterControls.Jump.canceled += onJump;
        playerMovement.CharacterControls.Jump.performed += onJump;
        playerMovement.CharacterControls.Crouch.started += onCrouch;
        playerMovement.CharacterControls.Crouch.canceled += onCrouch;
        playerMovement.CharacterControls.Crouch.performed += onCrouch;
        playerMovement.CharacterControls.Attack.started += onAttack;
        playerMovement.CharacterControls.Attack.canceled += onAttack;
        playerMovement.CharacterControls.Attack.performed += onAttack;

        setupJumpVariables();
    }
    void Start()
    {

    }

    void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelo = (2 * maxJumpHeight) / timeToApex;
    }

    void onAttack(InputAction.CallbackContext context)
    {
        isAttackPressed = context.ReadValueAsButton();
    }
    void handleAttack()
    {
        if (isAttackPressed)
        {
            isAttacking = true;
            animator.SetBool("isAttacking", true);
        }
        else
        {
            isAttacking = false;
            animator.SetBool("isAttacking", false);
        }
    }

    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    void handleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            isJumping = true;
            animator.SetBool("isJumping", true);
            currentMovement.y = initialJumpVelo;
            appliedMovement.y = initialJumpVelo;
        }
        else if (!isJumpPressed && characterController.isGrounded && isJumping)
        {
            isJumping = false;
        }
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }
    void onMovementInput (InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;
        currentCrouchMovement.x = currentMovementInput.x * crouchMultiplier;
        currentCrouchMovement.z = currentMovementInput.y * crouchMultiplier;
        isMoving = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void onCrouch(InputAction.CallbackContext context)
    {
        isCrouchPressed = context.ReadValueAsButton();
    }

    void handleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = cameraRelativeMovement.x;
        positionToLookAt.y = zero;
        positionToLookAt.z = cameraRelativeMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    void handleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 1.0f;

        if (characterController.isGrounded)
        {
            animator.SetBool("isFalling", false);
            animator.SetBool("isJumping", false);
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        }

        else if (isFalling)
        {
            if (transform.position.y >= 2.5f && isFalling)
            {
                animator.SetBool("isFalling", true);
                float previousYVelocity = currentMovement.y;
                currentMovement.y = currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
                appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * .5f, -20.0f);
            }
            else
            {
                float previousYVelocity = currentMovement.y;
                currentMovement.y = currentMovement.y + (gravity * fallMultiplier * Time.deltaTime);
                appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * .5f, -20.0f);
            }
        }

        else
        {
            float previousYvelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (gravity * Time.deltaTime);
            appliedMovement.y = (previousYvelocity + currentMovement.y) * .5f;
        }
    }
    void handleAnimation()
    {
        bool isWalking = animator.GetBool("isWalking");
        bool isRunning = animator.GetBool("isRunning");
        bool isCrouching = animator.GetBool("isCrouching");

        if (isMoving && !isWalking)
        {
            animator.SetBool("isWalking", true);
        }

        else if (!isMoving && isWalking)
        {
            animator.SetBool("isWalking", false);
        }

        if ((isMoving && isRunPressed) && !isRunning)
        {
            animator.SetBool("isRunning", true);
        }

        else if ((!isMoving || !isRunPressed) && isRunning)
        {
            animator.SetBool("isRunning", false);
        }

        if (((isMoving || !isMoving) && isCrouchPressed) && !isCrouching)
        {
            animator.SetBool("isCrouching", true);
        }

        else if (!isCrouchPressed && isCrouching)
        {
            animator.SetBool("isCrouching", false);
        }
        
        if ((isMoving && isCrouchPressed) && isWalking)
        {
            animator.SetBool("isCrouchWalking", true);
        }

        else if (((!isMoving || isMoving) && !isCrouchPressed) && isWalking)
        {
            animator.SetBool("isCrouchWalking", false);
        }

        else
        {
            animator.SetBool("isCrouchWalking", false);
        }

        if ((isRunPressed && isCrouchPressed) && isRunning)
        {
            animator.SetBool("isCrouchRunning", true);
        }

        else if (((!isRunPressed || !isCrouchPressed) && !isMoving) && isRunning)
        {
            animator.SetBool("isCrouchRunning", false);
        }

        else
        {
            animator.SetBool("isCrouchRunning", false);
        }
    }

    void Update()
    {
        handleRotation();
        handleAnimation();

        if (isRunPressed)
        {
            if (isCrouchPressed && isRunPressed)
            {
                appliedMovement.x = currentRunMovement.x / 2;
                appliedMovement.z = currentRunMovement.z / 2;
            }
            else
            {
                appliedMovement.x = currentRunMovement.x;
                appliedMovement.z = currentRunMovement.z;
            }
        }

        else if (isCrouchPressed)
        {
            appliedMovement.x = currentCrouchMovement.x;
            appliedMovement.z = currentCrouchMovement.z;
        }

        else
        {
            appliedMovement.x = currentMovement.x;
            appliedMovement.z = currentMovement.z;
        }

        cameraRelativeMovement = ConvertToCameraSpace(appliedMovement);
        characterController.Move(cameraRelativeMovement * Time.deltaTime);

        handleGravity();
        handleJump();
        handleAttack();
    }

    Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
    {
        float currentYValue = vectorToRotate.y;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraForwardZProd = vectorToRotate.z * cameraForward;
        Vector3 cameraRightXProd = vectorToRotate.x * cameraRight;

        Vector3 vectorRotatedToCameraSpace = cameraForwardZProd + cameraRightXProd;
        vectorRotatedToCameraSpace.y = currentYValue;
        return vectorRotatedToCameraSpace;
    }

    private void OnEnable()
    {
        playerMovement.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerMovement.CharacterControls.Disable();
    }
}

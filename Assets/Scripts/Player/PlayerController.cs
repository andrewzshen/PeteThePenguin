using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using MonsterLove.StateMachine;
using UnityEngine;

[RequireComponent(typeof(PlayerMover))]
public class PlayerController : MonoBehaviour {

    [SerializeField] private InputReader inputReader;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float airSpeed = 2.0f;
    [SerializeField] private float jumpSpeed = 10.0f;
    [SerializeField] private float jumpDuration = 0.2f;
    [SerializeField] private float gravity = 0.2f;
    [SerializeField] private float airFriction = 0.5f;
    [SerializeField] private float groundFriction = 100.0f;
    [SerializeField] private float fallSpeed = 10.0f;
    [SerializeField] private float slideSpeed = 5.0f;
    [SerializeField] private float maxSlope = 30.0f;

    private PlayerMover mover;

    private Transform cameraTransform;

    private enum States {
        Grounded,
        Falling,
        Sliding,
        Rising,
        Jumping
    }

    private StateMachine<States> stateMachine;

    private bool isJumping;
    private bool isSliding;
    private bool isSurfing;

    public Vector2 moveInput;

    private Vector3 velocity, inputVelocity, savedMovementVelocity;
    
    private void Awake() {
        mover = GetComponent<PlayerMover>();

        stateMachine = new StateMachine<States>(this);
        stateMachine.ChangeState(States.Falling);
    }

    private void Start() {
        inputReader.EnableGameplayActions();
    }

    private void OnEnable() {
        inputReader.MoveEvent += OnMove;
        inputReader.JumpEvent += OnJump;
        inputReader.SurfEvent += OnSurf;
    }

    private void OnDisable() {
        inputReader.MoveEvent -= OnMove;
        inputReader.JumpEvent -= OnJump;
        inputReader.SurfEvent -= OnSurf;
    }

    private void Update() {

    }

    private void FixedUpdate() {
        mover.GroundCheck();
        HandleMovement();
        mover.SetVelocity(velocity);
    }

    #region Grounded State

    private void Grounded_Enter() {
        Debug.Log("grounded enter");
    }
    
    private void Grounded_Update() {
        Debug.Log("grounded update");

        if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        } else if(mover.IsGrounded() && IsGroundTooSteep()) {
            stateMachine.ChangeState(States.Sliding);
        } else if(!mover.IsGrounded()) {
            stateMachine.ChangeState(States.Falling);
        } else if(false/* jump logic */) {
            stateMachine.ChangeState(States.Jumping);
        }
    }

    private void Grounded_FixedUpdate() {
        
    }

    private void Grounded_Exit() {
        Debug.Log("grounded exit");
    }

    #endregion

    #region Falling State

    private void Falling_Enter() {
        Debug.Log("grounded enter");
    }
    
    private void Falling_Update() {
        Debug.Log("grounded update");

        if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        }
    }

    private void Falling_Exit() {
        Debug.Log("grounded exit");
    }

    #endregion

    #region Sliding State

    private void Sliding_Enter() {
        Debug.Log("sliding enter");
    }
    
    private void Sliding_Update() {
        Debug.Log("Sliding State update");

        if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        }
    }

    private void Sliding_FixedUpdate() {
        Vector3 pointDown = Vector3.ProjectOnPlane(mover.GetGroundNormal(), transform.up).normalized;
        inputVelocity -= pointDown * Vector3.Dot(inputVelocity, pointDown);
        velocity += inputVelocity * Time.fixedDeltaTime;
    }

    private void Sliding_Exit() {
        Debug.Log("grounded exit");
    }

    #endregion

    #region Rising State

    private void Rising_Enter() {
        Debug.Log("grounded enter");
    }
    
    private void Rising_Update() {
        Debug.Log("grounded update");

        if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        }
    }

    private void Rising_Exit() {
        Debug.Log("grounded exit");
    }

    #endregion

    #region Jumping State

    private void Jumping_Enter() {
        Debug.Log("grounded enter");
    }
    
    private void Jumping_Update() {
        Debug.Log("grounded update");

        if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        }
    }

    private void Jumping_Exit() {
        Debug.Log("grounded exit");
    }

    #endregion

    #region Movement

    private void HandleMovement() {
        Vector3 verticalVelocity = transform.up.normalized * Vector3.Dot(velocity, transform.up.normalized);
        Vector3 horizontalVelocity = velocity - verticalVelocity;

        verticalVelocity -= transform.up * (gravity * Time.deltaTime);

        float friction = (stateMachine.State == States.Grounded) ? groundFriction : airFriction;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, friction * Time.fixedDeltaTime);

        velocity = verticalVelocity + horizontalVelocity;
    }

    private void HandleJumping() {
        
    }

    private void HandleSliding() {

    }

    #endregion

    private bool IsRising() => Vector3.Dot(velocity, transform.up) > 0.0f;
    private bool IsFalling() => Vector3.Dot(velocity, transform.up) < 0.0f;
    private bool IsGroundTooSteep() => !mover.IsGrounded() || Vector3.Angle(mover.GetGroundNormal(), transform.up) > maxSlope;

    private void OnGroundContactLost() {

    }

    private void OnGroundContactGained() {
        Vector3 inputVelocity = CalculateInputVelocity();
    }
        
    private Vector3 CalculateInputVelocity() { 
        return CalculateInputDirection() * moveSpeed;
    }

    private Vector3 CalculateInputDirection() {
        Vector3 direction = cameraTransform == null ?
                transform.right * moveInput.x + transform.forward * moveInput.y :
                Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized * moveInput.x +
                Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized * moveInput.y;
        return direction.normalized;
    }

    #region Event Listeners

    private void OnMove(Vector2 input) {
        moveInput = input;
    }

    private void OnJump(bool jump) {
        isJumping = jump;
    }

    private void OnSurf(bool surf) {
        isSurfing = surf;
    }
    

    #endregion
}

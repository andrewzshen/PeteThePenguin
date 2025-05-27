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
    [SerializeField] private float fallGravity = 0.2f;
    [SerializeField] private float slideGravity = 5.0f;
    [SerializeField] private float airFriction = 0.5f;
    [SerializeField] private float groundFriction = 100.0f;
    [SerializeField] private float fallSpeed = 10.0f;
    [SerializeField] private float slideSpeed = 5.0f;
    [SerializeField] private float maxSlope = 30.0f;

    [SerializeField] private Transform cameraTransform;

    private enum States {
        Grounded,
        Falling,
        Sliding,
        Rising,
        Jumping,
        Surfing
    }
    private StateMachine<States> stateMachine;

    private PlayerMover mover;

    private bool isJumping;
    private bool isSliding;
    private bool isSurfing;

    public Vector2 moveInput;

    private Vector3 velocity, inputVelocity, savedVelocity;
    
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
        stateMachine.Driver.Update.Invoke();
    }

    private void FixedUpdate() {
        stateMachine.Driver.FixedUpdate.Invoke();
        mover.GroundCheck();
        HandleMovement();
        velocity += CalculateInputVelocity();
        
        mover.SetUsingExtendedRaycasterRange(IsGrounded());
        mover.SetVelocity(velocity);
    }

    #region Grounded State

    private void Grounded_Enter() {
        Debug.Log("Grounded Enter");
    }
    
    private void Grounded_Update() {
        if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        } else if(IsFalling()) {
            stateMachine.ChangeState(States.Falling);
        } else if(mover.IsGrounded() && IsGroundTooSteep()) {
            stateMachine.ChangeState(States.Sliding);
        } else if(isJumping) {
            stateMachine.ChangeState(States.Jumping);
        }
    }

    private void Grounded_Exit() {
        Debug.Log("Grounded Exit");
    }

    #endregion

    #region Falling State

    private void Falling_Enter() {
        Debug.Log("Falling Enter");
    }
    
    private void Falling_Update() {
        if(mover.IsGrounded()) {
            stateMachine.ChangeState(IsGroundTooSteep() ? 
                States.Sliding :
                States.Grounded
            );
        } else if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        }
    }

    private void Falling_Exit() {
        Debug.Log("Falling Exit");
    }

    #endregion

    #region Sliding State

    private void Sliding_Enter() {
        Debug.Log("Sliding Enter");
    }
    
    private void Sliding_Update() {
        if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        } else if(IsFalling()) {
            stateMachine.ChangeState(States.Falling);
        } else if(mover.IsGrounded() && !IsGroundTooSteep()) {
            stateMachine.ChangeState(States.Grounded);
        }
    }

    private void Sliding_Exit() {
        Debug.Log("Sliding Exit");
    }

    #endregion

    #region Rising State

    private void Rising_Enter() {
        Debug.Log("Rising Enter");
    }
    
    private void Rising_Update() {
        if(mover.IsGrounded()) {
            stateMachine.ChangeState(IsGroundTooSteep() ? 
                States.Sliding :
                States.Grounded
            );
        } else if(IsFalling()) {
            stateMachine.ChangeState(States.Falling);
        }
    }

    private void Rising_Exit() {
        Debug.Log("Rising Exit");
    }

    #endregion

    #region Jumping State

    private void Jumping_Enter() {
        Debug.Log("Jumping Enter");
    }
    
    private void Jumping_Update() {
        if(IsRising()) {
            stateMachine.ChangeState(States.Rising);
        }
    }

    private void Jumping_Exit() {
        Debug.Log("Jumping Exit");
    }

    #endregion

    #region Surfing State

    #endregion

    #region Movement

    private void HandleMovement() {
        Vector3 verticalVelocity = ExtractDotVector(velocity, transform.up);
        Vector3 horizontalVelocity = velocity - verticalVelocity;

        verticalVelocity -= transform.up * (fallGravity * Time.deltaTime); // gravity shit
        if(stateMachine.State == States.Grounded) {
            verticalVelocity = Vector3.zero;
        }

        if(!IsGrounded()) {
            AdjustHorizontalMomentum();
        }

        if(stateMachine.State == States.Sliding) {
            HandleSliding();
        }
        
        float friction = (stateMachine.State == States.Grounded) ? groundFriction : airFriction;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, friction * Time.fixedDeltaTime);
        
        velocity = horizontalVelocity + verticalVelocity;

        if(stateMachine.State == States.Jumping) {
            HandleJumping();
        }
        
        if(stateMachine.State == States.Jumping) {
            velocity = Vector3.ProjectOnPlane(velocity, mover.GetGroundNormal());
            if(Vector3.Dot(velocity, transform.up) > 0.0f) {
                velocity -= transform.up * Vector3.Dot(velocity, transform.up);
            }
        
            Vector3 slideDirection = Vector3.ProjectOnPlane(-transform.up, mover.GetGroundNormal()).normalized;
            velocity += slideDirection * (slideGravity * Time.deltaTime);
        }
    }

    private void HandleJumping() {
        velocity -= transform.up * Vector3.Dot(velocity, transform.up);
        velocity += transform.up * jumpSpeed;
    }

    private void HandleSliding() {
        Vector3 pointDown = Vector3.ProjectOnPlane(mover.GetGroundNormal(), transform.up).normalized;
        inputVelocity -= pointDown * Vector3.Dot(inputVelocity, pointDown);
        velocity += inputVelocity * Time.fixedDeltaTime;
    }

    private void AdjustHorizontalMomentum() {
        
    }

    private void OnGroundContactLost() {
        
    }

    private void OnGroundContactGained() {
        // 
    }

    #endregion

    private bool IsGrounded() => stateMachine.State == States.Grounded || stateMachine.State == States.Sliding; 
    
    private bool IsRising() => Vector3.Dot(velocity, transform.up) > 0.0f;
    private bool IsFalling() => Vector3.Dot(velocity, transform.up) < 0.0f;
    private bool IsGroundTooSteep() => !mover.IsGrounded() || Vector3.Angle(mover.GetGroundNormal(), transform.up) > maxSlope;
        
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

    private Vector3 ExtractDotVector(Vector3 vector, Vector3 direction) {
        direction.Normalize();
        return direction * Vector3.Dot(vector, direction);
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

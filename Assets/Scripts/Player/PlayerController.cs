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

    private Vector3 velocity, savedVelocity, savedMovementVelocity;
    
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

    #region STATES

    private void Grounded_Enter() {
        Debug.Log("grounded enter");
    }
    
    private void Grounded_Update() {
        Debug.Log("grounded update");
    }

    private void GroundedExit() {
        Debug.Log("grounded exit");
    }

    #endregion

    #region MOVEMENT

    private void HandleMovement()
    {
        Vector3 verticalVelocity = transform.up.normalized * Vector3.Dot(velocity, transform.up.normalized);
        Vector3 horizontalVelocity = velocity - verticalVelocity;

        verticalVelocity -= transform.up * (gravity * Time.deltaTime);

        float friction = (stateMachine.State == States.Grounded) ? groundFriction : airFriction;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, friction * Time.deltaTime);

        velocity = verticalVelocity + horizontalVelocity;
    }

    private void HandleJumping() {

    }

    private void HandleSliding() {

    }

    #endregion

    private Vector3 CalculateMovementVelocity() { 
        return CalculateMovementDirection() * moveSpeed;
    }

    private Vector3 CalculateMovementDirection() {
        Vector3 direction = cameraTransform == null ?
                transform.right * moveInput.x + transform.forward * moveInput.y :
                Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized * moveInput.x +
                Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized * moveInput.y;
        return direction.normalized;
    }

    #region EVENT LISTENERS

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

using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerMover))]
public class PlayerController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private InputReader inputReader = default;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float airSpeed = 2.0f;
    [SerializeField] private float jumpSpeed = 10.0f;
    [SerializeField] private float jumpDuration = 0.2f;
    [SerializeField] private float airResistance = 0.5f;
    [SerializeField] private float groundFriction = 100.0f;
    [SerializeField] private float fallSpeed = 10.0f;
    [SerializeField] private float slideSpeed = 5.0f;
    [SerializeField] private float maxSlope = 30.0f;

    private bool useLocalMomentum;

    private PlayerMover mover;

    private Transform cameraTransform;

    private float currentSpeed;
    private float velocity;

    private bool isJumping;
    private bool isSliding;

    public Vector2 moveInput;

    private Vector3 momentum, savedVelocity, savedMovementVelocity;

    private void Awake() {
        mover = GetComponent<PlayerMover>();
    }

    private void Start() {
        inputReader.EnableGameplayActions();
    }

    private void OnEnable() {
        inputReader.MoveEvent += OnMove;
        inputReader.JumpEvent += OnJump;
        inputReader.SurfEvent += OnSlide;
    }

    private void OnDisable() {
        inputReader.MoveEvent -= OnMove;
        inputReader.JumpEvent -= OnJump;
        inputReader.SurfEvent -= OnSlide;
    }

    private void Update() {
        
    }

    private void FixedUpdate() {
        mover.GroundCheck();
        HandleMomentum();
    }

    private void HandleMomentum() {

    }

    private void HandleJumping() {

    }

    private void HandleSliding() {

    }

    #region EVENT LISTENERS

    private void OnMove(Vector2 input) {
        moveInput = input;
    }

    private void OnJump(bool jump) {
        isJumping = jump;
    }

    private void OnSlide() {
        isSliding = true;
    }

    #endregion
}

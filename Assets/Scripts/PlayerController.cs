using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    
    [Header("References")]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private InputReader inputReader;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 15.0f;
    [SerializeField] private float smoothTime = 0.2f;

    private Transform mainCam;

    private float currentSpeed;
    private float velocity;

    private bool isJumping;
    private bool isSliding;

    private Vector3 moveInput;

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
    }

    private void Start() {
        mainCam = Camera.main.transform;
    }

    private void OnEnable() {
        inputReader.moveEvent += OnMove;
        inputReader.jumpEvent += OnJump;
        inputReader.slideEvent += OnSlide;
    }

    private void OnDisable() {
        inputReader.moveEvent -= OnMove;
        inputReader.jumpEvent -= OnJump;
        inputReader.slideEvent -= OnSlide;
    }

    private void Update() {
        
    }

    private void FixedUpdate() {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement() {
        Vector3 adjustedDirection = new Vector3(moveInput.x, rigidBody.linearVelocity.y, moveInput.z);
        Debug.Log(adjustedDirection);

        if(adjustedDirection.magnitude > 0.0f) {
            HandleHorizontalMovement(adjustedDirection);
            HandleRotation(adjustedDirection);
            SmoothSpeed(adjustedDirection.magnitude);
        } else {
            SmoothSpeed(0.0f);
            rigidBody.linearVelocity = new Vector3(0.0f, rigidBody.linearVelocity.y, 0.0f);
        }
    }

    private void HandleHorizontalMovement(Vector3 direction) {
        Vector3 velocity = moveSpeed * Time.fixedDeltaTime * direction;
        rigidBody.linearVelocity = new Vector3(velocity.x, rigidBody.linearVelocity.y, velocity.z);
    }

    private void HandleRotation(Vector3 direction) {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleJump() {

    }

    private void SmoothSpeed(float speed) {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref velocity, smoothTime);
    }

    private void OnMove(Vector2 input) {
        moveInput = input;
    }

    private void OnJump() {
        isJumping = true;
    }

    private void OnSlide() {
        isSliding = true;
    }
}

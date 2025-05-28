using System;
using MonsterLove.StateMachine;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GroundRider : MonoBehaviour {

    [SerializeField] private InputReader inputReader;

    private enum States {
        Grounded,
        Falling,
        Sliding,
        Rising,
        Jumping,
        Surfing
    }
    private StateMachine<States> stateMachine;

    private Rigidbody rb;

    [Header("Riding")]
    [SerializeField] private float rayCheckLength;
    [SerializeField] private float rideHeight;
    [SerializeField] private float rideSpringStrength;
    [SerializeField] private float rideSpringDampFactor;
    [SerializeField] private float fallForce;

    [Header("Movement")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;
    [SerializeField] private AnimationCurve maxAccelerationFactorFromDot;

    [Header("Rotation")]
    [SerializeField] private float uprightRotationSpringStrength;
    [SerializeField] private float uprightRotationSpringDamper;

    [Header("Jumping")]
    
    private Vector2 moveInput;

    private Vector3 inputDirection;
    private Vector3 targetVelocity;

    private bool isJumping;
    private bool isSliding;
    private bool isSurfing;

    private void Awake() {
        inputReader.MoveEvent += OnMove;
        inputReader.JumpEvent += OnJump;
        inputReader.SurfEvent += OnSurf;

        rb = GetComponent<Rigidbody>();

        stateMachine = new StateMachine<States>(this);
        stateMachine.ChangeState(States.Falling);
    }

    private void OnEnable() {
        inputReader.EnableGameplayActions();
    }

    private void OnDisable() {
        inputReader.DisableGameplayActions();
    }

    private void OnDestroy() {
        inputReader.MoveEvent -= OnMove;
        inputReader.JumpEvent -= OnJump;
        inputReader.SurfEvent -= OnSurf;
    }

    private void FixedUpdate() {
        stateMachine.Driver.FixedUpdate.Invoke();
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement() {
        inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y);

        if(inputDirection.magnitude > 1.0f) {
            inputDirection.Normalize();
        }

        float dot = Vector3.Dot(inputDirection, targetVelocity.normalized);
        float modifiedAcceleration = acceleration * accelerationFactorFromDot.Evaluate(dot);
        Vector3 inputVelocity = maxSpeed * inputDirection;

        targetVelocity = Vector3.MoveTowards(targetVelocity, inputVelocity, modifiedAcceleration * Time.fixedDeltaTime);

        // Needed acceleration to reach target velocity in a single fixed update
        Vector3 neededAcceleration = (targetVelocity - rb.linearVelocity) / Time.fixedDeltaTime;
        float modifiedMaxAcceleration = maxAcceleration * maxAccelerationFactorFromDot.Evaluate(dot);
        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, modifiedMaxAcceleration);

        Vector3 force = Vector3.Scale(rb.mass * neededAcceleration, new Vector3(1.0f, 0.0f, 1.0f));
        rb.AddForce(force);
    }

    private void HandleRotation() {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
        Quaternion shortestRotation = targetRotation * Quaternion.Inverse(currentRotation);

        // If the dot is negative you gotta flip the shit
        if(Quaternion.Dot(targetRotation, currentRotation) < 0.0f) {
            shortestRotation = new Quaternion(
                -shortestRotation.x, 
                -shortestRotation.y, 
                -shortestRotation.z, 
                -shortestRotation.w
            );
        }

        shortestRotation.ToAngleAxis(out float rotationDegrees, out Vector3 rotationAxis);
        float rotationRadians = rotationDegrees * Mathf.Deg2Rad;
        rotationAxis.Normalize();

        Vector3 springForce = rotationRadians * (rotationAxis * uprightRotationSpringStrength);
        Vector3 dampingForce = rb.angularVelocity * uprightRotationSpringDamper;
        Vector3 torque = springForce - dampingForce;
        rb.AddTorque(torque, ForceMode.Force);
    }

    private void OnMove(Vector2 input) {
        moveInput = input;
    }

    private void OnJump(bool jump) {
        isJumping = jump;
    }

    private void OnSurf(bool surf) {
        isSurfing = surf;
    }
}
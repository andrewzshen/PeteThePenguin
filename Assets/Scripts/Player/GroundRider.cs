using MonsterLove.StateMachine;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GroundRider : MonoBehaviour {

    private Rigidbody rb;
    private Vector3 forceFromGravity;
    private Vector3 inputDirection;
    private Vector2 moveInput;

    [SerializeField] private InputReader inputReader;

    [Header("Ride Height Spring")]
    [SerializeField] private float rayCheckLength;
    [SerializeField] private float rideHeight;
    [SerializeField] private float rideHeightSpringStrength;
    [SerializeField] private float rideHeightSpringDampingFactor;
    [SerializeField] private float fallForce;

    [Header("Upright Spring")]
    [SerializeField] private float uprightRotationSpringStrength;
    [SerializeField] private float uprightRotationSpringDampingFactor;
    private Quaternion lastTargetRotation;

    [Header("Movement")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;
    [SerializeField] private AnimationCurve maxAccelerationFactorFromDot;

    private Vector3 targetVelocity = Vector3.zero;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    
    private enum States {
        Grounded,
        Falling,
        Sliding,
        Rising,
        Jumping,
        Surfing
    }
    private StateMachine<States> stateMachine;

    private Raycaster raycaster;

    private bool isJumping;
    private bool isSliding;
    private bool isSurfing;

    private void Awake() {
        inputReader.MoveEvent += OnMove;
        inputReader.JumpEvent += OnJump;
        inputReader.SurfEvent += OnSurf;

        rb = GetComponent<Rigidbody>();
        forceFromGravity = Physics.gravity * rb.mass;

        stateMachine = new StateMachine<States>(this);
        stateMachine.ChangeState(States.Grounded);
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
        MaintainUprightRotation();
        MaintainRideHeight();

        rb.AddForce(Physics.gravity, ForceMode.Acceleration);
    }

    private void Grounded_FixedUpdate() {

    }

    private void HandleMovement() {
        inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

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

        Vector3 movementForce = Vector3.Scale(rb.mass * neededAcceleration, new Vector3(1f, 0f, 1f));
        rb.AddForce(movementForce);
    }

    private void MaintainRideHeight() {
        bool nearGround = NearGround(out RaycastHit hitInfo);

        if(nearGround) {
            float distanceFromRideHeight = rideHeight - hitInfo.distance;
            float downVelocity = Vector3.Dot(Vector3.down, rb.linearVelocity);

            float springForce = -distanceFromRideHeight * rideHeightSpringStrength;
            float dampingForce = downVelocity * rideHeightSpringDampingFactor;
            Vector3 rideForce = (springForce - dampingForce) * Vector3.down;

            rb.AddForce(rideForce);

            if(hitInfo.rigidbody != null) {
                hitInfo.rigidbody.AddForceAtPosition(-rideForce, hitInfo.point);
            }
        }
    }

    private void MaintainUprightRotation() {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation;

        if(inputDirection.magnitude > 0.05f) {
            targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            lastTargetRotation = targetRotation;
        } else {
            float lastYaw = lastTargetRotation.eulerAngles.y;
            targetRotation = Quaternion.Euler(new Vector3(0f, lastYaw, 0f));
        }

        Quaternion shortestRotation = targetRotation * Quaternion.Inverse(currentRotation);

        if(Quaternion.Dot(targetRotation, currentRotation) < 0f) {
            shortestRotation = new Quaternion(-shortestRotation.x, -shortestRotation.y, -shortestRotation.z, -shortestRotation.w);
        }

        shortestRotation.ToAngleAxis(out float rotationDegrees, out Vector3 rotationAxis);
        float rotationRadians = rotationDegrees * Mathf.Deg2Rad;
        rotationAxis.Normalize();

        Vector3 springForce = rotationAxis * (rotationRadians * uprightRotationSpringStrength);
        Vector3 dampingForce = rb.angularVelocity * uprightRotationSpringDampingFactor;
        Vector3 uprightTorque = springForce - dampingForce;
        rb.AddTorque(uprightTorque);
    }

    private bool NearGround(out RaycastHit rayHit) => Physics.Raycast(transform.position, Vector3.down, out rayHit, rayCheckLength);

    private void OnMove(Vector2 input) => moveInput = input;
    private void OnJump(bool jump) => isJumping = jump;
    private void OnSurf(bool surf) => isSurfing = surf;
}
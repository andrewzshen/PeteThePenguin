using MonsterLove.StateMachine;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GroundRider : MonoBehaviour {

    [SerializeField] private InputReader inputReader;

    [Header("Height Spring")]
    [SerializeField] private float rayCheckLength;
    [SerializeField] private float rideHeight;
    [SerializeField] private float rideSpringStrength;
    [SerializeField] private float rideSpringDampingFactor;
    [SerializeField] private float fallForce;

    [Header("Upright String")]
    [SerializeField] private float uprightRotationSpringStrength;
    [SerializeField] private float uprightRotationSpringDampingFactor;

    private Quaternion lastTargetRotation;

    [Header("Movement")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;
    [SerializeField] private AnimationCurve maxAccelerationFactorFromDot;
    
    private Vector2 moveInput;

    private Vector3 inputDirection;
    private Vector3 targetVelocity;

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

    private Rigidbody rb;

    private Raycaster raycaster;

    private bool isJumping;
    private bool isSliding;
    private bool isSurfing;

    private void Awake() {
        inputReader.MoveEvent += OnMove;
        inputReader.JumpEvent += OnJump;
        inputReader.SurfEvent += OnSurf;

        rb = GetComponent<Rigidbody>();

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
        MaintainUpright();
    }

    private void Grounded_FixedUpdate() {
        MaintainHeight();
    }

    private void MaintainHeight() {
        bool nearGround = NearGround(out RaycastHit hitInfo);

        if(nearGround) {
            float distanceFromRideHeight = rideHeight - hitInfo.distance;
            float downVelocity = Vector3.Dot(Vector3.down, rb.linearVelocity);

            float springForce = -distanceFromRideHeight * rideSpringStrength;
            float dampingForce = downVelocity * rideSpringDampingFactor;
            Vector3 force = (springForce - dampingForce) * Vector3.down;

            rb.AddForce(force);

            if(hitInfo.rigidbody != null) {
                hitInfo.rigidbody.AddForceAtPosition(-force, hitInfo.point);
            }
        }
    }

    private void HandleMovement() {
        inputDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

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

        Vector3 force = Vector3.Scale(rb.mass * neededAcceleration, new Vector3(1f, 0f, 1f));
        rb.AddForce(force);
    }

    private void MaintainUpright() {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation;

        if(inputDirection.magnitude > 0.05f) {
            // We only care about the horizontal component:
            Vector3 flatLook = new Vector3(inputDirection.x, 0f, inputDirection.y).normalized;
            targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);

            // Save the “last yaw” so we can hold it when inputDirection → 0
            lastTargetRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        } else {
            // 2b) No input → just keep whatever yaw we had (no pitch/roll)
            targetRotation = lastTargetRotation;
        }

        Quaternion shortestRotation = CalculateShortestRotation(targetRotation, currentRotation);

        shortestRotation.ToAngleAxis(out float rotationDegrees, out Vector3 rotationAxis);
        float rotationRadians = rotationDegrees * Mathf.Deg2Rad;
        rotationAxis.Normalize();

        Vector3 springForce = rotationAxis * (rotationRadians * uprightRotationSpringStrength);
        Vector3 dampingForce = rb.angularVelocity * uprightRotationSpringDampingFactor;
        Vector3 torque = springForce - dampingForce;
        rb.AddTorque(torque);
    }

    private Quaternion CalculateTargetRotation() {
        return Quaternion.identity;
    }

    private Quaternion CalculateShortestRotation(Quaternion targetRotation, Quaternion currentRotation) {
        if(Quaternion.Dot(targetRotation, currentRotation) < 0f) {
            return targetRotation * Quaternion.Inverse(QuatMultiply(currentRotation, -1f));
        } else {
            return targetRotation * Quaternion.Inverse(currentRotation);
        }
    }

    private Quaternion QuatMultiply(Quaternion quat, float scalar) {
        return new Quaternion(quat.x * scalar, quat.y * scalar, quat.z * scalar, quat.w * scalar);
    }

    private bool NearGround(out RaycastHit rayHit) => Physics.Raycast(transform.position, Vector3.down, out rayHit, rayCheckLength);

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
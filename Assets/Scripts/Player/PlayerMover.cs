using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMover : MonoBehaviour {
    [Header("Collider Settings")]
    [Range(0.0f, 1.0f)] [SerializeField] private float stepHeightRatio = 1.0f;
    [SerializeField] private float colliderHeight = 2.0f;
    [SerializeField] private float colliderThickness = 2.0f;
    [SerializeField] private Vector3 colliderOffset = Vector3.zero;

    [SerializeField] private bool inDebugMode;

    private Rigidbody rb;
    private CapsuleCollider col;

    private bool isGrounded;
    private float baseRaycasterRange;
    private Vector3 currentGroundAdjustmentVelocity;
    private int currentLayerMask;

    private Raycaster raycaster;
    private bool isUsingExtendedRaycasterRange = true;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;

        col = GetComponent<CapsuleCollider>();

        CalculateColliderDimensions();
    }

    private void OnValidate() {
        if(gameObject.activeInHierarchy) {
            CalculateColliderDimensions();
        }
    }

    public void GroundCheck() {
        if(currentLayerMask != gameObject.layer) {
            CalculateRaycasterLayerMask();
        }

        currentGroundAdjustmentVelocity = Vector3.zero;
        raycaster.RayLength = isUsingExtendedRaycasterRange ? 
            baseRaycasterRange + colliderHeight * transform.localScale.x * stepHeightRatio :
            baseRaycasterRange;
        
        isGrounded = raycaster.HasDetectedHit();
        if(!isGrounded) {
            return;
        }

        float distance = raycaster.GetHitDistance();
        float upperLimit = colliderHeight * transform.localScale.x * (1.0f - stepHeightRatio) * 0.5f;
        float middle = upperLimit + colliderHeight * transform.localScale.x * stepHeightRatio;
        float distanceToGo = middle - distance;

        currentGroundAdjustmentVelocity = transform.up * (distanceToGo / Time.fixedDeltaTime);
    }

    private void CalculateColliderDimensions() {
        if(col == null) {
            col = GetComponent<CapsuleCollider>();
        }

        col.height = colliderHeight * (1.0f - stepHeightRatio);
        col.radius = colliderThickness * 0.5f;
        col.center = colliderOffset * colliderHeight + new Vector3(0.0f, stepHeightRatio * col.height * 0.5f, 0.0f);

        if(col.height * 0.5f < col.radius) {
            col.radius = col.height * 0.5f;
        }

        CalibrateRaycaster();
    }

    private void CalibrateRaycaster() {
        if(raycaster == null) {
            raycaster = new Raycaster(transform);
        }

        raycaster.SetRayOrigin(col.bounds.center);
        raycaster.SetRayDirection(Raycaster.RayDirection.DOWN);

        CalculateRaycasterLayerMask();

        float safetyDistanceFactor = 0.001f;

        float rayLength = colliderHeight * (1.0f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
        baseRaycasterRange = rayLength * (1.0f + safetyDistanceFactor) * transform.localScale.x;
        raycaster.RayLength = rayLength * transform.localScale.x;
    }

    private void CalculateRaycasterLayerMask() {
        int objectLayer = gameObject.layer;
        int layerMask = Physics.AllLayers;

        for(int i = 0; i < 32; i++) {
            if(Physics.GetIgnoreLayerCollision(objectLayer, i)) {
                layerMask &= ~(1 << i);
            }
        }

        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        layerMask &= ~(1 << ignoreRaycastLayer);

        raycaster.LayerMask = layerMask;
        currentLayerMask = objectLayer;
    }

    public bool IsGrounded() => isGrounded;
    public Vector3 GetGroundNormal() => raycaster.GetHitNormal();

    public void SetVelocity(Vector3 velocity) => rb.linearVelocity = velocity + currentGroundAdjustmentVelocity;
    public void SetUsingExtendedRaycasterRange(bool useExtended) => isUsingExtendedRaycasterRange = useExtended;
}

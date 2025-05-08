using UnityEngine;

public class Raycaster {
    private Vector3 origin; 
    private Transform transform; 

    public float RayLength { get; set; } = 1.0f;
    public LayerMask LayerMask { get; set; } = 255;

    public enum RayDirection { 
        FORWARD,
        BACKWARD,
        LEFT, 
        RIGHT, 
        UP, 
        DOWN
    }

    private RayDirection direction;

    private RaycastHit hitInfo;

    public Raycaster(Transform tr) {
        transform = tr; 
    }

    public void CastRay() {
        Vector3 worldOrigin = transform.TransformPoint(origin);
        Vector3 worldDirection = GetRayDirection();

        Physics.Raycast(worldOrigin, worldDirection, out hitInfo, RayLength, LayerMask, QueryTriggerInteraction.Ignore);
    }

    public void SetRayOrigin(Vector3 position) => origin = transform.InverseTransformPoint(position);

    private Vector3 GetRayDirection() {
        return direction switch {
            RayDirection.FORWARD => transform.forward,
            RayDirection.BACKWARD => -transform.forward,
            RayDirection.LEFT => -transform.right,
            RayDirection.RIGHT => transform.right,
            RayDirection.UP => transform.up,
            RayDirection.DOWN => -transform.up,
            _ => Vector3.one
        };
    }

    public void SetRayDirection(RayDirection dir) => direction = dir;

    public bool HasDetectedHit() => hitInfo.collider != null;
    public float GetHitDistance() => hitInfo.distance;
    public Vector3 GetHitNormal() => hitInfo.normal;
    public Vector3 GetHitPosition() => hitInfo.point;
    public Collider GetHitCollider() => hitInfo.collider;
    public Transform GetHitTransform() => hitInfo.transform;
}

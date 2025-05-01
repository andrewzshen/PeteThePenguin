using UnityEngine;

public class Raycaster {
    private float rayLength = 1.0f;
    private LayerMask layerMask = 255;

    private Vector3 origin;
    private Transform transform; 

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

    public Raycaster(Transform transform) {
        this.transform = transform; 
    }

    public void CastRay() {
        Vector3 worldOrigin = transform.TransformPoint(origin);
        Vector3 worldDirection = GetRayDirection();

        Physics.Raycast(worldOrigin, worldDirection, out hitInfo, rayLength, layerMask, QueryTriggerInteraction.Ignore);
    }

    public float RayLength {
        get { return rayLength; }
        set { rayLength = value; }
    }

    public LayerMask LayerMask {
        get { return layerMask; }
        set { layerMask = value; }
    }

    public void SetLayerMask(LayerMask mask) {
        layerMask = mask;
    }

    public void SetRayOrigin(Vector3 position) {
        origin = transform.InverseTransformPoint(position);
    }

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

    public void SetRayDirection(RayDirection direction) {
        this.direction = direction;
    }

    public bool HasDetectedHit() {
        return hitInfo.collider != null;
    }

    public float GetHitDistance() {
        return hitInfo.distance;
    }

    public Vector3 GetHitNormal() {
        return hitInfo.normal;
    }

    public Vector3 GetHitPosition() {
        return hitInfo.point;
    }

    public Collider GetHitCollider() {
        return hitInfo.collider;
    } 

    public Transform GetHitTransform() {
        return hitInfo.transform;
    }
}

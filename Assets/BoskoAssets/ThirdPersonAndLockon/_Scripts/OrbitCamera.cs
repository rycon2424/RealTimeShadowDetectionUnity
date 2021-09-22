using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform viewTarget;
    public CamState currentState = CamState.onPlayer;
    public enum CamState { onPlayer, onTarget }
    [Space]
    public LayerMask rayShootLayers;
    public LayerMask collisionLayers;
    public float distance = 6.0f;
    public float distanceSpeed = 150.0f;
    public float collisionOffset = 0.3f;
    public float minDistance = 4.0f;
    public float maxDistance = 12.0f;
    public float height = 1.5f;
    public float horizontalRotationSpeed = 250.0f;
    public float verticalRotationSpeed = 150.0f;
    public float rotationDampening = 0.75f;
    public float minVerticalAngle = -60.0f;
    public float maxVerticalAngle = 60.0f;
    public bool canChangeDistance = true;
    public bool hideMouse;
    [Space]
    public Vector3 camLockOnPos = new Vector3(0.8f, 1.8f, -3.7f);
    [Space]
    private Transform enemyTarget;
    private float h, v, newDistance, smoothDistance;
    private Vector3 newPosition;
    private Quaternion newRotation, smoothRotation;
    private Transform cameraTransform;
    [SerializeField] private Transform camTarget;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        SetEulerAngles();

        cameraTransform = this.transform;
        camTarget = cameraTransform.parent;
        smoothDistance = distance;
        
        if (hideMouse)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        NullErrorCheck();
    }

    public void SetEulerAngles()
    {
        h = this.transform.eulerAngles.x;
        v = this.transform.eulerAngles.y;
    }

    void NullErrorCheck()
    {
        if (!viewTarget)
        {
            Debug.LogError("Please make sure to assign a view target!");
            Debug.Break();
        }
        if (collisionLayers == 0)
        {
            Debug.LogWarning("Make sure to set the collision layers to the layers the camera should collide with!");
        }
    }

    void LateUpdate()
    {
        if (!viewTarget)
            return;

        switch (currentState)
        {
            case CamState.onPlayer:
                NoTarget();
                break;
            case CamState.onTarget:
                Target();
                break;
            default:
                break;
        }
    }

    public void ReceiveEnemy(Transform target)
    {
        enemyTarget = target;
        camTarget.localPosition = camLockOnPos;
        transform.localPosition = Vector3.zero;
        ChangeCamState(CamState.onTarget);
    }

    void Target()
    {
        h = transform.rotation.eulerAngles.y;
        v = 10;

        CalculateRotation();

        var lookPos = enemyTarget.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
    }

    void NoTarget()
    {
        h += Input.GetAxis("Mouse X") * horizontalRotationSpeed * Time.deltaTime;
        v -= Input.GetAxis("Mouse Y") * verticalRotationSpeed * Time.deltaTime;

        CalculateRotation();

        cameraTransform.position = newPosition;
        cameraTransform.rotation = smoothRotation;
    }

    void CalculateRotation()
    {
        h = ClampAngle(h, -360.0f, 360.0f);
        v = ClampAngle(v, minVerticalAngle, maxVerticalAngle);

        newRotation = Quaternion.Euler(v, h, 0.0f);

        if (canChangeDistance)
        {
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 10, minDistance, maxDistance);
        }
        smoothDistance = Mathf.Lerp(smoothDistance, distance, TimeSignature(distanceSpeed));

        smoothRotation = Quaternion.Slerp(smoothRotation, newRotation, TimeSignature((1 / rotationDampening) * 100.0f));

        newPosition = viewTarget.position;
        newPosition += smoothRotation * new Vector3(0.0f, height, -smoothDistance);

        CheckSphere();

        smoothRotation.eulerAngles = new Vector3(smoothRotation.eulerAngles.x, smoothRotation.eulerAngles.y, 0.0f);
    }

    public void ChangeCamState(OrbitCamera.CamState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case CamState.onPlayer:
                break;
            case CamState.onTarget:
                break;
            default:
                break;
        }
    }

    public RayInfo ShootRayFromCam(PlayerBehaviour pb)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        RayInfo ri = new RayInfo();
        if (Physics.Raycast(ray, out hit, pb.teleportRange, rayShootLayers))
        {
            ri.hit = true;
            ri.rayHitPoint = hit.point;
            ri.hitNormal = hit.normal;
        }
        else
        {
            ri.hit = false;
        }
        return ri;
    }
    
    void CheckSphere()
    {
        Vector3 tmpVect = viewTarget.position;
        tmpVect.y += height;

        RaycastHit hit;

        Vector3 dir = (newPosition - tmpVect).normalized;

        if (Physics.SphereCast(tmpVect, 0.3f, dir, out hit, distance, collisionLayers))
        {
            newPosition = hit.point + (hit.normal * collisionOffset);
        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;

        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    private float TimeSignature(float speed)
    {
        return 1.0f / (1.0f + 80.0f * Mathf.Exp(-speed * 0.02f));
    }
}

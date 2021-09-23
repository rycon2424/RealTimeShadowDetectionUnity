using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerBehaviour : MonoBehaviour
{
    State currentState;
    public string currentStateDebug;

    [Header("ShadowInfo")]
    public bool inShadow;
    public bool teleporting;
    public int teleportRange = 30;
    [Space]
    public GameObject teleportParticles;
    public GameObject indicator;
    public PostProcessProfile postProcProfile;
    public Color outShadowM;
    public Color inShadowM;
    public Material defaultMaterial;

    [Header("RaycastInfo")]
    public LayerMask everything;
    public LayerMask climbing;
    public Vector3 lastCachedhit;
    public float distanceFromWall;
    public float grabHeight;

    [Header("GroundedInfo")]
    public bool grounded;
    public bool ccGrounded;
    [Space]
    public bool mountingWall;

    [Header("IK")]
    public bool useIK;
    public Vector3 leftHandPos;
    public Vector3 rightHandPos;

    #region public hidden
    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public OrbitCamera orbitCam;
    [HideInInspector] public LockOnLookat lockOnFocus;
    [HideInInspector] public TargetingSystem targetingSystem;
    [HideInInspector] public Animator anim;
    [HideInInspector] public bool lockedOn;
    [HideInInspector] public PlayerInputHandler playerControls;
    [HideInInspector] public StateMachine stateMachine;

    [HideInInspector] public MonoBehaviour mono;
    #endregion

    void Start()
    {
        stateMachine = new StateMachine();
        playerControls = new PlayerInputHandler();

        mono = this;

        characterController = GetComponent<CharacterController>();
        targetingSystem = GetComponent<TargetingSystem>();
        anim = GetComponent<Animator>();

        orbitCam = GetComponentInChildren<OrbitCamera>();
        lockOnFocus = GetComponentInChildren<LockOnLookat>();

        lockOnFocus.gameObject.SetActive(false);

        SetupCommands();
        SetupStateMachine();
    }

    void SetupCommands()
    {
        playerControls.BindInputToCommand(KeyCode.E, new GrabLedgeCommand(), KeyCommand.KeyType.OnKey);
        playerControls.BindInputToCommand(KeyCode.Space, new JumpCommand(), KeyCommand.KeyType.OnKeyDown);
        playerControls.BindInputToCommand(KeyCode.C, new CrouchCommand(), KeyCommand.KeyType.OnKeyDown);
        playerControls.BindInputToCommand(KeyCode.LeftShift, new SprintCommand(), KeyCommand.KeyType.OnKeyDown);
    }

    void SetupStateMachine()
    {
        Locomotion lm = new Locomotion();
        InAir ia = new InAir();
        Climbing cl = new Climbing();
        Crouch cr = new Crouch();

        stateMachine.AddState(lm);
        stateMachine.AddState(ia);
        stateMachine.AddState(cl);
        stateMachine.AddState(cr);

        stateMachine.GoToState(this, "Locomotion");
    }

    void Update()
    {
        ShadowSystem();
        ccGrounded = characterController.isGrounded;
        currentStateDebug = currentState.GetType().ToString();
        currentState.StateUpdate(this);
        playerControls.HandleInput(this);
        Targeting();
        anim.SetBool("Land", grounded);
    }

    void Targeting()
    {
        if (lockedOn)
        {
            RotateTowardsCamera();
            if (Input.mouseScrollDelta.y != 0)
            {
                targetingSystem.SwitchTarget(orbitCam);
                lockOnFocus.target = targetingSystem.currentTarget;
            }
            if (Vector3.Distance(transform.position, targetingSystem.currentTarget.transform.position) > targetingSystem.loseTargetRange)
            {
                LoseTarget();
            }
        }
    }
    
    public void LoseTarget()
    {
        lockedOn = false;
        anim.SetBool("Target", false);
        orbitCam.ChangeCamState(OrbitCamera.CamState.onPlayer);
        targetingSystem.currentTarget = null;
        lockOnFocus.gameObject.SetActive(false);
    }

    public void ChangeState(State newState)
    {
        currentState = newState;
    }

    public void RotateTowardsCamera()
    {
        Quaternion newLookAt = Quaternion.LookRotation(targetingSystem.currentTarget.transform.position - transform.position);
        newLookAt.x = 0;
        newLookAt.z = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, newLookAt, Time.deltaTime * 5);
    }

    [Header("Ground Raycast")]
    public float startHeight = 0.5f;
    public float range = 1;
    public float airtime;

    public bool Grounded()
    {
        if (RayHit(transform.position + (transform.right * 0.2f) + (transform.up * startHeight), (Vector3.down), range, everything)
            || RayHit(transform.position + (-transform.right * 0.2f) + (transform.up * startHeight), (Vector3.down), range, everything)
            || RayHit(transform.position + (transform.forward * 0.2f) + (transform.up * startHeight), (Vector3.down), range, everything)
            || RayHit(transform.position + (-transform.forward * 0.2f) + (transform.up * startHeight), (Vector3.down), range, everything)
            || RayHit(transform.position + (transform.up * startHeight), (Vector3.down), range, everything))
        {
            grounded = true;
            if (airtime != 0)
            {
                //Debug.Log("Total airtime was " + airtime.ToString("F2"));
            }
            airtime = 0;
            return true;
        }
        airtime += Time.deltaTime;
        grounded = false;
        return false;
    }

    public bool RayHit(Vector3 start, Vector3 dir, float length, LayerMask lm)
    {
        RaycastHit hit;
        Ray ray = new Ray(start, dir);
        Debug.DrawRay(start, dir * length, Color.magenta, 0.1f);
        if (Physics.Raycast(ray, out hit, length, lm))
        {
            lastCachedhit = hit.point;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void LerpToPosition(Vector3 pos, float lerpTime)
    {
        Debug.DrawLine(transform.position, pos, Color.red, 5);
        StartCoroutine(LerpToPos(pos, lerpTime));
    }

    IEnumerator LerpToPos(Vector3 pos, float lerpTime)
    {
        anim.applyRootMotion = false;
        Vector3 startPos = transform.position;

        for (float t = 0; t < 1; t += Time.deltaTime / lerpTime)
        {
            transform.position = Vector3.Lerp(startPos, pos, t);
            yield return new WaitForEndOfFrame();
        }

        teleportParticles.SetActive(false);
        teleporting = false;
        anim.applyRootMotion = true;
    }

    public bool PlayerToWall(PlayerBehaviour pb, Vector3 dir, bool lerp, float checkYOffset)
    {
        RaycastHit hit;
        float range = 2;
        Vector3 playerHeight = new Vector3(pb.transform.position.x, pb.transform.position.y + checkYOffset, pb.transform.position.z);
        Debug.DrawRay(playerHeight, dir * range, Color.green);
        if (Physics.Raycast(playerHeight, dir, out hit, range))
        {
            Vector3 temp = pb.transform.position - hit.point;
            temp.y = 0;
            Vector3 positionToSend = pb.transform.position - temp;
            positionToSend -= (pb.transform.forward * distanceFromWall);
            if (lerp)
            {
                pb.LerpToPosition(positionToSend, 0.25f);
            }
            else
            {
                transform.position = positionToSend;
            }
            return true;
        }
        return false;
    }

    public bool PlayerFaceWall(PlayerBehaviour pb, Vector3 startOffset, Vector3 dir, float range)
    {
        RaycastHit hit;
        Vector3 playerHeight = pb.transform.position;
        playerHeight += startOffset;
        Debug.DrawRay(playerHeight, dir * range, Color.cyan, 5);
        if (Physics.Raycast(playerHeight, dir, out hit, range))
        {
            pb.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            return true;
        }
        return false;
    }

    public bool LedgeInfo()
    {
        if (!RayHit(transform.position + (transform.up * 1.7f), transform.forward, 0.45f, climbing))
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position + (transform.forward * 0.45f) + (transform.up * 1.7f), Vector3.down);
            Debug.DrawRay(transform.position + (transform.forward * 0.45f) + (transform.up * 1.7f), Vector3.down * 0.35f, Color.red, 0.25f);
            if (Physics.Raycast(ray, out hit, 0.35f, climbing))
            {
                lastCachedhit = hit.point;
                string tagObject = hit.collider.tag;
                switch (tagObject)
                {
                    case "Ledge":
                        if (stateMachine.IsInState("InAir"))
                        {
                            anim.Play("AirToClimb");
                        }
                        stateMachine.GoToState(this, "Climbing");
                        break;
                    case "Example":
                        break;
                    default:
                        break;
                }
                return true;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator LerpCharacterControllerCenter(float to, float lerpTime)
    {
        float timeElapsed = 0;
        float beginFloat = characterController.center.y;
        while (timeElapsed < lerpTime)
        {
            beginFloat = Mathf.Lerp(beginFloat, to, timeElapsed / 0.25f);
            Vector3 temp = new Vector3(0, beginFloat, 0);
            characterController.center = temp;
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Vector3 finalY = new Vector3(0, to, 0);
        characterController.center = finalY;
    }

    public void DelayFunction(string functionName, float delay)
    {
        Invoke(functionName, delay);
    }

    void DelayedRoot()
    {
        anim.applyRootMotion = true;
    }

    public void _GoToState(string stateName)
    {
        stateMachine.GoToState(this, stateName);
    }

    public void _DoubleJump(float targetFloat)
    {
        StartCoroutine(LerpCharacterControllerCenter(targetFloat, 0.2f));
    }

    public void AddRotation(Vector3 rot)
    {
        transform.Rotate(rot);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (useIK)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos);

            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos);
        }
    }

    float turnSmoothVelocity;
    public void RotateToCam()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x, 0f, y).normalized;

        if (dir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + orbitCam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

    }

    public void ShadowDash()
    {
        if (teleporting == false)
        {
            RayInfo temp = Teleport();
            if (temp.hit)
            {
                if (Input.GetMouseButton(0))
                {
                    indicator.SetActive(false);
                    teleportParticles.SetActive(true);
                    teleporting = true;
                    LerpToPosition(temp.rayHitPoint, 0.25f);
                }
            }
        }
    }
    
    public RayInfo Teleport()
    {
        RayInfo PotentialTeleport = new RayInfo();
        if (inShadow)
        {
            if (Input.GetMouseButton(1))
            {
                RayInfo hitFromCamera = orbitCam.ShootRayFromCam(this);
                if (hitFromCamera.hit)
                {
                    RayInfo hitFromSun = ShadowDetector.instance.GetRayInfo(hitFromCamera.rayHitPoint, Vector3.zero);
                    if (hitFromSun.hit)
                    {
                        if (Vector3.Distance(hitFromCamera.rayHitPoint, hitFromSun.rayHitPoint) > 0.05f)
                        {
                            indicator.SetActive(true);
                            indicator.transform.position = hitFromCamera.rayHitPoint;
                            indicator.transform.rotation = Quaternion.LookRotation(-hitFromCamera.hitNormal, Vector3.up);
                            indicator.transform.Rotate(-90, 0, 0);

                            Debug.DrawLine(orbitCam.transform.position, hitFromCamera.rayHitPoint, Color.green);

                            PotentialTeleport.hit = true;
                            PotentialTeleport.rayHitPoint = hitFromCamera.rayHitPoint;
                            return PotentialTeleport;
                        }
                    }
                }
            }
        }
        indicator.SetActive(false);
        return PotentialTeleport;
    }
    
    void ShadowSystem()
    {
        if (teleporting == false)
        {
            if (inShadow == true)
            {
                if (ShadowDetector.instance.ShootRayFromLightPlayer(transform.position) == true)
                {
                    StopCoroutine("CycleMaterial");
                    StartCoroutine("CycleMaterial");
                    StartCoroutine(LerpFloatPostProcessing(0.3f, 0, 0.5f));
                }
            }
            if (inShadow == false)
            {
                if (ShadowDetector.instance.ShootRayFromLightPlayer(transform.position) == false)
                {
                    StopCoroutine("CycleMaterial");
                    StartCoroutine("CycleMaterial");
                    StartCoroutine(LerpFloatPostProcessing(0, 0.3f, 0.5f));
                }
            }
        }
    }

    IEnumerator LerpFloatPostProcessing(float start, float end, float cycleTime)
    {
        float currentTime = 0;
        while (currentTime < cycleTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / cycleTime;
            float currentFloat = Mathf.Lerp(start, end, t);
            postProcProfile.GetSetting<Vignette>().intensity.value = currentFloat;
            yield return null;
        }
    }

    IEnumerator CycleMaterial()
    {
        yield return new WaitForFixedUpdate();

        Color startColor = defaultMaterial.color;
        Color endColor = Color.white;

        float cycleTime = 0.5f;

        if (inShadow)
        {
            endColor = outShadowM;
        }
        else
        {
            endColor = inShadowM;
        }
        inShadow = !inShadow;

        float currentTime = 0;
        while (currentTime < cycleTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / cycleTime;
            Color currentColor = Color.Lerp(startColor, endColor, t);
            defaultMaterial.color = currentColor;
            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowDetector : MonoBehaviour
{
    public Transform sun;
    [Space]
    public float laserRay = 1000;
    public Vector3 playerOffset;

    public static ShadowDetector instance;

    private void Awake()
    {
        if (instance != null)
        {
            instance = null;
        }
        instance = this;
    }

    public RayInfo GetRayInfo(Vector3 target, Vector3 offset)
    {
        RayInfo ri = new RayInfo();
        RaycastHit hit;
        Ray ray = new Ray((target + offset) + (sun.transform.forward * -100), sun.transform.forward);
        if (Physics.Raycast(ray, out hit, laserRay))
        {
            Debug.DrawLine((target + offset) + (sun.transform.forward * -100), hit.point, Color.red);
            ri.hit = true;
            ri.rayHitPoint = hit.point;
        }
        else
        {
            ri.hit = false;
            Debug.DrawRay((target + offset) + (sun.transform.forward * -100), sun.transform.forward * laserRay, Color.green);
        }
        return ri;
    }

    public bool ShootRayFromLightPlayer(Vector3 target)
    {
        RaycastHit hit;
        Ray ray = new Ray((target + playerOffset) + (sun.transform.forward * -100), sun.transform.forward);
        if (Physics.Raycast(ray, out hit, laserRay))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.DrawLine((target + playerOffset) + (sun.transform.forward * -100), hit.point, Color.green);
                return true;
            }
            else
            {
                Debug.DrawLine((target + playerOffset) + (sun.transform.forward * -100), hit.point, Color.red);
                return false;
            }
        }
        else
        {
            Debug.DrawRay((target + playerOffset) + (sun.transform.forward * -100), sun.transform.forward * laserRay, Color.green);
            return false;
        }
    }

}

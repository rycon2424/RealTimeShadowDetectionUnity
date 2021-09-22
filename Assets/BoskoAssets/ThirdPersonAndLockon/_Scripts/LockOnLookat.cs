using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnLookat : MonoBehaviour
{
    public Transform cam;
    public Target target;

    void Update()
    {
        transform.LookAt(cam);
        if (target != null)
        {
            transform.position = target.transform.position + (Vector3.up * target.heightOffset);
        }
    }
}

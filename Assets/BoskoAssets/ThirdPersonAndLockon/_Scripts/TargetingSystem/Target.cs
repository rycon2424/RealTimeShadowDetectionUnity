using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float heightOffset;
    TargetingSystem pt;

    void Awake()
    {
        pt = FindObjectOfType<TargetingSystem>();
        pt.targets.Add(this);
    }
}

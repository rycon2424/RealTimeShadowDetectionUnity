using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    [Header("Assign")]
    public float targetingRange = 5;
    public float loseTargetRange = 6;
    [Space]
    public List<Target> targets = new List<Target>();
    public Target currentTarget;

    public bool SelectTarget(OrbitCamera oc)
    {
        if (targets.Count < 0)
        {
            return false;
        }
        
        currentTarget = null;
        
        currentTarget = ClosestTarget(targets);
        
        if (currentTarget != null)
        {
            oc.ReceiveEnemy(currentTarget.transform);
            return true;
        }
        return false;
    }

    public void SwitchTarget(OrbitCamera oc)
    {
        List<Target> potentialNextTargets = new List<Target>();
        potentialNextTargets.AddRange(targets);
        potentialNextTargets.Remove(currentTarget);

        Target newTarget = ClosestTarget(potentialNextTargets);
        
        foreach (Target t in potentialNextTargets)
        {
            Debug.Log(t.name);
        }

        if (newTarget != null)
        {
            currentTarget = newTarget;
            oc.ReceiveEnemy(currentTarget.transform);
        }
    }

    Target ClosestTarget(List<Target> targetsInView)
    {
        float dot = -1.5f;
        Target closestTarget = null;

        foreach (Target target in targetsInView)
        {
            if (Vector3.Distance(transform.position, target.transform.position) <= targetingRange)
            {
                Vector3 localPoint = Camera.main.transform.InverseTransformPoint(target.transform.position).normalized;
                Vector3 forward = Vector3.forward;
                float distanceFromCenter = Vector3.Dot(localPoint, forward);
                if (distanceFromCenter > dot)
                {
                    dot = distanceFromCenter;
                    closestTarget = target;
                }
            }
        }
        if (closestTarget != null)
        {
            return closestTarget;
        }
        return null;
    }
}

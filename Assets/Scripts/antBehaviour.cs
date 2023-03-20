using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class antBehaviour : MonoBehaviour
{
    public float speed = 0.5f;

    private List<GameObject> targets = new List<GameObject>();
    private List<GameObject> visitedTargets = new List<GameObject>();
    private GameObject currentTarget = null;
    private GameObject lastTarget = null;
    private bool hasFood;

    void Start()
    {
        hasFood = false;

        // Get all targets
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("target");
        GameObject[] colony = GameObject.FindGameObjectsWithTag("colony");
        GameObject[] food = GameObject.FindGameObjectsWithTag("food");

        foreach (GameObject target in allTargets)
        {
            targets.Add(target);
        }
        foreach (GameObject target in colony)
        {
            targets.Add(target);
        }
        foreach (GameObject target in food)
        {
            targets.Add(target);
        }

        // Find the closest target to the ant's starting position
        float minDistance = float.MaxValue;
        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                currentTarget = target;
            }
        }

        findNewTarget();
    }

    void Update()
    {
        if (currentTarget != null)
        {
            transform.LookAt(currentTarget.transform.position);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // Check if the ant has reached the current target
            if (Vector3.Distance(transform.position, currentTarget.transform.position) < 0.1f)
            {
                visitTarget();
                findNewTarget();
            }
        } else
        {
            findNewTarget();
        }
    }

    void findNewTarget()
    {
        lastTarget = currentTarget;

        // Find a new target
        List<GameObject> weightedTargets = new List<GameObject>();
        float totalWeight = 0f;
        foreach (GameObject target in targets)
        {
            if (visitedTargets.Contains(target) || target.GetComponent<targetWeight>().getDeadEnd())
            {
                continue; // Skip targets that have already been visited
            }
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance <= 1.30f && CanSeeTarget(target))
            {
                float weight = target.GetComponent<targetWeight>().GetPheromoneLevel();
                weight = Mathf.Clamp(weight, 0f, 1f); // Ensure weight is between 0 and 1
                weight = 1 - weight; // Invert the weight so that higher pheromone levels are preferred
                weightedTargets.Add(target);
                totalWeight += weight;
            }
        }
        if (weightedTargets.Count > 0)
        {
            float randomValue = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            foreach (GameObject target in weightedTargets)
            {
                float weight = target.GetComponent<targetWeight>().GetPheromoneLevel();
                weight = Mathf.Clamp(weight, 0f, 1f); // Ensure weight is between 0 and 1
                weight = 1 - weight; // Invert the weight so that higher pheromone levels are preferred
                cumulativeWeight += weight;
                if (cumulativeWeight >= randomValue)
                {
                    currentTarget = target;
                    break;
                }
            }
        } else if (lastTarget != null && CanSeeTarget(lastTarget))
        {
            currentTarget.GetComponent<targetWeight>().foundDeadEnd();
            currentTarget = lastTarget;
            visitedTargets.Clear();
        }

        // Mark the current target as visited
        visitedTargets.Add(currentTarget);
    }

    bool CanSeeTarget(GameObject target)
    {
        Vector3 targetDirection = target.transform.position - currentTarget.transform.position;
        if (Physics.Raycast(currentTarget.transform.position, targetDirection, out RaycastHit hit))
        {
            if (hit.collider.gameObject != target)
            {
                return false;
            }
        }

        return true;
    }

    void visitTarget()
    {
        // Call visitPheromones() on the current target
        targetWeight targetWeightScript = currentTarget.GetComponent<targetWeight>();

        if (currentTarget.CompareTag("food"))
        {
            hasFood = true;
            visitedTargets.Clear();
        }
        else if (currentTarget.CompareTag("colony"))
        {
            hasFood = false;
            visitedTargets.Clear();
        }

        if (targetWeightScript != null)
        {
            if (hasFood)
            {
                targetWeightScript.foodPheromones();
            }
            else 
            {
                targetWeightScript.visitPheromones();
            }
        }
    }
}
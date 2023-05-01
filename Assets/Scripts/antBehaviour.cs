using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class antBehaviour : MonoBehaviour
{
    public float speed = 0.5f;

    private List<GameObject> targets = new List<GameObject>();
    private List<GameObject> visitedTargets = new List<GameObject>();
    private List<GameObject> returnPath = new List<GameObject>();

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
        returnPath.Add(currentTarget);
        findNewTarget();
    }

    void Update()
    {
        if (currentTarget != null)
        {
            transform.LookAt(currentTarget.transform.position);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // Check if the ant has reached the current target
            if (Vector3.Distance(transform.position, currentTarget.transform.position) < 0.2f)
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
        if (hasFood)
        {
            findReturnTarget();
        }
        else
        {
            // Find a new target
            List<GameObject> weightedTargets = new List<GameObject>();
            float totalWeight = 0f;
            foreach (GameObject target in targets)
            {
                if (visitedTargets.Contains(target) || target.GetComponent<targetWeight>().getDeadEnd())
                {
                    continue; // Skip targets that have already been visited and that are dead ends
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
            }
            else if (lastTarget != null)
            {
                if (currentTarget.CompareTag("target"))
                {
                    currentTarget.GetComponent<targetWeight>().foundDeadEnd();
                }
                if (lastTarget.GetComponent<targetWeight>().getDeadEnd() == false)
                {
                    currentTarget = lastTarget;
                }
                else
                {
                    foreach (GameObject target in targets)
                    {
                        float distance = Vector3.Distance(transform.position, target.transform.position);
                        if (distance <= 1.30f && CanSeeTarget(target))
                        {
                            weightedTargets.Add(target);
                        }
                    }
                    int randomTarget = Random.Range(0, weightedTargets.Count);
                    currentTarget = weightedTargets[randomTarget];
                }
                visitedTargets.Clear();
            }
        }
        // Mark the current target as visited
        visitedTargets.Add(currentTarget);
    }

    void findReturnTarget()
    {
        if (returnPath.Last().GetComponent<targetWeight>().getDeadEnd() == false)
        {
            currentTarget = returnPath.Last();
            returnPath.RemoveAt(returnPath.Count-1);
        } else 
        {
            returnPath.RemoveAt(returnPath.Count-1);
            findReturnTarget();
        }
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
        if (hasFood == false)
        {
            returnPath.Add(currentTarget);
        }

        targetWeight targetWeightScript = currentTarget.GetComponent<targetWeight>();

        if (currentTarget.CompareTag("food"))
        {
            hasFood = true;
            targetWeightScript.foundFoodSource();
            visitedTargets.Clear();
        }
        else if (currentTarget.CompareTag("colony") && hasFood)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            TextMeshProUGUI scoreText = canvas.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
            score myScore = scoreText.GetComponent<score>();
            myScore.addToCounter();
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
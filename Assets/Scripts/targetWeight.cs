using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetWeight : MonoBehaviour
{
    private float pheromoneStrength = 1.0f; // strength of pheromones to deposit
    private float pheromoneDecay = 0.1f; // rate of decay for pheromones
    private float pheromoneLevel = 0.0f; // current level of pheromones
    private bool deadEnd;
    private Renderer targetRenderer;

    void Start()
    {
        targetRenderer = gameObject.GetComponent<Renderer>();
        deadEnd = false;
        InvokeRepeating("PheromoneDecay", 0.5f, 0.5f); // Call PheromoneDecay every 0.5 seconds
    }

    public bool getDeadEnd()
    { return deadEnd; }
    
    public void foundDeadEnd()
    { 
        deadEnd = true;
        targetRenderer.material.color = Color.red;
    }

    // function to deposit pheromones on the target
    public void visitPheromones()
    {
        pheromoneLevel += pheromoneStrength;
    }

    public void foodPheromones()
    {
        pheromoneLevel += 10*pheromoneStrength;
    }

    // function to get the current pheromone level
    public float GetPheromoneLevel()
    {
        return pheromoneLevel;
    }

    // function to update the pheromone level based on decay rate
    private void PheromoneDecay()
    {
        if (pheromoneLevel > 0)
        {
            pheromoneLevel = pheromoneLevel - pheromoneDecay;
        } else
        {
            pheromoneLevel= 0;
        }
        if (gameObject.CompareTag("target") && deadEnd==false)
        {
            if (pheromoneLevel < 20)
            {
                targetRenderer.material.color = Color.white;
            }
            else if (pheromoneLevel < 50)
            {
                targetRenderer.material.color = Color.yellow;
            }
            else if (pheromoneLevel < 75)
            {
                Color orange = new Color(1f, 0.55f, 0f);
                targetRenderer.material.color = orange;
            }
            else
            {
                targetRenderer.material.color = Color.green;
            }
        }

    }

}

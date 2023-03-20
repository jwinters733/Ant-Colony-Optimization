using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeSpawn : MonoBehaviour
{
    [SerializeField] NodeScript nodePrefab;
    [SerializeField] GameObject antPrefab;
    [SerializeField] GameObject targetPointPrefab;
    [SerializeField] Vector2Int mazeSize;
    [SerializeField] int numAnts;
    private float targetOffset = -.25f;

    List<NodeScript> nodes = new List<NodeScript>();

    private void Start()
    {
        GenerateMaze(mazeSize);
        spawnAnts(numAnts, mazeSize);
        spawnTargets(mazeSize);
    }

    void GenerateMaze(Vector2Int size)
    {
        nodes.Clear();

        for(int x=0; x<size.x; x++)
        {
            for(int y=0; y<size.y; y++)
            {
                Vector3 nodePos = new Vector3(x - (size.x / 2f), 0, y - (size.y / 2f));
                NodeScript newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                nodes.Add(newNode);
            }
        }

        List<NodeScript> currentPath = new List<NodeScript>();
        List<NodeScript> completedNodes = new List<NodeScript>();

        currentPath.Add(nodes[Random.Range(0, nodes.Count)]);
        currentPath[0].SetState(NodeState.Current);

        while(completedNodes.Count < nodes.Count)
        {
            List<int> possibleNextNodes = new List<int>();
            List<int> possibleDirections = new List<int>();

            int currentNodeIndex = nodes.IndexOf(currentPath[currentPath.Count - 1]);
            int currentNodeX = currentNodeIndex / size.y;
            int currentNodeY = currentNodeIndex % size.y;

            if(currentNodeX < size.x-1)
            {
                if(!completedNodes.Contains(nodes[currentNodeIndex + size.y]) &&
                    !currentPath.Contains(nodes[currentNodeIndex + size.y]))
                {
                    possibleDirections.Add(1);
                    possibleNextNodes.Add(currentNodeIndex + size.y);
                }
            }
            if (currentNodeX > 0)
            {
                // Check node to the left of the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex - size.y]) &&
                    !currentPath.Contains(nodes[currentNodeIndex - size.y]))
                {
                    possibleDirections.Add(2);
                    possibleNextNodes.Add(currentNodeIndex - size.y);
                }
            }
            if (currentNodeY < size.y - 1)
            {
                // Check node above the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex + 1]) &&
                    !currentPath.Contains(nodes[currentNodeIndex + 1]))
                {
                    possibleDirections.Add(3);
                    possibleNextNodes.Add(currentNodeIndex + 1);
                }
            }
            if (currentNodeY > 0)
            {
                // Check node below the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex - 1]) &&
                    !currentPath.Contains(nodes[currentNodeIndex - 1]))
                {
                    possibleDirections.Add(4);
                    possibleNextNodes.Add(currentNodeIndex - 1);
                }
            }

             if(possibleDirections.Count> 0)
            {
                int chosenDirection = Random.Range(0, possibleDirections.Count);
                NodeScript chosenNode = nodes[possibleNextNodes[chosenDirection]];

                switch (possibleDirections[chosenDirection])
                {
                    case 1:
                        chosenNode.RemoveWall(1);
                        currentPath[currentPath.Count - 1].RemoveWall(0);
                        break;
                    case 2:
                        chosenNode.RemoveWall(0);
                        currentPath[currentPath.Count - 1].RemoveWall(1);
                        break;
                    case 3:
                        chosenNode.RemoveWall(3);
                        currentPath[currentPath.Count - 1].RemoveWall(2);
                        break;
                    case 4:
                        chosenNode.RemoveWall(2);
                        currentPath[currentPath.Count - 1].RemoveWall(3);
                        break;
                }

                currentPath.Add(chosenNode);
                chosenNode.SetState(NodeState.Current);
            }
            else
            {
                completedNodes.Add(currentPath[currentPath.Count - 1]);
                currentPath[currentPath.Count - 1].SetState(NodeState.Completed);
                currentPath.RemoveAt(currentPath.Count - 1);
            }
        }
    }

    void spawnAnts(int numAnts, Vector2Int size)
    {
        for (int i = 0; i < numAnts; i++)
        {
            Vector3 antPos = new Vector3(Random.Range(0 - (size.x / 2f), 0 - (size.x / 2f)+0.5f), 0, Random.Range(0 - (size.y / 2f), 0 - (size.y / 2f) + 0.5f));
            Instantiate(antPrefab, antPos, Quaternion.identity, transform);
        }
    }

    void spawnTargets(Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 targetPos = new Vector3(x - (size.x / 2f), targetOffset, y - (size.y / 2f));

                // Spawn the target point
                GameObject newTarget = Instantiate(targetPointPrefab, targetPos, Quaternion.identity, transform);

                // Set the target point as a child of the corresponding node
                NodeScript correspondingNode = nodes[x * size.y + y];
                newTarget.transform.SetParent(correspondingNode.transform);

                if(x==0 && y==0)
                {
                    newTarget.gameObject.tag = "colony";
                    MeshRenderer meshRenderer = newTarget.GetComponent<MeshRenderer>();
                    meshRenderer.material.color = Color.black;
                    newTarget.transform.localScale *= 2f;
                }

                if (x == size.x-1 && y == size.y-1)
                {
                    newTarget.gameObject.tag = "food";
                    MeshRenderer meshRenderer = newTarget.GetComponent<MeshRenderer>();
                    meshRenderer.material.color = Color.green;
                    newTarget.transform.localScale *= 1.5f;
                }
            }
        }
    }
}

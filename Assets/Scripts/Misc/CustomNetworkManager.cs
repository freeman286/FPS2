using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public class CustomNetworkManager : NetworkManager
{

    [SerializeField]
    private LayerMask mask = -1;

    [SerializeField]
    private float spawnDistance = 15f;

    [SerializeField]
    private float spawnRange = 30f;

    [SerializeField]
    private float lineOfSightRange = 30f;

    public override Transform GetStartPosition()
    {
        Health[] enemies = GameManager.GetAllHealth();

        if (enemies.Length == 1)
            return startPositions[Random.Range(0, startPositions.Count)];
        
        List<Transform> usableStartPositions = new List<Transform>(startPositions);

        foreach (Health _enemy in enemies)
        {
            if (_enemy == null)
                break;

            usableStartPositions.RemoveAll(t => Mathf.Abs(Vector3.Distance(t.position, _enemy.transform.position) - spawnDistance) > spawnRange);
        }

        usableStartPositions = LineOfSight(usableStartPositions, enemies);

        if (usableStartPositions.Count > 0)
            return usableStartPositions[Random.Range(0, usableStartPositions.Count)];


        usableStartPositions = new List<Transform>(startPositions);
        usableStartPositions = LineOfSight(usableStartPositions, enemies);

        if (usableStartPositions.Count > 0)
            return usableStartPositions[Random.Range(0, usableStartPositions.Count)];


        return startPositions[Random.Range(0, startPositions.Count)];
    }

    private List<Transform> LineOfSight(List<Transform> positions, Health[] enemies)
    {
        List<Transform> usablePositions = new List<Transform>(positions);

        foreach (Transform startPos in usablePositions.ToList())
        {
            foreach (Health _enemy in enemies)
            {
                RaycastHit _hit;
                if (Physics.Raycast(startPos.position, _enemy.transform.position - startPos.position, out _hit, spawnDistance + spawnRange, mask))
                {
                    if (_hit.collider.transform.root == _enemy.transform)
                    {
                        usablePositions.Remove(startPos);
                    }
                }
            }
        }

        return usablePositions;
    }

    public override void OnStartClient() {
        //Stuff that happens when the client connects
        //LevelLoader.instance.DoTransition();
    }

    public override void OnStartHost() {
        //Stuff that happens when the host starts
    }
}


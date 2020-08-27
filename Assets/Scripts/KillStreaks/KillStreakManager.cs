using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillStreakManager : MonoBehaviour
{
    public static KillStreakManager instance;

    private static Dictionary<KillStreak, Transform> spawnPoints = new Dictionary<KillStreak, Transform>();

    private static List<GameObject> targets = new List<GameObject>();

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one KillStreakSpawnManager in scene.");
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        spawnPoints.Clear();
        foreach (Transform _child in transform)
        {
            KillStreakSpawnPoint _killStreakSpawnPoint = _child.GetComponent<KillStreakSpawnPoint>();

            if (_killStreakSpawnPoint != null)
            {
                spawnPoints.Add(_killStreakSpawnPoint.killStreak, _child.transform);
            }

        }
    }

    public static Transform GetKillStreakSpawnPoint(KillStreak _killStreak)
    {
        return spawnPoints[_killStreak];
    }
}

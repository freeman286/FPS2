using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelLoader : MonoBehaviour
{

    public static LevelLoader instance;

    public Animator transition;

    private float timeSinceCreated;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one LevelLoader in scene.");
        }
        else
        {
            instance = this;
        }
    }

    public void DoTransition()
    {
        if (timeSinceCreated > 0.1f) // If something is trying to trigger this before we've even faded in something is wrong
        {
            transition.SetTrigger("Start");
        }
    }

    void Update()
    {
        timeSinceCreated += Time.deltaTime;
    }
}

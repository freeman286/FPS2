using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelLoader : MonoBehaviour
{

    public static LevelLoader instance;

    public Animator transition;

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
        transition.SetTrigger("Start");
    } 
}

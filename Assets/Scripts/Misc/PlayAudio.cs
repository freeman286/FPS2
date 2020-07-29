using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    void OnEnable()
    {
        GetComponent<AudioSource>().Play();
    }
}

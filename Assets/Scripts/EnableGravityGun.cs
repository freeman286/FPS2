using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableGravityGun : MonoBehaviour
{
    void Start()
    {
        transform.root.GetComponent<GravityGun>().enabled = true;
    }

    void OnDestroy()
    {
        transform.root.GetComponent<GravityGun>().enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MessageItem : MonoBehaviour
{

    [SerializeField]
    Text text = null;

    public void Setup(string message)
    {
        text.text = message;
    }

}
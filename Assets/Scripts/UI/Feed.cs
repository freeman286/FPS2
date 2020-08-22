using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : MonoBehaviour
{

    [SerializeField]
    private GameObject messageItemPrefab = null;

    void Start()
    {
        GameManager.instance.onPlayerKilledCallback += OnKill;
        GameManager.instance.messageCallback += Message;
    }

    public void OnKill(string player, string source)
    {
        Message("<b>" + source + "</b>" + " killed " + "<b>" + player + "</b>");
    }

    public void Message(string message)
    {
        GameObject go = (GameObject)Instantiate(messageItemPrefab, this.transform);
        go.GetComponent<MessageItem>().Setup(message);
        go.transform.SetAsFirstSibling();

        Destroy(go, 8f);
    }
}

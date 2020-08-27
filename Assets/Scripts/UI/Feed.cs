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

    public void OnKill(string _playerID, string _sourceID)
    {
        Player _player = GameManager.GetPlayer(_playerID);
        Player _sourcePlayer = GameManager.GetPlayer(_sourceID);

        if (_player != null && _sourcePlayer != null)
            Message("<b>" + _sourcePlayer.username + "</b>" + " killed " + "<b>" + _player.username + "</b>");
    }

    public void Message(string message)
    {
        GameObject go = (GameObject)Instantiate(messageItemPrefab, this.transform);
        go.GetComponent<MessageItem>().Setup(message);
        go.transform.SetAsFirstSibling();

        Destroy(go, 8f);
    }

    void OnDestroy()
    {
        GameManager.instance.onPlayerKilledCallback -= OnKill;
        GameManager.instance.messageCallback -= Message;
    }
}

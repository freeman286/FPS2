using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Mirror;

public class OfflineUI : MonoBehaviour
{
    [HideInInspector]
    public NetworkManager networkManager;
    [HideInInspector]
    public NetworkManagerHUD networkManagerHUD;

    [SerializeField]
    private InputField ipAddress;

    [SerializeField]
    private Image loadingImage;

    [SerializeField]
    private RectTransform loadingRectTransform;

    private bool ipValueChanged = true;

    public void Start()
    {
        networkManager = NetworkManager.singleton;
        networkManagerHUD = networkManager.GetComponent<NetworkManagerHUD>();
        networkManagerHUD.enabled = false;
        ipAddress.text = "localhost";
        loadingImage.enabled = false;
        ipAddress.onValueChanged.AddListener(delegate { IpValueChange(); });
    }

    public void Host()
    {
        if (string.IsNullOrEmpty(PlayerInfo.playerName))
            return;

        networkManager.StartHost();
        InvokeRepeating("Loading", 0f, 0.01f);
    }

    public void Client()
    {
        if (string.IsNullOrEmpty(PlayerInfo.playerName))
            return;

        if (ipValueChanged)
        {
            networkManager.networkAddress = ipAddress.text;
            loadingImage.enabled = true;
            InvokeRepeating("Loading", 0f, 0.01f);
            networkManager.StartClient();
            ipValueChanged = false;
        }
        
    }

    void Loading()
    {
        loadingRectTransform.Rotate(new Vector3(0, 0, 1));
    }

    void IpValueChange()
    {
        ipValueChanged = true;
    }
}

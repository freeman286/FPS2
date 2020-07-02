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
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Host()
    {
        if (string.IsNullOrEmpty(PlayerInfo.playerName))
            return;

        InvokeRepeating("Loading", 0f, 0.01f);

        StartCoroutine(Host_Coroutine());

    }

    private IEnumerator Host_Coroutine()
    {
        LevelLoader.instance.DoTransition();
        yield return new WaitForSeconds(1f);
        networkManager.StartHost();
    }

    public void Client()
    {
        if (string.IsNullOrEmpty(PlayerInfo.playerName))
            return;

        if (ipValueChanged)
        {
            InvokeRepeating("Loading", 0f, 0.01f);
            StartCoroutine(Client_Coroutine());
        }
        
    }

    private IEnumerator Client_Coroutine()
    {
        LevelLoader.instance.DoTransition();
        yield return new WaitForSeconds(1f);
        networkManager.networkAddress = ipAddress.text;
        loadingImage.enabled = true;
        networkManager.StartClient();
        ipValueChanged = false;
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

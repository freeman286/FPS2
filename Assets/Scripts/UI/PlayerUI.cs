using UnityEngine;
using UnityEngine.UI;
using Mirror;

using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{

    [SerializeField]
    public GameObject crosshair = null;

    [SerializeField]
    private GameObject healthBar = null;

    [SerializeField]
    private RectTransform healthBarFill = null;

    [SerializeField]
    private GameObject ammo = null;

    [SerializeField]
    private Text ammoText = null;

    [SerializeField]
    private GameObject equipmentBar = null;

    [SerializeField]
    private RectTransform equipmentBarFill = null;

    [SerializeField]
    private Image equipmentIcon = null;

    [SerializeField]
    private GameObject abilityBar = null;

    [SerializeField]
    private RectTransform abilityBarFill = null;

    [SerializeField]
    private Image abilityIcon = null;

    [SerializeField]
    private GameObject scoreboard = null;

    [SerializeField]
    private GameObject pauseMenu = null;

    [SerializeField]
    private GameObject ipAddress = null;

    [SerializeField]
    private Text IpAddressText = null;

    [SerializeField]
    private Slider lookSensitivitySlider = null;

    [SerializeField]
    private Text lookSensitivityText = null;

    private Player player;
    private Health health;
    private PlayerController controller;
    private PlayerShoot shoot;
    private WeaponManager weaponManager;
    private PlayerEquipment playerEquipment;
    private PlayerMovementAbilityController movementAbilityController;

    public bool enableInfoUI = true;


    [HideInInspector]
    public NetworkManager networkManager;
    [HideInInspector]
    public NetworkManagerHUD networkManagerHUD;

    public void SetPlayer(Player _player)
    {
        player = _player;
        health = player.GetComponent<Health>();
        controller = player.GetComponent<PlayerController>();
        shoot = player.GetComponent<PlayerShoot>();
        weaponManager = player.GetComponent<WeaponManager>();

        playerEquipment = player.GetComponent<PlayerEquipment>();
        playerEquipment.onEquipmentChangedCallback += UpdateEquipmentSprite;

        movementAbilityController = player.GetComponent<PlayerMovementAbilityController>();
        movementAbilityController.onMovementAbilityChangedCallback += UpdateAbilitySprite;

        player.onPlayerSetDefaultsCallback += SetDefaults;
        player.onPlayerDieCallback += Die;

        networkManager = NetworkManager.singleton;
        networkManagerHUD = networkManager.GetComponent<NetworkManagerHUD>();
        networkManagerHUD.enabled = false;

        IpAddressText.text = "IP Address: " + Util.LocalIPAddress();

        lookSensitivitySlider.value = PlayerInfo.lookSensitivity;
    }

    void Update()
    {

        if (Pause.IsOn)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        SetHealthAmount(health.GetHealthPct());
        SetEquipmentAmount(playerEquipment.GetEquipmentPct());
        SetAbilityAmount(movementAbilityController.GetAbilityPct());

        PlayerWeapon currentWeapon = weaponManager.GetCurrentWeapon();

        if (currentWeapon != null)
            SetAmmoAmount(currentWeapon.bullets);

        if (Input.GetButtonDown("Cancel"))
        {
            TogglePauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboard.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboard.SetActive(false);
        }

        ipAddress.SetActive(scoreboard.activeSelf || pauseMenu.activeSelf);
    }

    void OnApplicationFocus(bool pauseStatus)
    {
        if (pauseStatus)
        {
            pauseMenu.SetActive(true);
            Pause.IsOn = true;
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        Pause.IsOn = pauseMenu.activeSelf;
    }

    void SetHealthAmount(float _amount)
    {
        healthBarFill.localScale = new Vector3(1f, Mathf.Lerp(healthBarFill.localScale.y, _amount, 20f * Time.deltaTime), 1f);
    }

    void SetEquipmentAmount(float _amount)
    {
        equipmentBarFill.localScale = new Vector3(1f, Mathf.Lerp(equipmentBarFill.localScale.y, _amount, 20f * Time.deltaTime), 1f);
    }

    void SetAbilityAmount(float _amount)
    {
        abilityBarFill.localScale = new Vector3(1f, Mathf.Lerp(abilityBarFill.localScale.y, _amount, 20f * Time.deltaTime), 1f);
    }

    void SetAmmoAmount(int _amount)
    {
        ammoText.text = _amount.ToString();
    }

    public void SetDefaults()
    {
        EnableInfoUI(true);
    }

    void UpdateEquipmentSprite(Sprite icon)
    {
        equipmentIcon.sprite = icon;
    }

    void UpdateAbilitySprite(Sprite icon)
    {
        abilityIcon.sprite = icon;
    }

    public void Die()
    {
        EnableInfoUI(false);
    }

    public void UpdateLookSensitivity()
    {
        PlayerInfo.lookSensitivity = (float)System.Math.Round(lookSensitivitySlider.value, 1);
        if (controller != null)
        {
            controller.lookSensitivity = PlayerInfo.lookSensitivity;
        }
        lookSensitivityText.text = string.Format("Sensitivity: {0:F1}", PlayerInfo.lookSensitivity); ;
    }

    public void Disconnect()
    {
        EnableInfoUI(false);
        scoreboard.SetActive(false);
        pauseMenu.SetActive(false);
        ipAddress.SetActive(false);
        controller.enabled = false;
        shoot.enabled = false;

        StartCoroutine(Disconnect_Coroutine());
    }

    private IEnumerator Disconnect_Coroutine()
    {
        LevelLoader.instance.DoTransition();
        yield return new WaitForSeconds(1f);
        networkManager.StopClient();
        networkManager.StopHost();
    }

    void EnableInfoUI(bool _enabled)
    {
        enableInfoUI = _enabled;

        crosshair.SetActive(_enabled);
        healthBar.SetActive(_enabled);
        ammo.SetActive(_enabled);
        abilityBar.SetActive(_enabled);
        equipmentBar.SetActive(_enabled);
    }

}
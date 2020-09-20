using UnityEngine;
using UnityEngine.UI;
using Mirror;

using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    [Header("Health")]

    [SerializeField]
    private GameObject healthBar = null;

    [SerializeField]
    private RectTransform healthBarFill = null;

    [Header("Ammo")]

    [SerializeField]
    private GameObject ammo = null;

    [SerializeField]
    private Text ammoText = null;

    [Header("Equipment")]

    [SerializeField]
    private GameObject equipmentBar = null;

    [SerializeField]
    private RectTransform equipmentBarFill = null;

    [SerializeField]
    private Image equipmentIcon = null;

    [Header("Ability")]

    [SerializeField]
    private GameObject abilityBar = null;

    [SerializeField]
    private RectTransform abilityBarFill = null;

    [SerializeField]
    private Image abilityIcon = null;

    [Header("Killstreak")]

    [SerializeField]
    private GameObject killStreakBar = null;

    [SerializeField]
    private RectTransform killStreakBarFill = null;

    [SerializeField]
    private Image killStreakIcon = null;

    [Header("Pause Menu")]

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

    [Header("Misc")]

    [SerializeField]
    public GameObject crosshair = null;

    [SerializeField]
    private GameObject scoreboard = null;

    [SerializeField]
    private float LerpSpeed = 20f;

    private Player player;
    private Health health;
    private PlayerController controller;
    private PlayerShoot shoot;
    private WeaponManager weaponManager;
    private PlayerEquipment playerEquipment;
    private PlayerMovementAbilityController movementAbilityController;
    private PlayerKillStreakManager playerKillStreakManager;

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

        playerKillStreakManager = player.GetComponent<PlayerKillStreakManager>();
        playerKillStreakManager.onKillStreakChangedCallback += UpdateKillStreakSprite;

        player.onPlayerSetDefaultsCallback += SetDefaults;
        player.onPlayerDieCallback += Die;

        networkManager = NetworkManager.singleton;
        networkManagerHUD = networkManager.GetComponent<NetworkManagerHUD>();
        networkManagerHUD.enabled = false;

        IpAddressText.text = "IP Address: " + Util.LocalIPAddress();

        lookSensitivitySlider.value = PlayerPrefs.GetFloat(PlayerUtil.LOOK_SENSITIVITY_KEY, 3f);
    }

    void Update()
    {

        if (Pause.isOn)
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
        SetKillStreakAmount(playerKillStreakManager.GetKillStreakPct());

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

    void OnApplicationFocus(bool _pauseStatus)
    {
        if (_pauseStatus && !Pause.isOn)
            TogglePauseMenu();
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        Pause.isOn = pauseMenu.activeSelf;

        if (Pause.isOn && Pause.instance.pausedCallback != null)
            Pause.instance.pausedCallback.Invoke();
    }

    void SetHealthAmount(float _amount)
    {
        healthBarFill.localScale = new Vector3(1f, Mathf.Lerp(healthBarFill.localScale.y, _amount, LerpSpeed * Time.deltaTime), 1f);
    }

    void SetEquipmentAmount(float _amount)
    {
        equipmentBarFill.localScale = new Vector3(1f, Mathf.Lerp(equipmentBarFill.localScale.y, _amount, LerpSpeed * Time.deltaTime), 1f);
    }

    void SetAbilityAmount(float _amount)
    {
        abilityBarFill.localScale = new Vector3(1f, Mathf.Lerp(abilityBarFill.localScale.y, _amount, LerpSpeed * Time.deltaTime), 1f);
    }

    void SetKillStreakAmount(float _amount)
    {
        killStreakBarFill.localScale = new Vector3(1f, Mathf.Lerp(killStreakBarFill.localScale.y, _amount, LerpSpeed * Time.deltaTime), 1f);
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

    void UpdateKillStreakSprite(Sprite icon)
    {
        killStreakIcon.sprite = icon;
    }

    public void Die()
    {
        EnableInfoUI(false);
    }

    public void UpdateLookSensitivity()
    {
        float _lookSensitivity = (float)System.Math.Round(lookSensitivitySlider.value, 1);

        if (controller != null)
            controller.lookSensitivity = _lookSensitivity;
        lookSensitivityText.text = string.Format("Sensitivity: {0:F1}", _lookSensitivity);

        PlayerPrefs.SetFloat(PlayerUtil.LOOK_SENSITIVITY_KEY, _lookSensitivity);
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
        killStreakBar.SetActive(_enabled);
    }

}
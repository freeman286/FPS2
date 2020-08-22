using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

[System.Serializable]
public class DieEvent : UnityEvent<string>{}

public class Health : NetworkBehaviour
{
    [SyncVar]
    public bool isDead = false;

    public DieEvent dieEvent;

    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private float currentHealth;

    [SerializeField]
    private float healthRegenTime = 10f;

    [SerializeField]
    private float healthRegenSpeed = 10f;

    private float timeSinceDamaged = 0f;

    [HideInInspector]
    public Player lastDamagedPlayer;

    public float GetHealthPct()
    {
        return Mathf.Clamp((float)currentHealth / maxHealth, 0, 1);
    }

    void Start()
    {
        
        Player _player = GetComponent<Player>();

        if (_player != null)
        {
            _player.onPlayerSetDefaultsCallback += SetDefaults;
        } else
        {
            SetDefaults();
        }

    }

    [ServerCallback]
    void Update()
    {
        timeSinceDamaged += Time.deltaTime;

        if (healthRegenTime != -1f && timeSinceDamaged > healthRegenTime && currentHealth < maxHealth)
        {
            currentHealth += healthRegenSpeed * Time.deltaTime;
        }
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _sourceID, string _damageType)
    {
        if (isDead)
            return;

        lastDamagedPlayer = GameManager.GetPlayer(_sourceID);

        float _amountModified = _amount * lastDamagedPlayer.GetComponent<PlayerStats>().GetDamageMultiplier(_damageType, false);

        PlayerStats _stats = GetComponent<PlayerStats>();

        if (_stats != null)
            _amountModified *= _stats.GetDamageMultiplier(_damageType, true);

        currentHealth -= _amountModified;

        timeSinceDamaged = 0f;

        if (currentHealth <= 0)
        {
            isDead = true;

            if (dieEvent != null)
                dieEvent.Invoke(_sourceID);
        }
    }

    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;
    }
}

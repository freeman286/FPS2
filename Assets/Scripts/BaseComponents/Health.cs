using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Health : NetworkBehaviour
{
    [SyncVar]
    public bool isDead = false;

    [SerializeField]
    private UnityEvent dieEvent;

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
        SetDefaults();
    }

    void Update()
    {
        timeSinceDamaged += Time.deltaTime;

        if (timeSinceDamaged > healthRegenTime && currentHealth < maxHealth)
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
            dieEvent.Invoke();
        }
    }

    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;
    }
}

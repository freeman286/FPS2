using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    [SerializeField]
    private string fadePropertyReference = "Vector1_BCEE2681";

    [SerializeField]
    private float waitDuration = 4f;

    [SerializeField]
    private float fadeDuration = 1f;

    private List<Renderer> playerModelRenderers = new List<Renderer>();
    private List<Renderer> gunModelRenderers = new List<Renderer>();

    private float fadeTime = 0f;

    private bool fading;

    void Start()
    {
        Player _player = GetComponent<Player>();
        _player.onPlayerSetDefaultsCallback += SetDefaults;
        _player.onPlayerDieCallback += Die;

        GetComponent<WeaponManager>().onWeaponChangedCallback += WeaponChanged;

        foreach (GameObject _bodyPart in _player.rigidbodyOnDeath)
        {
            foreach (Component _component in _bodyPart.GetComponentsInChildren(typeof(Renderer)))
            {
                playerModelRenderers.Add(_component.GetComponent<Renderer>());
            }
        }

    }

    void Update()
    {
        if (fading) {
            fadeTime += Time.deltaTime;
            if (fadeTime - waitDuration <= fadeDuration && fadeTime - waitDuration > 0f)
            {
                float _fadeAmount = (fadeTime - waitDuration) / fadeDuration;

                SetFloatForList(gunModelRenderers, fadePropertyReference, _fadeAmount);
                SetFloatForList(playerModelRenderers, fadePropertyReference, _fadeAmount);
            }
        }
    }

    void SetDefaults()
    {
        fading = false;
        SetFloatForList(playerModelRenderers, fadePropertyReference, 0f);
    }

    void Die()
    {
        fading = true;
        fadeTime = 0f;
    }

    void WeaponChanged(Component[] _gunModelRenderers)
    {
        gunModelRenderers.Clear();

        foreach (Component _component in _gunModelRenderers) {
            gunModelRenderers.Add(_component.GetComponent<Renderer>());
        }

        SetFloatForList(gunModelRenderers, "Dissolve Amount", 0f);
    }

    void SetFloatForList(List<Renderer> _list, string _float, float _amount)
    {
        foreach(Renderer _renderer in _list)
        {
            _renderer.material.SetFloat(_float, _amount);
        }
    }

}

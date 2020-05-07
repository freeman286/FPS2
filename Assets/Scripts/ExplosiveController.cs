using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExplosiveController : NetworkBehaviour
{
    private ProjectileController projectileController;

    [SerializeField]
    private GameObject impact;

    [SerializeField]
    private int damage;

    [SerializeField]
    private float range;

    [SerializeField]
    private float fuse;

    [SerializeField]
    private AnimationCurve damageFallOff;


    // Start is called before the first frame update
    void Start()
    {
        projectileController = GetComponent<ProjectileController>();
    }

    void Update()
    {
        fuse -= Time.deltaTime;
        if (fuse <= 0)
        {
            CmdExplode(Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        CmdExplode(Quaternion.LookRotation(collision.contacts[0].normal));
    }

    [Command]
    void CmdExplode(Quaternion _rot)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);

        foreach (var _hit in hitColliders)
        {

            if (_hit.transform.name == "Area" && _hit.transform.root.GetComponent<Player>())
            {

                float _distance = Vector3.Distance(_hit.transform.position, transform.position);

                _hit.transform.root.GetComponent<Player>().RpcTakeDamage((int)(damageFallOff.Evaluate(_distance / range) * damage), projectileController.playerID);

            }

        }

        RpcExplode(_rot);
       
    }

    [ClientRpc]
    public void RpcExplode(Quaternion _rot)
    {
        GameObject _impact = (GameObject)Instantiate(impact, transform.position, _rot);

        Destroy(_impact, 4f);
        Destroy(gameObject);
    }

}



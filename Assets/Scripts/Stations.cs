using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StationType
{
    Health, Ammo
}

public class Stations : MonoBehaviour
{

    [SerializeField]
    public StationType type;
    [SerializeField]
    float cooldown;

    bool inCooldown = false;
    [SerializeField]
    MeshRenderer _renderer;

    IEnumerator CoolDown()
    {
        inCooldown = true;
        _renderer.enabled = false;

        yield return new WaitForSeconds(cooldown);

        inCooldown = false;
        _renderer.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Bullet") || inCooldown ||other.tag.Equals("Wall")) return;

        IAttacker p = other.GetComponent<IAttacker>();

        Debug.Log("Someone used me");

        if (type == StationType.Health)
            p.Health = 100;
        else if (type == StationType.Ammo)
            p.Ammo = 30;

        StartCoroutine(CoolDown());
    }
}

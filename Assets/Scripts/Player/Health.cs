using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float health;
    Material instancedMaterial;
    public bool isDead { get; private set; }

    public UnityEvent onDie = new UnityEvent();

    private void Start()
    {
        instancedMaterial = GetComponentInChildren<SpriteRenderer>().material;
    }

    public void Hit(float damages)
    {
        if (isDead)
            return;

        instancedMaterial.SetFloat("_HitTime", Time.time);
        health -= damages;

        if (health < 0)
            Die();
    }

    public void Die()
    {
        isDead = true;
        onDie.Invoke();
    }

    public void Heal(float amount)
    {
        health += amount;
    }
}

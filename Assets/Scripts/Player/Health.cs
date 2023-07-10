using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float health;
    public int maxHealth = 10;
    Material instancedMaterial;
    public bool isDead { get; private set; }
    public CompositeState isInvicibleState = new CompositeState();
    private CompositeStateToken invincible = new CompositeStateToken();
    public float invicibleTime;

    [Header("SFX")]
    public Source team;

    public UnityEvent onDie = new UnityEvent();
    public UnityEvent onHit = new UnityEvent();
    public UnityEvent onHeal = new UnityEvent();

    private void Start()
    {
        instancedMaterial = GetComponentInChildren<SpriteRenderer>().material;
        isInvicibleState.Add(invincible);
    }

    public void Hit(float damages)
    {
        if (isDead)
            return;

        if (isInvicibleState.IsOn)
            return;

        instancedMaterial.SetFloat("_HitTime", Time.time);
        health -= damages;
        onHit.Invoke();

        if (health <= 0)
            Die();

        switch (team)
        {
            case Source.Player:
                SFXManager.PlaySound(GlobalSFX.PlayerDamaged);
                break;
            case Source.Enemy:
                SFXManager.PlaySound(GlobalSFX.EnemyDamaged);
                break;
        }

        invincible.SetOn(true);
        StartCoroutine(InvicibleTiming());
    }

    IEnumerator InvicibleTiming()
    {
        yield return new WaitForSeconds(invicibleTime);
        invincible.SetOn(false);
    }

    public void Die()
    {
        switch (team)
        {
            case Source.Player:
                SFXManager.PlaySound(GlobalSFX.GameOver);
                break;
            case Source.Enemy:
                SFXManager.PlaySound(GlobalSFX.EnemyDeath);
                break;
        }

        GetComponent<Collider2D>().enabled = false;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.velocity = Vector2.zero;
        body.isKinematic = true;
        isDead = true;
        onDie.Invoke();

        Destroy(gameObject, 3);
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        onHeal.Invoke();
    }
}

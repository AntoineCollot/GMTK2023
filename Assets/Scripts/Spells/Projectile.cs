using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected SpellData data;
    protected Source source;
    protected Vector2 direction;
    protected Action<Health, SpellData> hitCallback;
    LayerMask layerMask;
    float knockback;

    public void Init(in SpellData data, Source source, Vector2 direction, Action<Health, SpellData> hitCallback, float knockback)
    {
        this.data = data;
        this.source = source;
        this.hitCallback = hitCallback;
        this.direction = direction;
        this.knockback = knockback;

        switch (source)
        {
            case Source.Player:
                layerMask = LayerMask.GetMask("Enemies", "Level");
                break;
            case Source.Enemy:
            default:
                layerMask = LayerMask.GetMask("Player", "Level");
                break;
        }

        GetComponent<Rigidbody2D>().velocity = direction * data.baseProjectileSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsInLayerMask(collision.transform.gameObject.layer))
            Hit(collision.transform.gameObject);
    }

    void Hit(GameObject obj)
    {
        if (obj.TryGetComponent(out Health health))
        {
            Damage(health);
        }

        Destroy(gameObject);
    }

    void Damage(Health health)
    {
        int appliedDamages = data.damages;
        health.Hit(appliedDamages);

        Vector2 knockbackDirection = direction;
        if (data.knockback > 0 && health.TryGetComponent(out IKnockbackable knockbackable))
            knockbackable.ApplyKnockback(knockbackDirection, knockback);

        hitCallback(health, data);
    }

    bool IsInLayerMask(int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}

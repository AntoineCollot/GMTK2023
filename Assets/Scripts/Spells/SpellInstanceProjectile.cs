using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInstanceProjectile : SpellInstance
{
    LayerMask layerMask;
    public override LayerMask LayerMask => layerMask;

    public override void Init(in SpellData data, Source source, Vector2 direction, Action<Health, SpellData> hitCallback)
    {
        base.Init(data, source, direction, hitCallback);

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
            knockbackable.ApplyKnockback(knockbackDirection, Knockback);

        hitCallback(health, data);
    }

    bool IsInLayerMask(int layer)
    {
        return LayerMask == (LayerMask | (1 << layer));
    }
}

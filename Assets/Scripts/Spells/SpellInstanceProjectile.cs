using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInstanceProjectile : SpellInstance
{
    LayerMask layerMask;
    public override LayerMask LayerMask => layerMask;
    public Projectile projectilePrefab;

    public override void Init(in SpellData data, ICastSpell source, Vector2 direction, Action<Health, SpellData> hitCallback, CompositeState isCastingState)
    {
        base.Init(data, source, direction, hitCallback, isCastingState);

        Invoke("EmitProjectile", AnticipationTime);
    }

    void EmitProjectile()
    {
        Projectile proj = Instantiate(projectilePrefab, null);
        proj.transform.position = transform.position;

        //Enemies can adjust the direction of the shoot
        if (source.Source == Source.Enemy)
            direction = (PlayerMovement.Instance.transform.position - transform.position).normalized;

        proj.Init(data, source.Source, direction, hitCallback, Knockback);
        isCastingToken.SetOn(false);
        source.OnSpellCastFinished();
        Destroy(gameObject);
    }
}

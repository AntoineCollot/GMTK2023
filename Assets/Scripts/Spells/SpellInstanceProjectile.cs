using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInstanceProjectile : SpellInstance
{
    LayerMask layerMask;
    public override LayerMask LayerMask => layerMask;
    public Projectile projectilePrefab;

    public override void Init(in SpellData data, Source source, Vector2 direction, Action<Health, SpellData> hitCallback, CompositeState isCastingState)
    {
        base.Init(data, source, direction, hitCallback, isCastingState);

        Invoke("EmitProjectile", AnticipationTime);
    }

    void EmitProjectile()
    {
        Projectile proj = Instantiate(projectilePrefab, null);
        proj.transform.position = transform.position;
        proj.Init(data, source, direction, hitCallback, Knockback);
        isCastingToken.SetOn(false);
        Destroy(gameObject);
    }
}

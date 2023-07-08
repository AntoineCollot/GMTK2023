using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Source { Player, Enemy}
public abstract class SpellInstance : MonoBehaviour
{
    protected SpellData data;
    protected Source source;
    protected Action<Health, SpellData> hitCallback;

    [Header("Settings")]
    public float anticipationTimePlayer = 0.2f;
    public float anticipationTimeEnemy = 0.5f;
    public float minSize = 1;
    public float maxSize = 3;
    public float minKnockback = 10;
    public float maxKnockback = 30;

    public const float MAX_AMELIORATION_VALUE = 10;

    public float AnticipationTime
    {
        get
        {
            if (source == Source.Player)
                return anticipationTimePlayer;

            return anticipationTimeEnemy;
        }
    }
    public float Size => Curves.QuadEaseOut(minSize, maxSize, Mathf.Clamp01(data.size / MAX_AMELIORATION_VALUE));
    public float Knockback => Curves.QuadEaseOut(minKnockback, maxKnockback, Mathf.Clamp01(data.knockback / MAX_AMELIORATION_VALUE));

    public LayerMask layerMask
    {
        get
        {
            if (source == Source.Player)
                return LayerMask.GetMask("Enemies");
            else
                return LayerMask.GetMask("Player");
        }
    }

    public virtual void Init(in SpellData data, Source source, Action<Health, SpellData> hitCallback)
    {
        this.data = data;
        this.source = source;
        this.hitCallback = hitCallback;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minSize);
        Gizmos.DrawWireSphere(transform.position, maxSize);
    }
}

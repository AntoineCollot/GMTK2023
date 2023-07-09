using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInstanceLaser : SpellInstance
{
    [Header("Laser Settings")]
    public float minSize = 2;
    public float maxSize = 7;
    public float Size => Curves.QuadEaseOut(minSize, maxSize, Mathf.Clamp01(data.size / MAX_AMELIORATION_VALUE));
    float laserLength;

    const float LASER_HIT_INTERVAL = 0.2f;
    public const float LASER_HIT_COUNT = 5;

    public float LaserWidth => transform.localScale.y;

    [Header("Animations")]
    public Sprite[] sprites;
    SpriteRenderer spriteRenderer;
    public float frameInterval = 0.05f;
    public SpriteRenderer anticipation;

    public override void Init(in SpellData data, ICastSpell source, Vector2 direction, Action<Health, SpellData> hitCallback, CompositeState isCastingState)
    {
        base.Init(data, source, direction, hitCallback, isCastingState);

        //Raycast to check for obstacles
        laserLength = RaycastLaserLength();

        //Move the transform to set the visuals correctly
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.size = new Vector2(laserLength, 1);
        anticipation.size = new Vector2(laserLength, 1);
        transform.right = direction;
    }

    private void Start()
    {
        StartCoroutine(ExecuteLaser(direction));
        StartCoroutine(Anim());
    }

    IEnumerator Anim()
    {
        int frame = 0;
        while (true)
        {
            spriteRenderer.sprite = sprites[frame % sprites.Length];
            frame++;
            yield return new WaitForSeconds(frameInterval);
        }
    }

    IEnumerator ExecuteLaser(Vector2 direction)
    {
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(AnticipationTime);
        spriteRenderer.enabled = true;

        source.OnSpellCastFinished();

        for (int i = 0; i < LASER_HIT_COUNT; i++)
        {
            //Raycast to check for obstacles
            //float laserLength = RaycastLaserLength();

            //Box cast to check for laser damages
            RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position,Vector2.one * LaserWidth, 0, direction, laserLength, LayerMask);
            foreach (RaycastHit2D boxHit in hits)
            {
                if (boxHit.transform == null)
                    continue;

                if (boxHit.transform.TryGetComponent(out Health health))
                {
                    Damage(health);
                }
            }
            yield return new WaitForSeconds(LASER_HIT_INTERVAL);
        }

        isCastingToken.SetOn(false);
        Destroy(gameObject);
    }

    void Damage(Health health)
    {
        float appliedDamages = data.damages * (1.0f/LASER_HIT_COUNT);
        health.Hit(appliedDamages);

        Vector2 knockbackDirection = direction;
        if (data.knockback > 0 && health.TryGetComponent(out IKnockbackable knockbackable))
            knockbackable.ApplyKnockback(knockbackDirection, Knockback * (1.0f / LASER_HIT_COUNT));

        hitCallback(health, data);
    }

    float RaycastLaserLength()
    {
        LayerMask levelMask = LayerMask.GetMask("Level");

        //Raycast to check for obstacles
        float laserLength = Size;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Size, levelMask);
        if (hit.transform != null)
            laserLength = hit.distance;

        return laserLength;
    }
}

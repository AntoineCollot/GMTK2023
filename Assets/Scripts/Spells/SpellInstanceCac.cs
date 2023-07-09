using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInstanceCac : SpellInstance
{
    [Header("Cac Settings")]
    public float minSize = 1;
    public float maxSize = 3;
    public float Size => Curves.QuadEaseOut(minSize, maxSize, Mathf.Clamp01(data.size / MAX_AMELIORATION_VALUE));

    public Transform anticipationCircleOutside;
    public Transform anticipationCircleInside;

    private void Start()
    {
        StartCoroutine(ExecuteSpell());
    }

    IEnumerator ExecuteSpell()
    {
        anticipationCircleOutside.localScale = Vector3.one * Size;

        float t = 0;
        while(t<1)
        {
            t += Time.deltaTime / AnticipationTime;

            anticipationCircleInside.localScale = Vector3.one * Mathf.Lerp(0, 1, t);

            yield return null;
        }

        anticipationCircleOutside.gameObject.SetActive(false);

        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            ParticleSystem.EmissionModule emission = particle.emission;
            ParticleSystem.ShapeModule shape = particle.shape;
            emission.rateOverTimeMultiplier = emission.rateOverTimeMultiplier * Size;
            shape.radius = Size - 0.2f;
            particle.Play();
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, Size, LayerMask);

        foreach (Collider2D collider in hitColliders)
        {
            //Make sure there is a health component
            if (!collider.TryGetComponent(out Health health))
                continue;

            int appliedDamages = data.damages;
            health.Hit(appliedDamages);

            Vector2 knockbackDirection = (collider.transform.position - transform.position).normalized;
            if (data.knockback > 0 && collider.TryGetComponent(out IKnockbackable knockbackable))
                knockbackable.ApplyKnockback(knockbackDirection, Knockback);

            //Callback
            hitCallback(health, data);
        }
        source.OnSpellCastFinished();
        isCastingToken.SetOn(false);

        Destroy(gameObject,3);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minSize);
        Gizmos.DrawWireSphere(transform.position, maxSize);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, Size);
    }
#endif
}

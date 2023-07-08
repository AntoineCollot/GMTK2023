using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellInstanceCac : SpellInstance
{
    private void Start()
    {
        StartCoroutine(ExecuteSpell());
    }

    IEnumerator ExecuteSpell()
    {
        yield return new WaitForSeconds(AnticipationTime);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, Size, layerMask);

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

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, Size);
    }
}

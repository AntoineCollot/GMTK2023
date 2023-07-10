using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimations : MonoBehaviour
{
    Animator anim;
    int directionHash;
    int moveSpeedHash;
    SpriteRenderer spriteRenderer;
    IAnimable animable;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.speed = 1f / 6f;
        spriteRenderer = GetComponent<SpriteRenderer>();
        PlayerDash dash = GetComponentInParent<PlayerDash>();
        GetComponentInParent<Health>().onDie.AddListener(OnDie);
        animable = GetComponentInParent<IAnimable>();
        if (dash != null)
        {
            dash.onDash.AddListener(OnDash);
            dash.onDashEnd.AddListener(OnDashEnd);
        }

        directionHash = Animator.StringToHash("Direction");
        moveSpeedHash = Animator.StringToHash("MoveSpeed");
    }

    public void OnEnnemyHit()
    {
        anim.SetTrigger("IsHit");
    }

    private void OnDie()
    {
        anim.SetBool("IsDead", true);
    }

    void LateUpdate()
    {
        switch (animable.AnimationDirection)
        {
            case Direction.Up:
                anim.SetFloat(directionHash, 0);
                spriteRenderer.flipX = false;
                break;
            case Direction.Right:
            default:
                anim.SetFloat(directionHash, 1);
                spriteRenderer.flipX = false;
                break;
            case Direction.Left:
                spriteRenderer.flipX = true;
                anim.SetFloat(directionHash, 1);
                break;
            case Direction.Down:
                spriteRenderer.flipX = false;
                anim.SetFloat(directionHash, 2);
                break;
        }
        anim.SetFloat(moveSpeedHash, animable.AnimationMoveSpeed);
    }

    void OnDash()
    {
        anim.SetBool("IsDashing", true);
    }

    void OnDashEnd()
    {
        anim.SetBool("IsDashing", false);
    }


    public void StartCast()
    {
        anim.SetTrigger("StartCast");
    }

    public void EndCast()
    {
        anim.SetTrigger("EndCast");
    }
}

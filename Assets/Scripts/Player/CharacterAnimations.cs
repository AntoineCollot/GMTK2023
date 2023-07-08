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
        animable = GetComponentInParent<IAnimable>();
        if (dash!=null)
            dash.onDash.AddListener(OnDash);

        directionHash = Animator.StringToHash("Direction");
        moveSpeedHash = Animator.StringToHash("MoveSpeed");
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

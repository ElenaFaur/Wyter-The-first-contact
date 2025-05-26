using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;
    PlayerStateList pState;
    private bool canDash = true;
    private bool dashed;
    private float gravity;

    [SerializeField] private LayerMask jumpableGround;

    private int jumpCounter;
    [SerializeField] private int resetJumpCounter = 1;

    private float dirX = 0f;
    private float dirY;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;
    private float jumpBufferCounter = 0;
    [SerializeField] private float jumpBufferFrames = 1;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime = 0.1f;
    [Space(5)]

    [Header("Dash Settings:")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 0.35f;
    [Space(5)]

    [Header("Attacking")]
    bool attack = false;
    float timeBetweenAttack, timeSinceAttack;
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;

    private enum MovementState { idle, running, jumping, falling }

    private void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    private void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        dirY = Input.GetAxisRaw("Vertical");
        attack = Input.GetMouseButtonDown(0);

        if (!pState.dashing)
        {
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        }

        UpdateJumpVariables();
        if (pState.dashing) return;

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }

        if (Input.GetButtonDown("Jump") && jumpCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            pState.jumping = true;
            jumpCounter--;
        }

        if (IsGrounded())
        {
            jumpCounter = resetJumpCounter;
        }

        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
        }

        StartDash();
        Attack();
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (dirX > 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;
        }
        else if (dirX < 0f)
        {
            state = MovementState.running;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > .1f)
        {
            state = MovementState.jumping;
        }
        else
            if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    void UpdateJumpVariables()
    {
        if (IsGrounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter = jumpBufferCounter - Time.deltaTime * 10;
        }
    }

    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if (IsGrounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        anim.SetTrigger("dashing");
        rb.gravityScale = 0;
        float dashDirection = dirX != 0 ? dirX : (sprite.flipX ? -1 : 1);
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;

        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("attacking");

            if (dirY == 0 || dirY < 0 && IsGrounded())
            {
                Hit(SideAttackTransform, SideAttackArea);
            }
            else if (dirY > 0)
            {
                Hit(UpAttackTransform, UpAttackArea);
            }
            else if (dirY < 0 && !IsGrounded())
            {
                Hit(DownAttackTransform, DownAttackArea);
            }
        }
    }

    private void Hit(Transform _attackTransform, Vector2 _attackArea)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            Debug.Log("Hit");
        }
    }
}

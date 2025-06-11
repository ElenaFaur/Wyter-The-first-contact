using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shade : Enemy
{
    [SerializeField] private float chaseDistance;
    float timer;
    [SerializeField]private float stunDuration;

    public static Shade Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        SaveData.Instance.SaveShadeData();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Shade_Idle);
    }

     protected override void Update()
    {
        base.Update();
        if (!PlayerMovement.Instance.pState.alive)
        {
            ChangeState(EnemyStates.Shade_Idle);
        }
    }

    protected override void UpdateEnemyStates()
    {
        float _dist = Vector2.Distance(transform.position, PlayerMovement.Instance.transform.position);

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Shade_Idle:
                rb.velocity = new Vector2(0, 0);
                if (_dist < chaseDistance)
                {
                    ChangeState(EnemyStates.Shade_Chase);
                }
                break;

            case EnemyStates.Shade_Chase:
                rb.MovePosition(Vector2.MoveTowards(transform.position, PlayerMovement.Instance.transform.position, Time.deltaTime * speed));
                Flip();

                if (_dist > chaseDistance)
                {
                    ChangeState(EnemyStates.Shade_Idle);
                }
                break;

            case EnemyStates.Shade_Stunned:
                timer += Time.deltaTime;

                if (timer > stunDuration)
                {
                    ChangeState(EnemyStates.Shade_Idle);
                    timer = 0;
                }
                break;

            case EnemyStates.Shade_Death:
                Death(Random.Range(5, 10));
                break;
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);

        if (health > 0)
        {
            ChangeState(EnemyStates.Shade_Stunned);
        }
        else
        {
            ChangeState(EnemyStates.Shade_Death);
        }
    }

    protected override void Death(float _destroyTime)
    {
        rb.gravityScale = 12;
        base.Death(_destroyTime);
    }

    protected override void ChangeCurrentAnimation()
    {
        int state = 0;
        if (GetCurrentEnemyState == EnemyStates.Shade_Idle)
        {
            anim.Play("Player_Idle");
        }
        if (GetCurrentEnemyState == EnemyStates.Shade_Chase)
        {
            state = 1;
            anim.SetInteger("state", state);
        }
        

        if (GetCurrentEnemyState == EnemyStates.Shade_Death)
        {
            PlayerMovement.Instance.RestoreMana();
            SaveData.Instance.SavePlayerData();
            anim.SetTrigger("death");
            Destroy(gameObject, 0.5f);
        }
    }

    protected override void Attack()
    {
        anim.SetTrigger("attacking");
        PlayerMovement.Instance.TakeDamage(damage);
    }
    void Flip()
    {
        sr.flipX = PlayerMovement.Instance.transform.position.x < transform.position.x;
    }
}

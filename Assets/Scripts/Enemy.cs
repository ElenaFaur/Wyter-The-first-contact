using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health = 100;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected PlayerMovement player;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    [SerializeField] protected GameObject orangeBlood;
    protected float recoilTimer;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;

    protected enum EnemyStates
    {
        //Crawler
        Crawler_Idle,
        Crawler_Flip,

        //Bat
        Bat_Idle,
        Bat_Chase,
        Bat_Stunned,
        Bat_Death,

        //Charger
        Charger_Idle,
        Charger_Surprised,
        Charger_Charge,

        //Shade
        Shade_Idle,
        Shade_Chase,
        Shade_Stunned,
        Shade_Death
    }

    protected EnemyStates currentEnemyState;
    protected virtual EnemyStates GetCurrentEnemyState
    {
        get { return currentEnemyState; }
        set
        {
            if (currentEnemyState != value)
            {
                currentEnemyState = value;

                ChangeCurrentAnimation();
            }
        }
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerMovement.Instance;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // if (health <= 0)
        // {
        //     Destroy(gameObject);
        // }

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
        else
        {
            UpdateEnemyStates();
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;

        if (!isRecoiling)
        {
            // rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
            GameObject _orangeBlood = Instantiate(orangeBlood, transform.position, Quaternion.identity);
            Destroy(_orangeBlood, 5.5f);
            rb.velocity = _hitForce * recoilFactor * _hitDirection;
            isRecoiling = true;
        }
    }

    protected void OnCollisionStay2D(Collision2D _other)
    {
        if (_other.gameObject.CompareTag("Player") && !PlayerMovement.Instance.pState.invincible && health > 0)
        {
            Attack();
            if (PlayerMovement.Instance.pState.alive)
            {
                PlayerMovement.Instance.HitStopTime(0.05f, 15, 0.04f);
            }
        }
    }

    protected virtual void Death(float _destroyTime)
    {
        Destroy(gameObject, _destroyTime);
    }

    protected virtual void UpdateEnemyStates() { }
    protected virtual void ChangeCurrentAnimation() { }

    protected virtual void ChangeState(EnemyStates _newState)
    {
        GetCurrentEnemyState = _newState;
    }
    protected virtual void Attack()
    {
        PlayerMovement.Instance.TakeDamage(damage);
        Vector2 recoilDirection = (player.transform.position - transform.position).normalized;
        PlayerMovement.Instance.Recoil(recoilDirection, 20);
    }
}

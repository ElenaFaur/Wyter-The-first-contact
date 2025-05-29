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
    protected float recoilTimer;
    protected Rigidbody2D rb;
    // Start is called before the first frame update
    // protected virtual void Start()
    // {

    // }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerMovement.Instance;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }

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
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;

        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
            isRecoiling = true;
        }
    }

    protected void OnCollisionStay2D(Collision2D _other)
    {
        if (_other.gameObject.CompareTag("Player") && !PlayerMovement.Instance.pState.invincible)
        {
            Attack();
            PlayerMovement.Instance.HitStopTime(0, 5, 0.5f);
        }
    }
    protected virtual void Attack()
    {
        PlayerMovement.Instance.TakeDamage(damage);
        Vector2 recoilDirection = (player.transform.position - transform.position).normalized;
        PlayerMovement.Instance.Recoil(recoilDirection, 20);
    }
}

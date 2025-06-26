using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    [HideInInspector] public Animator anim;
    private AudioSource audioSource;
    [HideInInspector] public PlayerStateList pState;
    private SpriteRenderer sr;
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

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallJumpingDuration;
    [SerializeField] private Vector2 wallJumpingPower;
    float wallJumpingDirection;
    bool isWallSliding;
    bool isWallJumping;
    public bool isWalled;
    [Space(5)]

    [Header("Dash Settings:")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 0.35f;
    [Space(5)]

    [Header("Attacking")]
    [SerializeField] bool attack = false;
    [SerializeField] private float timeBetweenAttack = 0.5f;
    private float timeSinceAttack;
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage = 10;
    [SerializeField] GameObject slashEffect;
    bool restoreTime;
    float restoreTimeSpeed;
    [Space(5)]

    [Header("Recoil")]
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    [SerializeField] private float recoilDuration = 0.2f;
    private bool isRecoiling = false;
    [Space(5)]

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    public int maxTotalHealth = 10;
    public int heartShards;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;
    float healTimer;
    [SerializeField] float timeToHeal;
    [Space(5)]

    [Header("Mana Settings")]
    [SerializeField] UnityEngine.UI.Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    public bool halfMana;
    [Space(5)]

    [Header("Spell Settings")]
    [SerializeField] float manaSpellCost = 0.3f;
    [SerializeField] float timeBetweenCast = 0.5f;
    float timeSinceCast;
    [SerializeField] float spellDamage; //upspellexplosion and downspellfireball
    [SerializeField] float downSpellForce; //desolate dive only
    [SerializeField] GameObject sideSpellFireball;
    [SerializeField] GameObject upSpellExplosion;
    [SerializeField] GameObject downSpellFireball;
    [Space(5)]

    [Header("Camera Stuff")]
    [SerializeField] private float playerFallSpeedThreshold = -10;
    [Space(5)]

    [Header("Audio")]
    // [SerializeField] AudioClip landingSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip dashAndAttackSound;
    [SerializeField] AudioClip spellCastSound;
    [SerializeField] AudioClip hurtSound;

    // private bool landingSoundPlayed;

    private enum MovementState { idle, running, jumping, falling }

    public static PlayerMovement Instance;
    bool openInventory;

    //unlocking abilities
    public bool unlockedWallJump;
    public bool unlockedDash;
    public bool unlockedJump;
    public bool unlockedSideCast;
    public bool unlockedUpCast;
    public bool unlockedDownCast;

    //unlocking puzzles
    // public bool unlockedBlueDoor;

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
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        pState = GetComponent<PlayerStateList>();
        pState.alive = true;
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        gravity = rb.gravityScale;
        Mana = mana;
        manaStorage.fillAmount = Mana;
        Health = maxHealth;

        SaveData.Instance.LoadPlayerData();

        if (halfMana == true)
        {
            UIManager.Instance.SwitchMana(UIManager.ManaState.HalfMana);
        }
        else
        {
            UIManager.Instance.SwitchMana(UIManager.ManaState.FullMana);
        }

        if (Health == 0)
        {
            pState.alive = false;
            GameManager.Instance.RespawnPlayer();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, 0.2f);
        }
    }

    private void Update()
    {
        if (GameManager.Instance.gameIsPaused) return;
        if (isRecoiling) return;

        if (pState.alive)
        {
            dirX = Input.GetAxisRaw("Horizontal");
            dirY = Input.GetAxisRaw("Vertical");
            attack = Input.GetMouseButtonDown(0);
            openInventory = Input.GetButton("Inventory");
            ToggleInventory();
        }

        if (!pState.dashing)
        {
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        }

        UpdateJumpVariables();
        UpdateCameraYDampForPlayerFall();

        if (pState.dashing) return;

        if (pState.alive)
        {
            RestoreTimeScale();
            if (!isWallJumping)
            {
                UpdateAnimationState();

                if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                    pState.jumping = false;
                }

                if (Input.GetButtonDown("Jump") && jumpCounter > 0 && unlockedJump)
                {
                    audioSource.PlayOneShot(jumpSound);
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
                        audioSource.PlayOneShot(jumpSound);
                        rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                        pState.jumping = true;
                    }
                }
            }

            if (unlockedWallJump)
            {
                WallSlide();
                WallJump();
            }

            if (unlockedDash)
            {
                StartDash();
            }
            Attack();
            Heal();
            CastSpell();
        }

        FlashWhileInvincible();

        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(Death());
        }

    }

    private void FixedUpdate()
    {
        if (pState.dashing || isRecoiling) return;

        // if (pState.recoilingX)
        // {
        //     float xDirection = pState.lookingRight ? -1f : 1f;
        //     Recoil(new Vector2(xDirection, 0f), recoilXSpeed);
        //     pState.recoilingX = false;
        // }

        // if (pState.recoilingY)
        // {
        //     Recoil(new Vector2(0f, -1f), recoilYSpeed);
        //     pState.recoilingY = false;
        // }
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.GetComponent<Enemy>() != null && pState.casting) //for up and down cast spell
        {
            _other.GetComponent<Enemy>().EnemyHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (dirX > 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;

            Vector3 pos = SideAttackTransform.localPosition;
            pos.x = Mathf.Abs(pos.x);
            SideAttackTransform.localPosition = pos;

            Vector3 posWallCheck = wallCheck.localPosition;
            posWallCheck.x = Mathf.Abs(posWallCheck.x);
            wallCheck.localPosition = posWallCheck;

            pState.lookingRight = true;
        }
        else if (dirX < 0f)
        {
            state = MovementState.running;
            sprite.flipX = true;

            Vector3 pos = SideAttackTransform.localPosition;
            pos.x = -Mathf.Abs(pos.x);
            SideAttackTransform.localPosition = pos;

            Vector3 posWallCheck = wallCheck.localPosition;
            posWallCheck.x = -Mathf.Abs(posWallCheck.x);
            wallCheck.localPosition = posWallCheck;

            pState.lookingRight = false;
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
            // if (!landingSoundPlayed)
            // {
            //     audioSource.PlayOneShot(landingSound);
            //     landingSoundPlayed = true;
            // }
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            // landingSoundPlayed = false;
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

    private bool Walled()
    {
        isWalled = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    void WallSlide()
    {
        if (Walled() && !IsGrounded() && dirX != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = !pState.lookingRight ? 1 : -1;

            CancelInvoke(nameof(StopWallJumping));
        }

        if (Input.GetButtonDown("Jump") && isWallSliding)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

            dashed = false;
            jumpCounter = 0;

            if (pState.lookingRight)
            {
                sprite.flipX = true;
                pState.lookingRight = false;
            }
            else
            {
                sprite.flipX = false;
                pState.lookingRight = true;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    void StopWallJumping()
    {
        isWallJumping = false;
    }

    void UpdateCameraYDampForPlayerFall()
    {
        //if falling past a certain speed threshold
        if (rb.velocity.y < playerFallSpeedThreshold && !CameraManager.Instance.isLerpingYDamping && !CameraManager.Instance.hasLerpedYDamping)
        {
            CameraManager.Instance.StartYDampingCoroutine(true);
        }
        //if standing still or moving up
        if (rb.velocity.y > 0 && !CameraManager.Instance.isLerpingYDamping && CameraManager.Instance.hasLerpedYDamping)
        {
            //reset camera function
            CameraManager.Instance.StartYDampingCoroutine(false);
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
        audioSource.PlayOneShot(dashAndAttackSound);
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
            audioSource.PlayOneShot(dashAndAttackSound);

            if (dirY == 0 || dirY < 0 && IsGrounded())
            {
                int _recoilLeftOrRight = pState.lookingRight ? 1 : -1;
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * _recoilLeftOrRight, recoilXSpeed);
                Instantiate(slashEffect, SideAttackTransform);
            }
            else if (dirY > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);
            }
            else if (dirY < 0 && !IsGrounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, Vector2.down, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
            }
        }
    }

    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilBool, Vector2 _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            _recoilBool = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, _recoilDir, _recoilStrength);
                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    Mana += manaGain;
                }
            }
        }
    }

    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;

        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
    }

    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    void FlashWhileInvincible()
    {
        sr.material.color = pState.invincible ? Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) : Color.white;
    }

    IEnumerator StartTimeAgain(float _delay)
    {
        yield return new WaitForSecondsRealtime(_delay);
        restoreTime = true;
    }

    IEnumerator Death()
    {
        pState.alive = false;
        Time.timeScale = 1f;
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);
        anim.SetTrigger("death");
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSecondsRealtime(0.9f);
        StartCoroutine(UIManager.Instance.ActiveDeathScreen());

        yield return new WaitForSecondsRealtime(0.9f);
        Instantiate(GameManager.Instance.shade, transform.position, Quaternion.identity);
    }

    public void Respawned()
    {
        if (!pState.alive)
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            GetComponent<BoxCollider2D>().enabled = true;
            pState.alive = true;
            halfMana = true;
            UIManager.Instance.SwitchMana(UIManager.ManaState.HalfMana);
            Mana = 0;
            Health = maxHealth;
            anim.Play("Player_Idle");
        }
    }

    public void RestoreMana()
    {
        halfMana = false;
        UIManager.Instance.SwitchMana(UIManager.ManaState.FullMana);
    }

    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }


    public void Recoil(Vector2 recoilDirection, float recoilStrength)
    {
        // Reset any existing recoil
        StopRecoil();

        // Apply force in the recoil direction
        rb.AddForce(recoilDirection.normalized * recoilStrength, ForceMode2D.Impulse);

        // Set recoil state
        isRecoiling = true;

        // Start coroutine to stop recoil after duration
        StartCoroutine(StopRecoilAfterTime(recoilDuration));
    }

    private IEnumerator StopRecoilAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        StopRecoil();
    }

    private void StopRecoil()
    {
        isRecoiling = false;
        // Optional: You might want to dampen the velocity after recoil
        rb.velocity = new Vector2(rb.velocity.x * 0.5f, rb.velocity.y * 0.5f);
    }

    public void TakeDamage(float _damage)
    {
        if (pState.alive)
        {
            audioSource.PlayOneShot(hurtSound);
            Health -= Mathf.RoundToInt(_damage);
            if (Health <= 0)
            {
                Health = 0;
                StartCoroutine(Death());
            }
            else
            {
                StartCoroutine(StopTakingDamage());
            }
        }

    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);
        anim.SetTrigger("takeDamage");
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

    void Heal()
    {
        if (Input.GetButton("Healing") && Health < maxHealth && Mana > 0 && !pState.jumping && !pState.dashing)
        {
            pState.healing = true;
            anim.SetBool("healing", true);

            //healing
            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }

            //drain mana
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            pState.healing = false;
            anim.SetBool("healing", false);
            healTimer = 0;
        }
    }

    public float Mana
    {
        get { return mana; }
        set
        {
            //if mana stats change
            if (mana != value)
            {
                if (!halfMana)
                {
                    mana = Mathf.Clamp(value, 0, 1);
                }
                else
                {
                    mana = Mathf.Clamp(value, 0, 0.5f);
                }
                manaStorage.fillAmount = Mana;
            }
        }
    }

    void CastSpell()
    {
        if (Input.GetButtonDown("CastSpell") && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
        {
            pState.casting = true;
            timeSinceCast = 0;
            StartCoroutine(CastCoroutine());
        }
        else
        {
            //disable downspell if on the ground
            timeSinceCast += Time.deltaTime;
        }

        if (IsGrounded())
        {
            downSpellFireball.SetActive(false);
        }

        //if down spell is active, force player down until grounded
        if (downSpellFireball.activeInHierarchy)
        {
            rb.velocity += downSpellForce * Vector2.down;
        }
    }

    IEnumerator CastCoroutine()
    {
        if (unlockedSideCast || unlockedUpCast || unlockedDownCast)
        {
            audioSource.PlayOneShot(spellCastSound);
        }

        //side cast
        if ((dirY == 0 || (dirY < 0 && IsGrounded())) && unlockedSideCast)
        {
            anim.SetBool("casting", true);
            yield return new WaitForSeconds(0.15f);
            GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);

            //flip fireball
            if (pState.lookingRight)
            {
                _fireBall.transform.eulerAngles = Vector3.zero;// if facing right, fireball continues as per normal
                Recoil(Vector2.left, recoilXSpeed * 0.6f);
            }
            else
            {
                _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180); //if not facing right, rotate the fireball 180 deg
                Recoil(Vector2.right, recoilXSpeed * 0.6f);
            }
            pState.recoilingX = true;

            Mana -= manaSpellCost;
            yield return new WaitForSeconds(0.35f);
        }

        //up cast
        else if (dirY > 0 && unlockedUpCast)
        {
            anim.SetBool("casting", true);
            yield return new WaitForSeconds(0.15f);
            // Instantiate(upSpellExplosion, transform);
            GameObject spell = Instantiate(upSpellExplosion, transform);
            if (!pState.lookingRight)
            {
                float offsetX = 20.5f;
                spell.transform.localPosition += new Vector3(-offsetX, 0, 0);
            }
            rb.velocity = Vector2.zero;
            Mana -= manaSpellCost;
            yield return new WaitForSeconds(0.35f);
        }

        //down cast
        else if (dirY < 0 && !IsGrounded() && unlockedDownCast)
        {
            anim.SetBool("casting", true);
            yield return new WaitForSeconds(0.15f);
            downSpellFireball.SetActive(true);
            Mana -= manaSpellCost;
            yield return new WaitForSeconds(0.35f);
        }

        anim.SetBool("casting", false);
        pState.casting = false;
    }

    void ToggleInventory()
    {
        if (openInventory)
        {
            UIManager.Instance.inventory.SetActive(true);
        }
        else
        {
            UIManager.Instance.inventory.SetActive(false);
        }
    }
}

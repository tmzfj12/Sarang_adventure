using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI Elements")]
    public GameObject[] healthImages;
    public TextMeshProUGUI stageText;

    [Header("Attack Settings")]
    public bool canAttack = false;
    public float attackDuration = 10f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackCooldown = 0.5f;

    [Header("Hurt Settings")]
    public float knockbackForce = 5f;
    public float invulnerabilityDuration = 1f;
    public int invulnerabilityFlashes = 3;

    private float nextAttackTime = 0f;
    private PlayerController playerController;
    private Animator animator;
    private float powerUpTimer;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // �ִϸ��̼� �Ķ����
    private readonly string IS_HURT_PARAM = "isHurt";
    private readonly string ATTACK_TRIGGER = "Attack";

    void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        UpdateHealthUI();

        // �ʱ� �������� �ؽ�Ʈ ����
        if (stageText != null)
            stageText.text = "Stage 1";
    }

    void Update()
    {
        // ���� �Ŀ��� ���� ����
        if (canAttack)
        {
            // ���� �Է� üũ �� ��ٿ� Ȯ��
            if (Input.GetKeyDown(KeyCode.LeftControl) && Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }

            powerUpTimer -= Time.deltaTime;
            if (powerUpTimer <= 0)
            {
                canAttack = false;
            }
        }
    }

    // ü�� UI ������Ʈ
    private void UpdateHealthUI()
    {
        if (healthImages != null)
        {
            for (int i = 0; i < healthImages.Length; i++)
            {
                if (healthImages[i] != null)
                    healthImages[i].SetActive(i < currentHealth);
            }
        }
    }

    // ������ �ޱ� (�˹� ���� �߰�)
    public void TakeDamage(int damage, Vector2 knockbackDirection = default)
    {
        // �̹� ���� ���¸� ����
        if (isInvulnerable)
            return;

        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;

        UpdateHealthUI();

        // �ǰ� �ִϸ��̼�
        if (animator != null)
        {
            animator.SetBool(IS_HURT_PARAM, true);
        }

        // �˹� ȿ�� ����
        if (rb != null && knockbackDirection != default)
        {
            rb.linearVelocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        // ���� �ð� Ȱ��ȭ
        StartCoroutine(InvulnerabilityCoroutine());

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    // �Ͻ��� ���� ���¸� ���� �ڷ�ƾ
    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        // ������ ȿ�� (���û���)
        if (spriteRenderer != null)
        {
            for (int i = 0; i < invulnerabilityFlashes; i++)
            {
                spriteRenderer.color = new Color(1, 1, 1, 0.5f);
                yield return new WaitForSeconds(invulnerabilityDuration / (invulnerabilityFlashes * 2));
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(invulnerabilityDuration / (invulnerabilityFlashes * 2));
            }
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
        }

        isInvulnerable = false;

        // �ǰ� �ִϸ��̼� ����
        if (animator != null)
        {
            animator.SetBool(IS_HURT_PARAM, false);
        }
    }

    // ü�� ȸ��
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateHealthUI();
    }

    // ���� ��� Ȱ��ȭ
    public void EnableAttack()
    {
        canAttack = true;
        powerUpTimer = attackDuration;
    }

    // ���� ����
    private void Attack()
    {
        if (canAttack && projectilePrefab != null && firePoint != null)
        {
            // �ణ�� �������� �߰��Ͽ� ����
            Vector2 spawnPosition = firePoint.position;
            bool isFacingRight = playerController.facingRight;

            // ���⿡ ���� �ణ �������� ������ �߰�
            if (isFacingRight)
                spawnPosition.x += 0.3f;
            else
                spawnPosition.x -= 0.3f;

            // ������ ��ġ�� ����ü ����
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // ���� ����
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(isFacingRight);
            }

            // �ִϸ��̼� Ʈ����
            if (animator != null)
                animator.SetTrigger("Attack");
        }
    }

    // ���� ����
    private void GameOver()
    {
        Debug.Log("Game Over");
        // ���ӿ��� �� �ʿ��� �߰� ����
        // ��: ���ӿ��� UI ǥ��, ����� �ɼ� ���� ��
    }

    // �������� ���� �� ȣ��
    public void UpdateStageText(int stageNumber)
    {
        if (stageText != null)
            stageText.text = $"Stage {stageNumber}";
    }
}
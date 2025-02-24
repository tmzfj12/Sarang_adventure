using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI Elements")]
    public GameObject[] healthImages;  // ���� �̹��� �迭
    public TextMeshProUGUI stageText; // �������� ǥ�� �ؽ�Ʈ

    [Header("Power-up Settings")]
    public bool canAttack = false;
    public float attackDuration = 10f;
    public GameObject attackCollider;

    private PlayerController playerController;
    private float powerUpTimer;

    void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        UpdateHealthUI();

        if (attackCollider != null)
            attackCollider.SetActive(false);

        // �ʱ� �������� �ؽ�Ʈ ����
        if (stageText != null)
            stageText.text = "Stage 1";
    }

    void Update()
    {
        // ���� �Ŀ��� ���� ����
        if (canAttack)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                Attack();
            }

            powerUpTimer -= Time.deltaTime;
            if (powerUpTimer <= 0)
            {
                canAttack = false;
                if (attackCollider != null)
                    attackCollider.SetActive(false);
            }
        }
    }

    private void UpdateHealthUI()
    {
        // ���� �̹����� ü�� ǥ��
        if (healthImages != null)
        {
            for (int i = 0; i < healthImages.Length; i++)
            {
                if (healthImages[i] != null)
                    healthImages[i].SetActive(i < currentHealth);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;

        UpdateHealthUI();

        // �÷��̾� �ǰ� �ִϸ��̼� ����
        if (playerController != null)
            playerController.TakeDamage();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateHealthUI();
    }

    public void EnableAttack()
    {
        canAttack = true;
        powerUpTimer = attackDuration;
    }

    private void Attack()
    {
        if (attackCollider != null)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private System.Collections.IEnumerator PerformAttack()
    {
        attackCollider.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        attackCollider.SetActive(false);
    }

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
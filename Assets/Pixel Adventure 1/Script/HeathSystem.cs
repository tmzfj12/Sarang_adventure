using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI Elements")]
    public GameObject[] healthImages;  // 딸기 이미지 배열
    public TextMeshProUGUI stageText; // 스테이지 표시 텍스트

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

        // 초기 스테이지 텍스트 설정
        if (stageText != null)
            stageText.text = "Stage 1";
    }

    void Update()
    {
        // 공격 파워업 상태 관리
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
        // 딸기 이미지로 체력 표시
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

        // 플레이어 피격 애니메이션 실행
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
        // 게임오버 시 필요한 추가 로직
        // 예: 게임오버 UI 표시, 재시작 옵션 제공 등
    }

    // 스테이지 변경 시 호출
    public void UpdateStageText(int stageNumber)
    {
        if (stageText != null)
            stageText.text = $"Stage {stageNumber}";
    }
}
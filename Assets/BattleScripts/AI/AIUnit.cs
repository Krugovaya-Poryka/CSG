using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private string aiName;

    private AIManager.AIDifficulty difficulty;

    private int currentHealth = 100;

    private int maxHealth = 100;

    private Vector3 currentPosition;

    private int attackDamage = 10;

    private float attackRange = 5f;

    private float visionRange = 15f;

    private float moveSpeed = 3f;

    private bool isAlive = true;

    private bool isTakingTurn = false;

    private bool isReadyForNextTurn = true;

    private int turnCount = 0;

    public void Initialize(string name, AIManager.AIDifficulty aiDifficulty, Vector3 startPosition)
    {
        aiName = name;
        difficulty = aiDifficulty;
        currentPosition = startPosition;

        SetupByDifficulty(aiDifficulty);

        gameObject.name = $"AI_{name}";
        transform.position = startPosition;
        
        Debug.Log($"<color=green>✓ AIUnit '{aiName}' ініціалізований</color>");
        Debug.Log($"  - Складність: {difficulty}");
        Debug.Log($"  - Здоров'я: {currentHealth}/{maxHealth}");
        Debug.Log($"  - Шкода: {attackDamage}");
        Debug.Log($"  - Позиція: {currentPosition}");
    }

    private void SetupByDifficulty(AIManager.AIDifficulty aiDifficulty)
    {
        switch (aiDifficulty)
        {
            case AIManager.AIDifficulty.Easy:
                attackDamage = 5;
                maxHealth = 60;
                currentHealth = maxHealth;
                visionRange = 10f;
                moveSpeed = 2f;
                Debug.Log("<color=yellow>⚙ Складність EASY: AttackDamage=5, Health=60, VisionRange=10</color>");
                break;
                
            case AIManager.AIDifficulty.Normal:
                attackDamage = 10;
                maxHealth = 100;
                currentHealth = maxHealth;
                visionRange = 15f;
                moveSpeed = 3f;
                Debug.Log("<color=yellow>⚙ Складність NORMAL: AttackDamage=10, Health=100, VisionRange=15</color>");
                break;
                
            case AIManager.AIDifficulty.Hard:
                attackDamage = 15;
                maxHealth = 150;
                currentHealth = maxHealth;
                visionRange = 20f;
                moveSpeed = 4f;
                Debug.Log("<color=yellow>⚙ Складність HARD: AttackDamage=15, Health=150, VisionRange=20</color>");
                break;
        }
    }

    public string AnalyzeBattlefield()
    {
        if (!isAlive)
            return "Не можу аналізувати - я мертва!";
        
        string analysis = $"[AI {aiName}] Аналізую поле бою...\n";
        analysis += $"  - Позиція: {currentPosition}\n";
        analysis += $"  - Здоров'я: {currentHealth}/{maxHealth}\n";
        analysis += $"  - Радіус видення: {visionRange}\n";
        analysis += $"  - Складність: {difficulty}\n";
        
        Debug.Log($"<color=cyan>{analysis}</color>");
        return analysis;
    }

    public bool CanSeeTarget(Vector3 targetPosition)
    {
        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        
        bool canSee = distanceToTarget <= visionRange;
        
        if (canSee)
        {
            Debug.Log($"<color=green>👀 {aiName} бачить ворога! Дистанція: {distanceToTarget:F2}</color>");
        }
        else
        {
            Debug.Log($"<color=red>❌ {aiName} не бачить ворога! Дистанція: {distanceToTarget:F2} (>vidrange:{visionRange})</color>");
        }
        
        return canSee;
    }

    public Vector3 ChooseTarget(Vector3[] enemyPositions)
    {
        if (enemyPositions.Length == 0)
        {
            Debug.LogWarning($"<color=orange>⚠ {aiName}: немає ворогів для вибору!</color>");
            return Vector3.zero;
        }
        
        Vector3 bestTarget = enemyPositions[0];
        float closestDistance = Vector3.Distance(currentPosition, bestTarget);
        
        for (int i = 1; i < enemyPositions.Length; i++)
        {
            float distance = Vector3.Distance(currentPosition, enemyPositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = enemyPositions[i];
            }
        }
        
        Debug.Log($"<color=yellow>🎯 {aiName} обирає ціль на відстані {closestDistance:F2}</color>");
        return bestTarget;
    }

    public void MoveTowards(Vector3 targetPosition)
    {
        if (!isAlive)
        {
            Debug.LogError($"<color=red>✗ {aiName} не можу рухатися - я мертва!</color>");
            return;
        }

        Vector3 direction = (targetPosition - currentPosition).normalized;

        currentPosition += direction * moveSpeed * Time.deltaTime;

        transform.position = currentPosition;

        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        
        Debug.Log($"<color=cyan">🚶 {aiName} рухається до цілі (дистанція: {distanceToTarget:F2})</color>");
    }

    public int Attack(Vector3 targetPosition)
    {
        if (!isAlive)
        {
            Debug.LogError($"<color=red>✗ {aiName} не можу атакувати - я мертва!</color>");
            return 0;
        }

        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        
        if (distanceToTarget > attackRange)
        {
            Debug.LogWarning($"<color=orange>⚠ {aiName}: ворог занадто далеко! (дистанція: {distanceToTarget:F2} > attackRange: {attackRange})</color>");
            return 0;
        }

        int actualDamage = attackDamage + Random.Range(-2, 3);
        actualDamage = Mathf.Max(1, actualDamage);
        
        Debug.Log($"<color=red>⚔ {aiName} атакує! Нанесена шкода: {actualDamage}</color>");
        
        return actualDamage;
    }

    public void StartTurn()
    {
        if (!isAlive)
        {
            Debug.LogError($"<color=red>✗ {aiName} не може робити хід - мертва!</color>");
            return;
        }
        
        isTakingTurn = true;
        turnCount++;
        
        Debug.Log($"<color=green">▶ {aiName} починає хід #{turnCount}</color>");
    }

    public void EndTurn()
    {
        isTakingTurn = false;
        
        Debug.Log($"<color=magenta>⏹ {aiName} завершив хід</color>");
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isAlive)
            return;
        
        currentHealth -= damageAmount;
        
        Debug.Log($"<color=red">💥 {aiName} отримав {damageAmount} шкоди! Здоров'я: {currentHealth}/{maxHealth}</color>");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (!isAlive)
            return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Макс = maxHealth
        
        Debug.Log($"<color=green">💚 {aiName} вилікований на {healAmount}! Здоров'я: {currentHealth}/{maxHealth}</color>");
    }

    public void Die()
    {
        isAlive = false;
        isTakingTurn = false;
        
        Debug.Log($"<color=red">☠ {aiName} ЗАГИНУВ!</color>");
    }

    public string GetName() => aiName;

    public int GetHealth() => currentHealth;

    public int GetMaxHealth() => maxHealth;

    public Vector3 GetPosition() => currentPosition;

    public int GetAttackDamage() => attackDamage;

    public float GetAttackRange() => attackRange;

    public bool IsAlive() => isAlive;

    public bool IsTakingTurn() => isTakingTurn;

    public int GetTurnCount() => turnCount;

    public string GetStatus()
    {
        return $"[{aiName}] Здоров'я: {currentHealth}/{maxHealth}, Ходи: {turnCount}, Жива: {isAlive}";
    }
}

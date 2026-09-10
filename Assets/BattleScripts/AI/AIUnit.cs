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
        
        Debug.Log($"<color=green>[+] AIUnit '{aiName}' initialized</color>");
        Debug.Log($"  - Difficulty: {difficulty}");
        Debug.Log($"  - Health: {currentHealth}/{maxHealth}");
        Debug.Log($"  - Damage: {attackDamage}");
        Debug.Log($"  - Position: {currentPosition}");
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
                Debug.Log("<color=yellow>[CONFIG] EASY: Damage=5, HP=60, Vision=10</color>");
                break;
                
            case AIManager.AIDifficulty.Normal:
                attackDamage = 10;
                maxHealth = 100;
                currentHealth = maxHealth;
                visionRange = 15f;
                moveSpeed = 3f;
                Debug.Log("<color=yellow>[CONFIG] NORMAL: Damage=10, HP=100, Vision=15</color>");
                break;
                
            case AIManager.AIDifficulty.Hard:
                attackDamage = 15;
                maxHealth = 150;
                currentHealth = maxHealth;
                visionRange = 20f;
                moveSpeed = 4f;
                Debug.Log("<color=yellow>[CONFIG] HARD: Damage=15, HP=150, Vision=20</color>");
                break;
        }
    }

    public string AnalyzeBattlefield()
    {
        if (!isAlive)
            return "Cannot analyze: dead";
        
        string analysis = $"[AI {aiName}] Analyzing battlefield...\n";
        analysis += $"  - Position: {currentPosition}\n";
        analysis += $"  - Health: {currentHealth}/{maxHealth}\n";
        analysis += $"  - Vision: {visionRange}\n";
        analysis += $"  - Difficulty: {difficulty}\n";
        
        Debug.Log($"<color=cyan>{analysis}</color>");
        return analysis;
    }

    public bool CanSeeTarget(Vector3 targetPosition)
    {
        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        bool canSee = distanceToTarget <= visionRange;
        
        if (canSee)
        {
            Debug.Log($"<color=green>[VISION] {aiName} sees target! Distance: {distanceToTarget:F2}</color>");
        }
        else
        {
            Debug.Log($"<color=red>[VISION] {aiName} cannot see target! Distance: {distanceToTarget:F2} (> Range: {visionRange})</color>");
        }
        
        return canSee;
    }

    public Vector3 ChooseTarget(Vector3[] enemyPositions)
    {
        if (enemyPositions.Length == 0)
        {
            Debug.LogWarning($"<color=orange>[!] {aiName}: No targets available!</color>");
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
        
        Debug.Log($"<color=yellow>[TARGET] {aiName} selected target at distance {closestDistance:F2}</color>");
        return bestTarget;
    }

    public void MoveTowards(Vector3 targetPosition)
    {
        if (!isAlive)
        {
            Debug.LogError($"<color=red>[-] {aiName} cannot move: dead</color>");
            return;
        }

        Vector3 direction = (targetPosition - currentPosition).normalized;
        currentPosition += direction * moveSpeed * Time.deltaTime;
        transform.position = currentPosition;

        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        Debug.Log($"<color=cyan>[MOVE] {aiName} moving to target (Distance: {distanceToTarget:F2})</color>");
    }

    public int Attack(Vector3 targetPosition)
    {
        if (!isAlive)
        {
            Debug.LogError($"<color=red>[-] {aiName} cannot attack: dead</color>");
            return 0;
        }

        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        if (distanceToTarget > attackRange)
        {
            Debug.LogWarning($"<color=orange>[!] {aiName}: Target out of range! ({distanceToTarget:F2} > {attackRange})</color>");
            return 0;
        }

        int actualDamage = attackDamage + Random.Range(-2, 3);
        actualDamage = Mathf.Max(1, actualDamage);
        
        Debug.Log($"<color=red>[ATTACK] {aiName} attacks! Damage dealt: {actualDamage}</color>");
        return actualDamage;
    }

    public void StartTurn()
    {
        if (!isAlive)
        {
            Debug.LogError($"<color=red>[-] {aiName} cannot take turn: dead</color>");
            return;
        }
        
        isTakingTurn = true;
        turnCount++;
        Debug.Log($"<color=green>[TURN] {aiName} started turn #{turnCount}</color>");
    }

    public void EndTurn()
    {
        isTakingTurn = false;
        Debug.Log($"<color=magenta>[TURN] {aiName} ended turn</color>");
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isAlive)
            return;
        
        currentHealth -= damageAmount;
        Debug.Log($"<color=red>[DAMAGE] {aiName} took {damageAmount} damage! Health: {currentHealth}/{maxHealth}</color>");

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
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"<color=green>[HEAL] {aiName} healed for {healAmount}! Health: {currentHealth}/{maxHealth}</color>");
    }

    public void Die()
    {
        isAlive = false;
        isTakingTurn = false;
        Debug.Log($"<color=red>[DEAD] {aiName} has been destroyed!</color>");
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
        return $"[{aiName}] HP: {currentHealth}/{maxHealth}, Turns: {turnCount}, Alive: {isAlive}";
    }
}

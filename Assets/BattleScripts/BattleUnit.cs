using System.Collections;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public UnitData data;
    public int stackSize;
    public int currentHealth; // Здоров'я верхньої істоти в стеку
    public int teamId;
    public Vector2Int hexCoords;

    public int remainingRetaliations;
    public int currentShots;

    public void Init(UnitData unitData, int count, int team)
    {
        data = unitData;
        stackSize = count;
        teamId = team;
        currentHealth = data.maxHealth;
        currentShots = data.maxShots;
        remainingRetaliations = data.retaliationsCount;
    }

    public void ResetRoundData()
    {
        remainingRetaliations = data.retaliationsCount;
    }

    // Формула розрахунку шкоди HOMM3
    public int CalculateDamageTo(BattleUnit target, bool isMeleePenalty = false)
    {
        // 1. Сумуємо базова шкоду для кожного юніта в стеку
        int baseDamageSum = 0;
        for (int i = 0; i < stackSize; i++)
        {
            baseDamageSum += Random.Range(data.minDamage, data.maxDamage + 1);
        }

        // 2. Модифікатор Атаки проти Захисту
        float modifier = 1.0f;

        if (data.attack > target.data.defense)
        {
            // +5% шкоди за кожну одиницю переваги атаки (макс. +300%)
            float bonus = (data.attack - target.data.defense) * 0.05f;
            modifier += Mathf.Min(bonus, 3.0f);
        }
        else if (data.attack < target.data.defense)
        {
            // -2.5% шкоди за кожну одиницю переваги захисту (мін. 30% підсумкової шкоди)
            float penalty = (target.data.defense - data.attack) * 0.025f;
            modifier -= penalty;
            modifier = Mathf.Max(modifier, 0.3f);
        }

        // 3. Штраф рукопашної для стрільців (-50%)
        if (isMeleePenalty)
        {
            modifier *= 0.5f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(baseDamageSum * modifier));
    }

    public void MeleeAttack(BattleUnit target, System.Action onComplete)
    {
        bool hasMeleePenalty = data.isRanged;
        int damage = CalculateDamageTo(target, hasMeleePenalty);

        Debug.Log($"{data.unitName} б'є {target.data.unitName} на {damage} шкоди!");
        target.TakeDamage(damage);

        // Контратака (якщо ціль вижила і має дозволені контратаки)
        if (target.stackSize > 0 && target.remainingRetaliations > 0)
        {
            int retDamage = target.CalculateDamageTo(this);
            Debug.Log($"{target.data.unitName} контратакує на {retDamage} шкоди!");
            TakeDamage(retDamage);
            target.remainingRetaliations--;
        }

        onComplete?.Invoke();
    }

    public void RangedAttack(BattleUnit target, System.Action onComplete)
    {
        if (currentShots <= 0)
        {
            Debug.Log("Немає пострілів!");
            onComplete?.Invoke();
            return;
        }

        currentShots--;
        int damage = CalculateDamageTo(target);

        Debug.Log($"{data.unitName} стріляє в {target.data.unitName} на {damage} шкоди! (Залишилося пострілів: {currentShots})");
        target.TakeDamage(damage);

        onComplete?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        int totalDamage = damage;

        // Попоштучне зняття HP та вибивання юнітів зі стеку
        while (totalDamage > 0 && stackSize > 0)
        {
            if (totalDamage < currentHealth)
            {
                currentHealth -= totalDamage;
                totalDamage = 0;
            }
            else
            {
                totalDamage -= currentHealth;
                stackSize--;
                currentHealth = data.maxHealth;
            }
        }

        if (stackSize <= 0)
        {
            stackSize = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{data.unitName} знищено!");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        gameObject.name += "_Corpse";
    }

    public IEnumerator MoveToHex(GameObject targetHex, Vector2Int newCoords, System.Action onComplete)
    {
        Vector3 targetPos = targetHex.transform.position;
        targetPos.z = transform.position.z;

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 8f * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        hexCoords = newCoords;
        onComplete?.Invoke();
    }
}
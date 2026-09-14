using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

public class BattleUnit : MonoBehaviour
{
    public UnitData data;
    public int stackSize;
    public int currentHealth;
    public int teamId;
    public Vector2Int hexCoords;

    public int remainingRetaliations;
    public int currentShots;

    public int initialStackSize;

    [Header("Візуалізація")]
    public SpriteRenderer unitSprite;   // Посилання на SpriteRenderer на дочірньому Texture
    public Transform textureChild;      // Дочірній об'єкт Texture
    public GameObject damageTextPrefab; // Префаб випливаючого тексту
    public TMP_Text stackText; // Посилання на текст кількості юнітів

    public void Init(UnitData unitData, int count, int team)
    {
        data = unitData;
        stackSize = count;
        initialStackSize = count;
        teamId = team;
        currentHealth = data.maxHealth;
        currentShots = data.maxShots;
        remainingRetaliations = data.retaliationsCount;

        if (unitSprite == null) unitSprite = GetComponentInChildren<SpriteRenderer>();
        if (textureChild == null) textureChild = transform.Find("Texture");

        // Автоматичний пошук тексту, якщо забули перетягнути в Inspector
        if (stackText == null) stackText = GetComponentInChildren<TMP_Text>();

        if (unitSprite != null && data != null && data.idleSprite != null)
        {
            unitSprite.sprite = data.idleSprite;
        }

        UpdateStackText();
    }

    public void UpdateStackText()
    {
        if (stackText != null)
        {
            stackText.text = stackSize.ToString();

            // Якщо стек знищено — ховаємо текст
            stackText.gameObject.SetActive(stackSize > 0);
        }
    }

    public void TakeDamage(int damage)
    {
        ShowDamageText(damage);

        int totalDamage = damage;
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

        UpdateStackText();

        if (stackSize <= 0)
        {
            stackSize = 0;
            Die();
        }
    }

    public void ResetRoundData()
    {
        remainingRetaliations = data.retaliationsCount;
    }

    public void FaceTarget(Vector3 targetPos)
    {
        float deltaX = targetPos.x - transform.position.x;

        if (Mathf.Abs(deltaX) > 0.01f)
        {
            if (textureChild == null) textureChild = transform.Find("Texture");

            if (textureChild != null)
            {
                Vector3 scale = textureChild.localScale;
                float absX = Mathf.Abs(scale.x);
                scale.x = (deltaX < 0) ? -absX : absX;
                textureChild.localScale = scale;
            }
        }
    }

    // --- БЛИЖНІЙ БІЙ ---
    public void MeleeAttack(BattleUnit target, System.Action onComplete)
    {
        StartCoroutine(MeleeAttackRoutine(target, onComplete));
    }

    private IEnumerator MeleeAttackRoutine(BattleUnit target, System.Action onComplete)
    {
        FaceTarget(target.transform.position);

        // 1. Увімкнути спрайт атаки
        if (unitSprite != null && data.attackSprite != null)
            unitSprite.sprite = data.attackSprite;

        yield return new WaitForSeconds(0.15f); // Коротка затримка замаху

        // 2. Нанесення шкоди
        bool hasMeleePenalty = data.isRanged;
        int damage = CalculateDamageTo(target, hasMeleePenalty);
        Debug.Log($"{data.unitName} б'є {target.data.unitName} на {damage} шкоди!");
        target.TakeDamage(damage);

        yield return new WaitForSeconds(0.25f); // Затримка показу кадру удару

        // 3. Повернення до Idle спрайту
        if (unitSprite != null && data.idleSprite != null)
            unitSprite.sprite = data.idleSprite;

        // 4. Контратака цілі
        if (target.stackSize > 0 && target.remainingRetaliations > 0)
        {
            yield return target.RetaliateRoutine(this);
        }

        onComplete?.Invoke();
    }

    // --- КОНТРАТАКА ---
    public IEnumerator RetaliateRoutine(BattleUnit attacker)
    {
        FaceTarget(attacker.transform.position);

        if (unitSprite != null && data.attackSprite != null)
            unitSprite.sprite = data.attackSprite;

        yield return new WaitForSeconds(0.15f);

        int retDamage = CalculateDamageTo(attacker);
        Debug.Log($"{data.unitName} контратакує на {retDamage} шкоди!");
        attacker.TakeDamage(retDamage);
        remainingRetaliations--;

        yield return new WaitForSeconds(0.25f);

        if (unitSprite != null && data.idleSprite != null)
            unitSprite.sprite = data.idleSprite;
    }

    // --- ДАЛЬНІЙ БІЙ ---
    public void RangedAttack(BattleUnit target, System.Action onComplete)
    {
        if (currentShots <= 0)
        {
            Debug.Log("Немає пострілів!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(RangedAttackRoutine(target, onComplete));
    }

    private IEnumerator RangedAttackRoutine(BattleUnit target, System.Action onComplete)
    {
        FaceTarget(target.transform.position);
        currentShots--;

        // 1. Увімкнути спрайт пострілу
        if (unitSprite != null && data.attackSprite != null)
            unitSprite.sprite = data.attackSprite;

        yield return new WaitForSeconds(0.2f); // Затримка на вистріл

        // 2. Нанесення шкоди
        int damage = CalculateDamageTo(target);
        Debug.Log($"{data.unitName} стріляє в {target.data.unitName} на {damage} шкоди!");
        target.TakeDamage(damage);

        yield return new WaitForSeconds(0.2f);

        // 3. Повернення до Idle
        if (unitSprite != null && data.idleSprite != null)
            unitSprite.sprite = data.idleSprite;

        onComplete?.Invoke();
    }

    public int CalculateDamageTo(BattleUnit target, bool isMeleePenalty = false)
    {
        int baseDamageSum = 0;
        for (int i = 0; i < stackSize; i++)
        {
            baseDamageSum += Random.Range(data.minDamage, data.maxDamage + 1);
        }

        float modifier = 1.0f;

        if (data.attack > target.data.defense)
        {
            float bonus = (data.attack - target.data.defense) * 0.05f;
            modifier += Mathf.Min(bonus, 3.0f);
        }
        else if (data.attack < target.data.defense)
        {
            float penalty = (target.data.defense - data.attack) * 0.025f;
            modifier -= penalty;
            modifier = Mathf.Max(modifier, 0.3f);
        }

        if (isMeleePenalty) modifier *= 0.5f;

        return Mathf.Max(1, Mathf.RoundToInt(baseDamageSum * modifier));
    }

    // Замініть ShowDamageText у BattleUnit.cs
    private void ShowDamageText(int amount)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning($"[BattleUnit] damageTextPrefab не призначений у префабі {gameObject.name}!");
            return;
        }

        // Z = -1f виносить текст вперед перед спрайтами
        Vector3 spawnPos = transform.position + new Vector3(0, 0.8f, -1f);
        GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        FloatingText floatingText = textObj.GetComponent<FloatingText>();
        if (floatingText != null)
        {
            floatingText.Setup(amount);
        }
    }

    // Додайте цей новий метод переміщення у BattleUnit.cs (замість старого MoveToHex)
    public IEnumerator MoveAlongPath(List<GameObject> path, Vector2Int finalCoords, System.Action onComplete)
    {
        foreach (GameObject hexObj in path)
        {
            FaceTarget(hexObj.transform.position);

            Vector3 targetPos = hexObj.transform.position;
            targetPos.z = transform.position.z;

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, 8f * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPos;
        }

        hexCoords = finalCoords;
        onComplete?.Invoke();
    }

    private void Die()
    {
        Debug.Log($"{data.unitName} знищено!");
        Collider2D col = GetComponentInChildren<Collider2D>();
        if (col != null) col.enabled = false;

        if (unitSprite != null) unitSprite.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        gameObject.name += "_Corpse";
    }

    public IEnumerator MoveToHex(GameObject targetHex, Vector2Int newCoords, System.Action onComplete)
    {
        FaceTarget(targetHex.transform.position);

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
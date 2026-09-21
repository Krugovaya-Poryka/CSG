using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Обов'язково додайте для роботи з Image
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

    public bool hasWaited = false;

    [Header("Візуалізація")]
    public SpriteRenderer unitSprite;
    public Transform textureChild;
    public GameObject damageTextPrefab;
    public TMP_Text stackText;

    [Header("Налаштування плашки стеку")]
    public Image stackBgImage; // Посилання на Image плашки (Square)
    public Color playerColor = new Color(0.2f, 0.6f, 1f, 0.85f); // Блакитний колір для гравця
    public Color enemyColor = new Color(0.9f, 0.2f, 0.2f, 0.85f);  // Червоний колір для ворога

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
        if (stackText == null) stackText = GetComponentInChildren<TMP_Text>();

        // Автоматичний пошук Image плашки, якщо її не перетягнули в Інспекторі
        if (stackBgImage == null && stackText != null)
        {
            stackBgImage = stackText.GetComponentInParent<Image>();
        }

        if (unitSprite != null && data != null && data.idleSprite != null)
        {
            unitSprite.sprite = data.idleSprite;
        }

        ApplyTeamColor();
        UpdateStackText();
    }

    private void ApplyTeamColor()
    {
        if (stackBgImage != null)
        {
            // Якщо teamId == 0 — це команда гравця, інакше — ворог
            stackBgImage.color = (teamId == 0) ? playerColor : enemyColor;
        }
    }

    public void UpdateStackText()
    {
        if (stackText != null)
        {
            stackText.text = stackSize.ToString();

            // Якщо стек знищено — ховаємо увесь Canvas / плашку
            if (stackBgImage != null)
            {
                stackBgImage.gameObject.SetActive(stackSize > 0);
            }
            else
            {
                stackText.gameObject.SetActive(stackSize > 0);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // 1. Розрахунок здоров'я та кількості в стеку
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

        // 2. Спавн та запуск FloatingText
        if (damageTextPrefab != null)
        {
            // -0.5f по Z виносить текст трохи ближче до камери
            Vector3 spawnPos = transform.position + new Vector3(0, 1.8f, -0.5f);
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            FloatingText floatText = textObj.GetComponent<FloatingText>();
            if (floatText != null)
            {
                floatText.Setup(damage);
            }
            else
            {
                Debug.LogError($"[TakeDamage] На префабі {damageTextPrefab.name} немає скрипта FloatingText!");
            }
        }
        else
        {
            Debug.LogError($"[TakeDamage] На юніті {gameObject.name} не призначено damageTextPrefab в Інспекторі!");
        }

        // 3. Оновлення UI та перевірка смерті
        UpdateStackText();

        if (stackSize <= 0)
        {
            stackSize = 0;
            Die();
        }
    }

    public void ResetRoundData()
    {
        hasWaited = false;
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
        if (BattleLogUI.Instance != null) BattleLogUI.Instance.LogAttack(this, target, damage);
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
        if (BattleLogUI.Instance != null) BattleLogUI.Instance.LogAttack(this, attacker, retDamage, true);
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
        if (BattleLogUI.Instance != null) BattleLogUI.Instance.LogAttack(this, target, damage);
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
        if (damageTextPrefab == null) return;

        // 1. Шукаємо головний Canvas на сцені
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null) return;

        // 2. Переводимо світову позицію юніта в екранну позицію Canvas
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1.5f, 0));

        // 3. Спавнимо префаб як дочірній об'єкт Canvas
        GameObject textObj = Instantiate(damageTextPrefab, mainCanvas.transform);
        textObj.transform.position = screenPos;

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
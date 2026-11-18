using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    public Image stackBgImage;
    public Color playerColor = new Color(24f / 255f, 113f / 255f, 202f / 255f, 1f);
    public Color enemyColor = new Color(195f / 255f, 19f / 255f, 19f / 255f, 1f);

    public virtual void Init(UnitData unitData, int count, int team)
    {
        data = unitData;
        stackSize = count;
        initialStackSize = count;
        teamId = team;
        currentHealth = data != null ? data.maxHealth : 1;
        currentShots = data != null ? data.maxShots : 0;
        remainingRetaliations = data != null ? data.retaliationsCount : 0;

        if (unitSprite == null) unitSprite = GetComponentInChildren<SpriteRenderer>();
        if (textureChild == null) textureChild = transform.Find("Texture");
        if (stackText == null) stackText = GetComponentInChildren<TMP_Text>();
        if (stackBgImage == null && stackText != null) stackBgImage = stackText.GetComponentInParent<Image>();

        if (unitSprite != null && data != null && data.idleSprite != null)
            unitSprite.sprite = data.idleSprite;

        ApplyTeamColor();
        UpdateStackText();
    }

    protected void ApplyTeamColor()
    {
        if (stackBgImage != null)
            stackBgImage.color = (teamId == 0) ? playerColor : enemyColor;
    }

    public void UpdateStackText()
    {
        if (stackText != null)
        {
            stackText.text = stackSize.ToString();
            if (stackBgImage != null) stackBgImage.gameObject.SetActive(stackSize > 0);
            else stackText.gameObject.SetActive(stackSize > 0);
        }
    }

    public void TakeDamage(int damage)
    {
        int totalDamage = damage;
        int maxHp = (data != null && data.maxHealth > 0) ? data.maxHealth : 1;

        while (totalDamage > 0 && stackSize > 0)
        {
            if (totalDamage < currentHealth) { currentHealth -= totalDamage; totalDamage = 0; }
            else { totalDamage -= currentHealth; stackSize--; currentHealth = maxHp; }
        }

        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 1.8f, -0.5f);
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
            FloatingText floatText = textObj.GetComponent<FloatingText>();
            if (floatText != null) floatText.Setup(damage);
        }

        UpdateStackText();
        if (stackSize <= 0) { stackSize = 0; Die(); }
    }

    public void ResetRoundData()
    {
        hasWaited = false;
        remainingRetaliations = data != null ? data.retaliationsCount : 0;
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
                scale.x = (deltaX < 0) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                textureChild.localScale = scale;
            }
        }
    }

    public virtual bool CanAttackWithoutMoving(BattleUnit target, int distance, bool isBlockedByEnemy)
    {
        return distance == 1;
    }

    public virtual void PerformAttack(BattleUnit target, System.Action onComplete)
    {
        onComplete?.Invoke();
    }

    public IEnumerator RetaliateRoutine(BattleUnit attacker)
    {
        FaceTarget(attacker.transform.position);
        if (unitSprite != null && data != null && data.attackSprite != null) unitSprite.sprite = data.attackSprite;
        yield return new WaitForSeconds(0.15f);

        int retDamage = CalculateDamageTo(attacker);
        if (BattleLogUI.Instance != null) BattleLogUI.Instance.LogAttack(this, attacker, retDamage, true);
        attacker.TakeDamage(retDamage);
        remainingRetaliations--;

        yield return new WaitForSeconds(0.25f);
        if (unitSprite != null && data != null && data.idleSprite != null) unitSprite.sprite = data.idleSprite;
    }

    public int CalculateDamageTo(BattleUnit target, bool isMeleePenalty = false)
    {
        if (data == null || target == null || target.data == null) return 1;

        int baseDamageSum = 0;
        for (int i = 0; i < stackSize; i++) baseDamageSum += Random.Range(data.minDamage, data.maxDamage + 1);

        float modifier = 1.0f;
        if (data.attack > target.data.defense) modifier += Mathf.Min((data.attack - target.data.defense) * 0.05f, 3.0f);
        else if (data.attack < target.data.defense) modifier = Mathf.Max(modifier - ((target.data.defense - data.attack) * 0.025f), 0.3f);

        if (isMeleePenalty) modifier *= 0.5f;
        return Mathf.Max(1, Mathf.RoundToInt(baseDamageSum * modifier));
    }

    public IEnumerator MoveAlongPath(List<GameObject> path, Vector2Int finalCoords, System.Action onComplete)
    {
        foreach (GameObject hexObj in path)
        {
            if (hexObj == null) continue;
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
        Collider2D col = GetComponentInChildren<Collider2D>();
        if (col != null) col.enabled = false;
        if (unitSprite != null) unitSprite.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        gameObject.name += "_Corpse";
    }

    public void RestoreAliveState(int newStackSize, int newHealth, Vector2Int newCoords)
    {
        hexCoords = newCoords;
        stackSize = Mathf.Max(0, newStackSize);
        currentHealth = (data != null && data.maxHealth > 0)
            ? Mathf.Clamp(newHealth, stackSize > 0 ? 1 : 0, data.maxHealth)
            : Mathf.Max(0, newHealth);

        if (gameObject.name.EndsWith("_Corpse"))
        {
            gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - "_Corpse".Length);
        }

        Collider2D[] cols = GetComponentsInChildren<Collider2D>(true);
        foreach (var col in cols)
        {
            if (col != null) col.enabled = (stackSize > 0);
        }

        if (unitSprite != null)
        {
            unitSprite.color = (stackSize > 0) ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }

        if (stackSize > 0 && data != null && data.idleSprite != null && unitSprite != null)
        {
            unitSprite.sprite = data.idleSprite;
        }

        ApplyTeamColor();
        UpdateStackText();
    }
}
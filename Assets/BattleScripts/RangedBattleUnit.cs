using System.Collections;
using UnityEngine;

public class RangedBattleUnit : BattleUnit
{
    public override bool CanAttackWithoutMoving(BattleUnit target, int distance, bool isBlockedByEnemy)
    {
        // Може атакувати без руху, якщо ворог упритул 
        // АБО якщо є патрони і юніт не заблокований ворогом упритул
        if (distance == 1) return true;

        return currentShots > 0 && !isBlockedByEnemy;
    }

    public override void PerformAttack(BattleUnit target, System.Action onComplete)
    {
        bool isAdjacent = HexUtils.GetHexDistance(hexCoords, target.hexCoords) == 1;

        if (currentShots > 0 && !isAdjacent)
        {
            StartCoroutine(RangedAttackRoutine(target, onComplete));
        }
        else
        {
            StartCoroutine(MeleeFallbackRoutine(target, onComplete));
        }
    }

    private IEnumerator RangedAttackRoutine(BattleUnit target, System.Action onComplete)
    {
        FaceTarget(target.transform.position);
        currentShots--;

        if (unitSprite != null && data.attackSprite != null)
            unitSprite.sprite = data.attackSprite;

        yield return new WaitForSeconds(0.2f);

        int damage = CalculateDamageTo(target, false);
        if (BattleLogUI.Instance != null) BattleLogUI.Instance.LogAttack(this, target, damage);
        target.TakeDamage(damage);

        yield return new WaitForSeconds(0.2f);

        if (unitSprite != null && data.idleSprite != null)
            unitSprite.sprite = data.idleSprite;

        onComplete?.Invoke();
    }

    private IEnumerator MeleeFallbackRoutine(BattleUnit target, System.Action onComplete)
    {
        FaceTarget(target.transform.position);
        if (unitSprite != null && data.attackSprite != null)
            unitSprite.sprite = data.attackSprite;

        yield return new WaitForSeconds(0.15f);

        int damage = CalculateDamageTo(target, true);
        if (BattleLogUI.Instance != null) BattleLogUI.Instance.LogAttack(this, target, damage);
        target.TakeDamage(damage);

        yield return new WaitForSeconds(0.25f);
        if (unitSprite != null && data.idleSprite != null)
            unitSprite.sprite = data.idleSprite;

        if (target.stackSize > 0 && target.remainingRetaliations > 0)
        {
            yield return target.RetaliateRoutine(this);
        }

        onComplete?.Invoke();
    }
}
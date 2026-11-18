using System.Collections;
using UnityEngine;

public class MeleeBattleUnit : BattleUnit
{
    public override void PerformAttack(BattleUnit target, System.Action onComplete)
    {
        StartCoroutine(MeleeAttackRoutine(target, onComplete));
    }

    private IEnumerator MeleeAttackRoutine(BattleUnit target, System.Action onComplete)
    {
        FaceTarget(target.transform.position);

        if (unitSprite != null && data.attackSprite != null)
            unitSprite.sprite = data.attackSprite;

        yield return new WaitForSeconds(0.15f);

        int damage = CalculateDamageTo(target, false);
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
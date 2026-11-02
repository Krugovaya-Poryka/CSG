using System.Collections.Generic;

/// <summary>
/// ЛР3: ієрархія стратегій.
///
///   AIStrategy (abstract)
///    ├── ClosestEnemyStrategy        б'є найближчого (поведінка ЛР1/ЛР2 за замовчуванням)
///    │    ├── DefensiveStrategy      та сама ціль, але відступає раніше (override ShouldRetreat)
///    │    └── FocusFireStrategy      спершу стрільці (використовує base.ChooseTarget)
///    └── AggressiveStrategy          б'є найслабшого, відступає пізно
/// </summary>
public class ClosestEnemyStrategy : AIStrategy
{
    public override string Name => "Closest";

    public override TargetCandidate ChooseTarget(List<TargetCandidate> candidates)
    {
        if (candidates == null) return null;

        TargetCandidate best = null;
        foreach (TargetCandidate c in candidates)
        {
            if (best == null || c.Distance < best.Distance) best = c;
        }
        return best;
    }
}

public class DefensiveStrategy : ClosestEnemyStrategy
{
    public override string Name => "Defensive";

    // Ціль обирається успадкованим кодом; змінюється лише готовність відступати
    public override bool ShouldRetreat(float forceRatio, float baseThreshold)
    {
        return forceRatio < baseThreshold * 1.5f;
    }
}

public class FocusFireStrategy : ClosestEnemyStrategy
{
    public override string Name => "FocusFire";

    public override TargetCandidate ChooseTarget(List<TargetCandidate> candidates)
    {
        if (candidates == null || candidates.Count == 0) return null;

        List<TargetCandidate> ranged = candidates.FindAll(c => c.IsRanged);

        // Є стрільці: серед них найближчий; немає: серед усіх найближчий
        return base.ChooseTarget(ranged.Count > 0 ? ranged : candidates);
    }
}

public class AggressiveStrategy : AIStrategy
{
    public override string Name => "Aggressive";

    public override TargetCandidate ChooseTarget(List<TargetCandidate> candidates)
    {
        if (candidates == null) return null;

        TargetCandidate best = null;
        foreach (TargetCandidate c in candidates)
        {
            if (best == null
                || c.Power < best.Power
                || (c.Power == best.Power && c.Distance < best.Distance))
            {
                best = c;
            }
        }
        return best;
    }

    public override bool ShouldRetreat(float forceRatio, float baseThreshold)
    {
        return forceRatio < baseThreshold * 0.5f;
    }
}

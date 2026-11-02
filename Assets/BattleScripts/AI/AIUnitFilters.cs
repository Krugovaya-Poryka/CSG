using System;

/// <summary>
/// ЛР3: ієрархія фільтрів для пошуку ШІ-юнітів. Різні алгоритми фільтрації
/// працюють через один виклик AIManager.FindAI(filter).
/// </summary>
public abstract class AIUnitFilter
{
    public abstract string Description { get; }
    public abstract bool Matches(AIUnit unit);
}

public class DifficultyFilter : AIUnitFilter
{
    private readonly AIManager.AIDifficulty difficulty;

    public DifficultyFilter(AIManager.AIDifficulty difficulty) { this.difficulty = difficulty; }

    public override string Description => $"складність = {difficulty}";
    public override bool Matches(AIUnit unit) => unit.Difficulty == difficulty;
}

public class NameContainsFilter : AIUnitFilter
{
    private readonly string text;

    public NameContainsFilter(string text) { this.text = text ?? string.Empty; }

    public override string Description => $"ім'я містить '{text}'";

    public override bool Matches(AIUnit unit)
    {
        return unit.UnitName != null
            && unit.UnitName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

public class StrategyFilter : AIUnitFilter
{
    private readonly string strategyName;

    public StrategyFilter(string strategyName) { this.strategyName = strategyName; }

    public override string Description => $"стратегія = {strategyName}";

    public override bool Matches(AIUnit unit)
    {
        return unit.Strategy != null && unit.Strategy.Name == strategyName;
    }
}

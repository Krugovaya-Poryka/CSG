using System.Collections.Generic;

/// <summary>
/// ЛР3: кандидат на ціль. Звичайний клас з простих даних, тому стратегії
/// не залежать від BattleUnit чи сітки й тестуються окремо.
/// Unit може бути null (у тестах).
/// </summary>
public class TargetCandidate
{
    public BattleUnit Unit { get; }
    public string Name { get; }
    public int Distance { get; }
    public int Power { get; }
    public bool IsRanged { get; }

    public TargetCandidate(BattleUnit unit, string name, int distance, int power, bool isRanged)
    {
        Unit = unit;
        Name = name;
        Distance = distance;
        Power = power;
        IsRanged = isRanged;
    }
}

/// <summary>
/// ЛР3: абстрактний базовий клас стратегії ШІ. КОРІНЬ ієрархії успадкування.
///   ChooseTarget  - abstract: кожен нащадок ОБОВ'ЯЗКОВО реалізує свій алгоритм вибору цілі.
///   ShouldRetreat - virtual: типова поведінка, яку нащадок МОЖЕ перевизначити.
/// Поліморфізм: AIUnit викликає strategy.ChooseTarget(...) і не знає, який саме нащадок усередині.
/// </summary>
public abstract class AIStrategy
{
    public abstract string Name { get; }

    public abstract TargetCandidate ChooseTarget(List<TargetCandidate> candidates);

    // forceRatio: наша сила / сила ворога; baseThreshold: поріг зі складності (AIProfile)
    public virtual bool ShouldRetreat(float forceRatio, float baseThreshold)
    {
        return forceRatio < baseThreshold;
    }

    // Відновлення стратегії за назвою (потрібне при завантаженні збереження)
    public static AIStrategy FromName(string name)
    {
        switch (name)
        {
            case "Aggressive": return new AggressiveStrategy();
            case "Defensive": return new DefensiveStrategy();
            case "FocusFire": return new FocusFireStrategy();
            default: return new ClosestEnemyStrategy();
        }
    }
}

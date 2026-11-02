using System;
using System.Collections.Generic;

/// <summary>
/// ЛР3: ієрархія критеріїв сортування ШІ-юнітів. Базовий клас реалізує IComparer,
/// нащадки задають порядок. Один виклик AIManager.GetAISorted(sorter), різний результат.
/// </summary>
public abstract class AIUnitSorter : IComparer<AIUnit>
{
    public abstract string Description { get; }
    public abstract int Compare(AIUnit x, AIUnit y);

    protected static int CompareNames(AIUnit x, AIUnit y)
    {
        return string.Compare(x.UnitName, y.UnitName, StringComparison.Ordinal);
    }
}

public class ByNameSorter : AIUnitSorter
{
    public override string Description => "за ім'ям";
    public override int Compare(AIUnit x, AIUnit y) => CompareNames(x, y);
}

public class ByDifficultySorter : AIUnitSorter
{
    public override string Description => "за складністю, потім за ім'ям";

    public override int Compare(AIUnit x, AIUnit y)
    {
        int c = x.Difficulty.CompareTo(y.Difficulty);
        return c != 0 ? c : CompareNames(x, y);
    }
}

public class ByStrategySorter : AIUnitSorter
{
    public override string Description => "за назвою стратегії, потім за ім'ям";

    public override int Compare(AIUnit x, AIUnit y)
    {
        int c = string.Compare(x.Strategy.Name, y.Strategy.Name, StringComparison.Ordinal);
        return c != 0 ? c : CompareNames(x, y);
    }
}

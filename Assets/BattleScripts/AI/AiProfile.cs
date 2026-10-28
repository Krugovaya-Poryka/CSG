/// <summary>
/// ЛР2: профіль поведінки ШІ для певної складності.
/// Звичайний C#-клас (не MonoBehaviour), тому має справжній конструктор.
/// Незмінний (immutable): лише get, без set. Саме тому один профіль безпечно
/// ділити між кількома гравцями (агрегація: AIPlayer o-- AIProfile).
/// </summary>
public class AIProfile
{
    public AIManager.AIDifficulty Difficulty { get; }
    public float RetreatThreshold { get; }

    public AIProfile(AIManager.AIDifficulty difficulty)
    {
        Difficulty = difficulty;
        RetreatThreshold = GetRetreatThreshold(difficulty);
    }

    // Єдине місце, де записані пороги відступу (використовує і AIUnit.ShouldRetreat)
    public static float GetRetreatThreshold(AIManager.AIDifficulty difficulty)
    {
        switch (difficulty)
        {
            case AIManager.AIDifficulty.Easy: return 0.25f;
            case AIManager.AIDifficulty.Hard: return 0.75f;
            default: return 0.5f;
        }
    }
}
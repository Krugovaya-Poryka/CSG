/// <summary>
/// ЛР2: один запис журналу рішень ШІ. Не має сенсу без свого AIPlayer:
/// створюється всередині AIPlayer і знищується разом із ним (композиція).
/// Незмінний: після створення поля змінити не можна.
/// </summary>
public class AIDecisionRecord
{
    public int Turn { get; }
    public string UnitName { get; }
    public string Action { get; }
    public string Timestamp { get; }

    // Новий запис: час ставиться автоматично
    public AIDecisionRecord(int turn, string unitName, string action)
        : this(turn, unitName, action, System.DateTime.Now.ToString("HH:mm:ss"))
    {
    }

    // ЛР3: відновлення запису зі збереження (час береться зі збереженого)
    public AIDecisionRecord(int turn, string unitName, string action, string timestamp)
    {
        Turn = turn;
        UnitName = unitName;
        Action = action;
        Timestamp = timestamp;
    }

    public string Describe()
    {
        return $"[{Timestamp}] T{Turn} {UnitName}: {Action}";
    }
}
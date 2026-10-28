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

	public AIDecisionRecord(int turn, string unitName, string action)
	{
		Turn = turn;
		UnitName = unitName;
		Action = action;
		Timestamp = System.DateTime.Now.ToString("HH:mm:ss");
	}

	public string Describe()
	{
		return $"[{Timestamp}] T{Turn} {UnitName}: {Action}";
	}
}
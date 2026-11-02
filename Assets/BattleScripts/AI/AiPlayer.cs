using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ЛР2: ШІ-гравець, «ціле» в ієрархії.
///   КОМПОЗИЦІЯ: AIPlayer *-- AIUnit (сам створює й знищує юнітів)
///   КОМПОЗИЦІЯ: AIPlayer *-- AIDecisionRecord (журнал рішень)
///   АГРЕГАЦІЯ:  AIPlayer o-- AIProfile (профіль отримує ззовні, ділиться з іншими гравцями)
///   АГРЕГАЦІЯ:  AIPlayer o-- AIStrategy (ЛР3: стратегію отримує ззовні, її ділять юніти гравця)
/// Звичайний C#-клас: має конструктор, не прив'язаний до GameObject.
/// </summary>
public class AIPlayer
{
    public string Name { get; }
    public AIProfile Profile { get; }   // агрегація: чужий (спільний) об'єкт
    public AIStrategy Strategy { get; } // ЛР3: поліморфна поведінка гравця

    // Колекції-частини. List: важливий порядок, кількість мала, потрібен обхід.
    private readonly List<AIUnit> units = new List<AIUnit>();
    private readonly List<AIDecisionRecord> history = new List<AIDecisionRecord>();
    private int totalCreated = 0;

    public int UnitCount => units.Count;
    public int HistoryCount => history.Count;
    public int TotalCreated => totalCreated;

    public AIPlayer(string name, AIProfile profile) : this(name, profile, null)
    {
    }

    // strategy == null: типова стратегія ClosestEnemyStrategy (поведінка ЛР1/ЛР2)
    public AIPlayer(string name, AIProfile profile, AIStrategy strategy)
    {
        Name = name;
        Profile = profile ?? throw new System.ArgumentNullException(nameof(profile));
        Strategy = strategy ?? new ClosestEnemyStrategy();
    }

    // ---------- Юніти (композиція) ----------
    public AIUnit CreateUnit(string unitName, Vector3 startPosition)
    {
        return CreateUnit(unitName, startPosition, Profile.Difficulty);
    }

    public AIUnit CreateUnit(string unitName, Vector3 startPosition, AIManager.AIDifficulty difficulty)
    {
        if (string.IsNullOrEmpty(unitName))
        {
            Debug.LogError($"[-] Player '{Name}': unit name must not be empty!");
            return null;
        }

        if (GetUnit(unitName) != null)
        {
            Debug.LogError($"[-] Player '{Name}': unit '{unitName}' already exists!");
            return null;
        }

        GameObject go = new GameObject($"AI_{Name}_{unitName}");
        go.transform.position = startPosition;

        AIUnit unit = go.AddComponent<AIUnit>();
        unit.Initialize(unitName, difficulty);
        unit.SetStrategy(Strategy);   // ЛР3: юніт діє за стратегією свого гравця

        units.Add(unit);
        totalCreated++;

        Debug.Log($"[+] Player '{Name}': unit '{unitName}' ({difficulty}) created");
        return unit;
    }

    public bool RemoveUnit(string unitName)
    {
        AIUnit unit = GetUnit(unitName);
        if (unit == null) return false;

        units.Remove(unit);
        Object.Destroy(unit.gameObject);   // частина гине, коли її прибирає власник
        Debug.Log($"[-] Player '{Name}': unit '{unitName}' removed");
        return true;
    }

    // Тихий пошук (без попереджень): повертає null, якщо юніта немає
    public AIUnit GetUnit(string unitName)
    {
        return units.Find(u => u != null && u.UnitName == unitName);
    }

    // Копія списку: зовнішній код не зможе змінити внутрішню колекцію
    public List<AIUnit> GetUnits()
    {
        return new List<AIUnit>(units);
    }

    // ---------- Журнал рішень (композиція) ----------
    // Приймаємо прості дані, а сам запис створюємо тут: власник створює свої частини
    public void RecordDecision(int turn, string unitName, string action)
    {
        history.Add(new AIDecisionRecord(turn, unitName, action));
    }

    // ЛР3: відновлення запису зі збереженим часом
    public void RecordDecision(int turn, string unitName, string action, string timestamp)
    {
        history.Add(new AIDecisionRecord(turn, unitName, action, timestamp));
    }

    public List<AIDecisionRecord> GetHistory()
    {
        return new List<AIDecisionRecord>(history);
    }

    // ---------- Знищення цілого => знищення частин ----------
    public void DestroyAll()
    {
        int count = units.Count;

        foreach (AIUnit unit in units)
        {
            if (unit != null) Object.Destroy(unit.gameObject);
        }

        units.Clear();
        history.Clear();
        // Profile НЕ чіпаємо: він не належить гравцеві (агрегація)

        Debug.Log($"[-] Player '{Name}' destroyed: {count} units and history removed, profile kept");
    }
}
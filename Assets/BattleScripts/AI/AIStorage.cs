using System;
using System.Collections.Generic;
using System.IO;

// ---------- Дані для збереження (прості Serializable-класи з public-полями) ----------
[Serializable]
public class UnitSaveData
{
    public string name;
    public float x, y, z;
    public int difficulty;
}

[Serializable]
public class RecordSaveData
{
    public int turn;
    public string unitName;
    public string action;
    public string timestamp;
}

[Serializable]
public class PlayerSaveData
{
    public string name;
    public int difficulty;
    public string strategy;
    public List<UnitSaveData> units = new List<UnitSaveData>();
    public List<RecordSaveData> history = new List<RecordSaveData>();
}

[Serializable]
public class AISaveData
{
    public List<PlayerSaveData> players = new List<PlayerSaveData>();
}

/// <summary>
/// ЛР3: абстрактне сховище стану ШІ. Save/Load однакові для всіх,
/// а формат (Serialize/Deserialize) визначає нащадок: JSON або власний текстовий.
/// Deserialize кидає виняток на пошкоджених даних, тому поточний стан гри не псується.
/// </summary>
public abstract class AIStorage
{
    public abstract string Name { get; }
    public abstract string FileExtension { get; }

    protected abstract string Serialize(AISaveData data);
    protected abstract AISaveData Deserialize(string text);

    public void Save(AISaveData data, string path)
    {
        File.WriteAllText(path, Serialize(data));
    }

    // null, якщо файлу немає
    public AISaveData Load(string path)
    {
        if (!File.Exists(path)) return null;
        return Deserialize(File.ReadAllText(path));
    }
}

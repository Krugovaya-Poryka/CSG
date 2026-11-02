using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// ЛР3: збереження у форматі JSON (через вбудований JsonUtility Unity).
/// </summary>
public class JsonAIStorage : AIStorage
{
    public override string Name => "JSON";
    public override string FileExtension => ".json";

    protected override string Serialize(AISaveData data)
    {
        return JsonUtility.ToJson(data, true);
    }

    protected override AISaveData Deserialize(string text)
    {
        // Простий захист: у нашому JSON завжди є поле "players"
        if (string.IsNullOrWhiteSpace(text) || !text.Contains("\"players\""))
            throw new FormatException("Невірний JSON збереження");

        AISaveData data = JsonUtility.FromJson<AISaveData>(text);
        if (data == null || data.players == null) throw new FormatException("Невірний JSON збереження");
        return data;
    }
}

/// <summary>
/// ЛР3: збереження у власному текстовому форматі, по рядку на запис:
///   P|ім'я|складність|стратегія
///   U|ім'я|x|y|z|складність       (належить останньому P)
///   H|хід|юніт|дія|час            (належить останньому P)
/// Текстові поля кодуються Uri.EscapeDataString, щоб '|' в іменах не ламав формат.
/// </summary>
public class TextAIStorage : AIStorage
{
    public override string Name => "Text";
    public override string FileExtension => ".txt";

    private static string Esc(string s) => Uri.EscapeDataString(s ?? string.Empty);
    private static string Unesc(string s) => Uri.UnescapeDataString(s);
    private static string F(float v) => v.ToString("R", CultureInfo.InvariantCulture);

    protected override string Serialize(AISaveData data)
    {
        StringBuilder sb = new StringBuilder();
        foreach (PlayerSaveData p in data.players)
        {
            sb.Append($"P|{Esc(p.name)}|{p.difficulty}|{Esc(p.strategy)}\n");
            foreach (UnitSaveData u in p.units)
                sb.Append($"U|{Esc(u.name)}|{F(u.x)}|{F(u.y)}|{F(u.z)}|{u.difficulty}\n");
            foreach (RecordSaveData r in p.history)
                sb.Append($"H|{r.turn}|{Esc(r.unitName)}|{Esc(r.action)}|{Esc(r.timestamp)}\n");
        }
        return sb.ToString();
    }

    protected override AISaveData Deserialize(string text)
    {
        AISaveData data = new AISaveData();
        PlayerSaveData current = null;
        string[] lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string raw in lines)
        {
            string[] f = raw.TrimEnd('\r').Split('|');
            try
            {
                if (f[0] == "P" && f.Length == 4)
                {
                    current = new PlayerSaveData
                    {
                        name = Unesc(f[1]),
                        difficulty = int.Parse(f[2], CultureInfo.InvariantCulture),
                        strategy = Unesc(f[3])
                    };
                    data.players.Add(current);
                }
                else if (f[0] == "U" && f.Length == 6 && current != null)
                {
                    current.units.Add(new UnitSaveData
                    {
                        name = Unesc(f[1]),
                        x = float.Parse(f[2], CultureInfo.InvariantCulture),
                        y = float.Parse(f[3], CultureInfo.InvariantCulture),
                        z = float.Parse(f[4], CultureInfo.InvariantCulture),
                        difficulty = int.Parse(f[5], CultureInfo.InvariantCulture)
                    });
                }
                else if (f[0] == "H" && f.Length == 5 && current != null)
                {
                    current.history.Add(new RecordSaveData
                    {
                        turn = int.Parse(f[1], CultureInfo.InvariantCulture),
                        unitName = Unesc(f[2]),
                        action = Unesc(f[3]),
                        timestamp = Unesc(f[4])
                    });
                }
                else
                {
                    throw new FormatException("невідомий або неповний рядок");
                }
            }
            catch (FormatException) { throw new FormatException($"Пошкоджений рядок збереження: '{raw}'"); }
            catch (OverflowException) { throw new FormatException($"Пошкоджений рядок збереження: '{raw}'"); }
        }
        return data;
    }
}

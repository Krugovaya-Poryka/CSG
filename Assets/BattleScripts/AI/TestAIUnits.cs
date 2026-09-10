using NUnit.Framework;
using UnityEngine;

public class TestAIUnit
{
    private AIUnit aiUnit;

    [SetUp]
    public void Setup()
    {
        GameObject go = new GameObject("TestAI");
        aiUnit = go.AddComponent<AIUnit>();

        aiUnit.Initialize("TestAI", AIManager.AIDifficulty.Normal, Vector3.zero);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(aiUnit.gameObject);
    }

    [Test]
    public void TestAIInitialization()
    {

        string name = aiUnit.GetName();

        Assert.AreEqual("TestAI", name, "Ім'я AI повинно бути 'TestAI'");
    }

    [Test]
    public void TestAIIsAliveAfterInitialization()
    {
        bool isAlive = aiUnit.IsAlive();

        Assert.IsTrue(isAlive, "AI повинна бути живою після ініціалізації");
    }

    [Test]
    public void TestHealthByDifficulty()
    {
        GameObject easyAI = new GameObject("EasyAI");
        AIUnit easyUnit = easyAI.AddComponent<AIUnit>();
        easyUnit.Initialize("EasyAI", AIManager.AIDifficulty.Easy, Vector3.zero);

        Assert.AreEqual(60, easyUnit.GetMaxHealth(), "Easy рівень повинен мати 60 HP");

        GameObject hardAI = new GameObject("HardAI");
        AIUnit hardUnit = hardAI.AddComponent<AIUnit>();
        hardUnit.Initialize("HardAI", AIManager.AIDifficulty.Hard, Vector3.zero);

        Assert.AreEqual(150, hardUnit.GetMaxHealth(), "Hard рівень повинен мати 150 HP");

        Object.Destroy(easyAI);
        Object.Destroy(hardAI);
    }

    [Test]
    public void TestTakeDamage()
    {
        int initialHealth = aiUnit.GetHealth();
        int damageAmount = 20;

        aiUnit.TakeDamage(damageAmount);

        Assert.AreEqual(initialHealth - damageAmount, aiUnit.GetHealth(), 
            "Здоров'я повинно зменшитися на розмір шкоди");
    }

    [Test]
    public void TestHeal()
    {
        aiUnit.TakeDamage(30);
        int healthAfterDamage = aiUnit.GetHealth();
        int healAmount = 10;

        aiUnit.Heal(healAmount);

        Assert.AreEqual(healthAfterDamage + healAmount, aiUnit.GetHealth(),
            "Здоров'я повинно збільшитися на розмір лікування");
    }

    [Test]
    public void TestHealDoesNotExceedMaxHealth()
    {
        int maxHealth = aiUnit.GetMaxHealth();
        aiUnit.TakeDamage(10);
      
        aiUnit.Heal(100);

        Assert.AreEqual(maxHealth, aiUnit.GetHealth(),
            "Здоров'я не повинно перевищувати максимальне значення");
    }

    [Test]
    public void TestAIDeath()
    {
        int maxHealth = aiUnit.GetMaxHealth();

        aiUnit.TakeDamage(maxHealth + 10);

        Assert.IsFalse(aiUnit.IsAlive(), "AI повинна бути мертвою після отримання смертельної шкоди");
    }

    [Test]
    public void TestCanSeeTargetInRange()
    {
        Vector3 aiPosition = aiUnit.GetPosition();
        Vector3 targetPosition = aiPosition + Vector3.forward * 10f; 

        bool canSee = aiUnit.CanSeeTarget(targetPosition);

        Assert.IsTrue(canSee, "AI повинна бачити ворога в радіусі видення");
    }

    [Test]
    public void TestCannotSeeTargetOutOfRange()
    {
        Vector3 aiPosition = aiUnit.GetPosition();
        Vector3 targetPosition = aiPosition + Vector3.forward * 100f; // 100 одиниць - занадто далеко

        bool canSee = aiUnit.CanSeeTarget(targetPosition);

        Assert.IsFalse(canSee, "AI не повинна бачити ворога поза радіусом видення");
    }

    [Test]
    public void TestChooseClosestTarget()
    {
        Vector3 aiPosition = aiUnit.GetPosition();
        Vector3[] enemies = new Vector3[]
        {
            aiPosition + Vector3.forward * 10f,
            aiPosition + Vector3.forward * 5f,
            aiPosition + Vector3.forward * 20f
        };

        Vector3 selectedTarget = aiUnit.ChooseTarget(enemies);

        Assert.AreEqual(enemies[1], selectedTarget, "Повинна бути обрана найближча ціль");
    }

    [Test]
    public void TestChooseTargetWithOneEnemy()
    {
        Vector3 aiPosition = aiUnit.GetPosition();
        Vector3 enemy = aiPosition + Vector3.forward * 15f;
        Vector3[] enemies = new Vector3[] { enemy };

        Vector3 selectedTarget = aiUnit.ChooseTarget(enemies);

        Assert.AreEqual(enemy, selectedTarget, "Повинна бути обрана єдиний ворог");
    }

    [Test]
    public void TestAttackInRange()
    {
        Vector3 aiPosition = aiUnit.GetPosition();
        Vector3 targetPosition = aiPosition + Vector3.forward * 3f; 
        
        int damage = aiUnit.Attack(targetPosition);

        Assert.Greater(damage, 0, "Атака повинна нанести шкоду > 0");
    }

    [Test]
    public void TestAttackOutOfRange()
    {
        Vector3 aiPosition = aiUnit.GetPosition();
        Vector3 targetPosition = aiPosition + Vector3.forward * 100f;

        int damage = aiUnit.Attack(targetPosition);

        Assert.AreEqual(0, damage, "Атака поза діапазоном не повинна наносити шкоду");
    }

    [Test]
    public void TestAttackDamageRange()
    {
        Vector3 aiPosition = aiUnit.GetPosition();
        Vector3 targetPosition = aiPosition + Vector3.forward * 3f;
        int baseDamage = aiUnit.GetAttackDamage();

        int damage = aiUnit.Attack(targetPosition);

        Assert.IsTrue(damage >= baseDamage - 2 && damage <= baseDamage + 2,
            $"Шкода повинна бути в діапазоні [{baseDamage - 2}, {baseDamage + 2}], отримано {damage}");
    }

    [Test]
    public void TestTurnManagement()
    {
        int initialTurns = aiUnit.GetTurnCount();

        aiUnit.StartTurn();
        bool isTakingTurn = aiUnit.IsTakingTurn();
   
        Assert.IsTrue(isTakingTurn, "AI повинна робити хід після StartTurn()");

        aiUnit.EndTurn();
        isTakingTurn = aiUnit.IsTakingTurn();

        Assert.IsFalse(isTakingTurn, "AI не повинна робити хід після EndTurn()");
 
        Assert.AreEqual(initialTurns + 1, aiUnit.GetTurnCount(), "Лічильник ходів повинен збільшитися");
    }

    [Test]
    public void TestDeadAICannotTakeTurn()
    {
        int maxHealth = aiUnit.GetMaxHealth();
        aiUnit.TakeDamage(maxHealth + 10);

        aiUnit.StartTurn();
        bool isTakingTurn = aiUnit.IsTakingTurn();
      
        Assert.IsFalse(isTakingTurn, "Мертва AI не повинна робити хід");
    }

    [Test]
    public void TestGetStatus()
    {
        string status = aiUnit.GetStatus();
        
        Assert.IsNotEmpty(status, "Статус не повинен бути пустим");
        Assert.Contains("TestAI", status, "Статус повинен містити ім'я AI");
        Assert.Contains("100", status, "Статус повинен містити здоров'я");
    }
    
    [Test]
    public void TestGetPosition()
    {
        Vector3 position = aiUnit.GetPosition();
        
        Assert.AreEqual(Vector3.zero, position, "Позиція AI повинна бути Vector3.zero");
    }
}

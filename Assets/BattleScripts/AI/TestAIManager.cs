using NUnit.Framework;
using UnityEngine;

public class TestAIManager
{
    private AIManager manager;

    [SetUp]
    public void Setup()
    {
        manager = AIManager.Instance;

        var allAI = manager.GetAllActiveAI();
        foreach (var ai in allAI)
        {
            manager.DestroyAI(ai.GetName());
        }
    }

    [TearDown]
    public void TearDown()
    {
        var allAI = manager.GetAllActiveAI();
        foreach (var ai in allAI)
        {
            manager.DestroyAI(ai.GetName());
        }
    }

    [Test]
    public void TestSingletonPattern()
    {
        AIManager instance1 = AIManager.Instance;
        AIManager instance2 = AIManager.Instance;
        
        Assert.AreEqual(instance1, instance2, "AIManager повинна повертати один і той же екземпляр (Singleton)");
    }

    [Test]
    public void TestSetDifficulty()
    {
        AIManager.AIDifficulty difficulty = AIManager.AIDifficulty.Hard;

        manager.SetDifficulty(difficulty);
        AIManager.AIDifficulty result = manager.GetDifficulty();
     
        Assert.AreEqual(difficulty, result, "Складність повинна бути встановлена правильно");
    }

    [Test]
    public void TestAICreatedWithCurrentDifficulty()
    {
        manager.SetDifficulty(AIManager.AIDifficulty.Hard);

        AIUnit ai = manager.CreateAI("HardEnemy", Vector3.zero);
  
        Assert.IsNotNull(ai, "AI повинна бути створена");
        Assert.AreEqual(150, ai.GetMaxHealth(), "Hard рівень повинен мати 150 HP");
    }

    [Test]
    public void TestCreateAI()
    {
        AIUnit ai = manager.CreateAI("TestEnemy", new Vector3(5, 0, 0));
       
        Assert.IsNotNull(ai, "CreateAI повинна повертати AIUnit");
        Assert.AreEqual("TestEnemy", ai.GetName(), "Ім'я AI повинно бути правильним");
        Assert.AreEqual(new Vector3(5, 0, 0), ai.GetPosition(), "Позиція AI повинна бути правильною");
    }
  
    [Test]
    public void TestCannotCreateDuplicateAI()
    {
        manager.CreateAI("Enemy1", Vector3.zero);
  
        AIUnit duplicate = manager.CreateAI("Enemy1", Vector3.zero);
      
        Assert.IsNull(duplicate, "Не повинна бути створена AI з дублікатним ім'ям");
    }

    [Test]
    public void TestDestroyAI()
    {
        manager.CreateAI("Enemy1", Vector3.zero);
        Assert.AreEqual(1, manager.GetActiveAICount(), "Повинна бути 1 активна AI");
    
        manager.DestroyAI("Enemy1");
       
        Assert.AreEqual(0, manager.GetActiveAICount(), "Не повинно бути активних AI");
    }
  
    [Test]
    public void TestGetAI()
    {
        AIUnit created = manager.CreateAI("Enemy1", Vector3.zero);
       
        AIUnit retrieved = manager.GetAI("Enemy1");
      
        Assert.AreEqual(created, retrieved, "GetAI повинна повертати правильну AI");
    }
  
    [Test]
    public void TestGetNonExistentAI()
    {
        AIUnit ai = manager.GetAI("NonExistent");
       
        Assert.IsNull(ai, "GetAI повинна повертати null для неіснуючої AI");
    }

    [Test]
    public void TestGetActiveAICount()
    {
        Assert.AreEqual(0, manager.GetActiveAICount(), "Спочатку повинно бути 0 AI");
       
        manager.CreateAI("Enemy1", Vector3.zero);
        Assert.AreEqual(1, manager.GetActiveAICount(), "Повинна бути 1 активна AI");
        
        manager.CreateAI("Enemy2", Vector3.zero);
        Assert.AreEqual(2, manager.GetActiveAICount(), "Повинно бути 2 активні AI");

        manager.DestroyAI("Enemy1");

        Assert.AreEqual(1, manager.GetActiveAICount(), "Повинна бути 1 активна AI");
    }

    [Test]
    public void TestGetAllActiveAI()
    {
        manager.CreateAI("Enemy1", Vector3.zero);
        manager.CreateAI("Enemy2", Vector3.zero);
        manager.CreateAI("Enemy3", Vector3.zero);

        var allAI = manager.GetAllActiveAI();
  
        Assert.AreEqual(3, allAI.Count, "Повинно бути 3 AI в списку");
    }

    [Test]
    public void TestSaveBattleResult()
    {
        Assert.DoesNotThrow(() => manager.SaveBattleResult("Victory", "Enemy1"));
    }

    [Test]
    public void TestLoadBattleHistory()
    {
        string history = manager.LoadBattleHistory();
   
        Assert.IsNotEmpty(history, "Історія не повинна бути пустою");
    }

    [Test]
    public void TestFullCycle()
    {
        AIUnit ai1 = manager.CreateAI("Enemy1", new Vector3(0, 0, 0));
        AIUnit ai2 = manager.CreateAI("Enemy2", new Vector3(5, 0, 0));
        
        Assert.AreEqual(2, manager.GetActiveAICount());
     
        AIUnit retrieved = manager.GetAI("Enemy1");
        Assert.AreEqual(ai1, retrieved);
       
        manager.DestroyAI("Enemy1");
        Assert.AreEqual(1, manager.GetActiveAICount());

        Assert.DoesNotThrow(() => manager.SaveBattleResult("Loss", "Enemy2"));
       
        manager.DestroyAI("Enemy2");
        Assert.AreEqual(0, manager.GetActiveAICount());
    }

    [Test]
    public void TestMultipleAIWithDifferentDifficulties()
    {
        manager.SetDifficulty(AIManager.AIDifficulty.Easy);
        AIUnit easyAI = manager.CreateAI("EasyEnemy", Vector3.zero);
       
        manager.SetDifficulty(AIManager.AIDifficulty.Normal);
        AIUnit normalAI = manager.CreateAI("NormalEnemy", Vector3.zero);

        manager.SetDifficulty(AIManager.AIDifficulty.Hard);
        AIUnit hardAI = manager.CreateAI("HardEnemy", Vector3.zero);

        Assert.AreEqual(60, easyAI.GetMaxHealth());
        Assert.AreEqual(100, normalAI.GetMaxHealth());
        Assert.AreEqual(150, hardAI.GetMaxHealth());
        
        Assert.AreEqual(3, manager.GetActiveAICount());
    }
}

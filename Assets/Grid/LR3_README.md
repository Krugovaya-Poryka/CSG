# ЛР3: наслідування та поліморфізм

Орієнтовна готовність — **95%**. Реалізація та перевірки логіки виконані, конфлікти класів усунено; Roslyn-компіляція всіх C# у Assets проходить без помилок. Unity 6000.6.0f1 імпортувала проєкт; автоматична перевірка запуску Map, BattleScene та CastleScene у headless Play Mode пройшла без помилок. Інтерактивний UI та повний gameplay ще потребують ручної перевірки. Оцінка не є офіційним балом викладача.

| Критерій | Бал |
|---|---:|
| Inheritance | 30/30 |
| Polymorphism | 29/30 |
| Test/demo scenario | 13/15 |
| UML | 14/15 |
| Code quality/integration | 9/10 |
| Разом | 95/100 |

## Усунення конфліктів класів

За окремим дозволом користувача видалено два зайві файли лише з `Assets/Grid/`, разом із їхніми `.meta`:

- `HeroArmy.cs` дублював ім'я існуючого компонента `Assets/HeroInventory/HeroArmy.cs`.
- `HeroInventary.cs` дублював ім'я повного інвентаря `Assets/Grid/HeroInventory.cs`.

Посилань на GUID видалених файлів у сценах, prefab та assets не знайдено. Герой продовжує використовувати існуючий бойовий HeroArmy і повний HeroInventory з екіпіровкою. Нових систем чи альтернативних класів не додано. Джерела видалених дублікатів збережені в Git-історії.

Після видалення всі поточні C# з Assets компілюються Roslyn без помилок і без виключення будь-яких наявних файлів. Зміни цієї правки обмежені папкою Grid. Відомі проблеми бойового UI (прив'язка перейменованого поля BattleLogUI) та порядку null-перевірки в BattleInputHandler знаходяться поза Grid і цією правкою не виправлялися; успішна компіляція не доводить працездатності всього gameplay.

## Ієрархія та поведінка

```text
MapObject (abstract, MonoBehaviour)
├── ResourceObject
├── NeutralArmy
└── TeamOwnedObject (abstract, спільне teamId)
    ├── Castle
    └── Mine
```

`MapObject.Interact(HeroController hero)` — абстрактний контракт. `TeamOwnedObject` успадковує його без реалізації та зберігає спільний `teamId`. Чотири конкретні класи реалізують `public override void Interact(HeroController hero)`:

| Клас | Реальний результат взаємодії |
|---|---|
| ResourceObject | Додає `amount` до `Team.gold`, встановлює `IsCollected`, викликає `Destroy`. Повторний виклик до завершення кадру не додає золото вдруге. Без героя/зареєстрованої команди ресурс залишається. |
| Castle | Для чужої команди змінює `teamId`, через `RegisterCastle` переноситься зі старих `Team.castles` до нової. Для своєї команди повідомляє про відвідування. Зв'язок entrance та очищення OnDestroy збережені. |
| Mine | Змінює `teamId`, через `RegisterMine` переноситься до `Team.mines` власника. Start реєструє шахту, OnDestroy прибирає посилання. Відвідування своєї шахти без повторів у списку. Дохід за хід не додавався. |
| NeutralArmy | Власне безпечне повідомлення про зустріч. Наявний BattleSetup працює з окремо налаштованими арміями; переходу з NeutralArmy до бою немає. ЛР3 не створює неналаштований бій і не змінює AI. |

Усі чотири реалізації безпечно ігнорують null-героя. `RegisterMine` повторює наявний підхід `RegisterCastle`: прибирає всі попередні посилання, відновлює null-колекції та додає об'єкт до однієї команди. Наявна економіка використовується лише ресурсом; складної економічної системи не додано.

## Стабільність при перезавантаженні скриптів

Під час справжнього Play Mode виявлено: Unity примусово перезавантажувала збірки, після чого статичний `TeamManager.Instance` втрачався, хоча об'єкт менеджера залишався у сцені. Додано відновлення статичних посилань через `OnEnable` у `TeamManager`, `HeroSelectionManager` та `HeroDetailsPanel`. Це зберігає доступ до команд, вибору героя й панелі після script reload; нових менеджерів чи систем не додано.

## Де видно поліморфізм

У `HeroController.MoveAlongPath()` після прибуття на клітинку спочатку береться `GridStat.castle`, щоб зберегти взаємодію через entrance. Якщо замку немає, використовується `GetComponentInChildren<MapObject>()`. Обидва випадки завершуються єдиним викликом:

```csharp
MapObject mapObject = targetTile.castle;
if (mapObject == null)
    mapObject = targetTile.GetComponentInChildren<MapObject>();
if (mapObject != null)
    mapObject.Interact(this);
```

Castle entrance має пріоритет, але виклик відбувається через базовий тип. Перевірок `obj is Castle/ResourceObject/...` для вибору поведінки немає. Рух, підтвердження шляху, вартість руху та path preview не перероблялися.

У `LR3PolymorphismDemo.RunDemo()` колекція `List<MapObject>` обходиться як `foreach (MapObject obj in objects)`, після чого викликається `obj.Interact(hero)`. Виведення `GetType().Name` пояснює диспетчеризацію й не обирає реалізацію.

## Запуск демонстрації

1. Відкрити проєкт у Unity та дочекатися повторного імпорту й компіляції після видалення дублікатів.
2. Створити окрему тимчасову сцену. Додати порожній GameObject з `HeroController`: grid і army для цього сценарію не потрібні. Встановити герою `teamId = 1`.
3. Створити чотири окремі GameObjects з компонентами `ResourceObject`, `Castle`, `Mine`, `NeutralArmy`. Для замку й шахти встановити `teamId = 2`; для ресурсу, наприклад, `amount = 100`. Спрайти/колайдери для прямого виклику не потрібні.
4. На ще один GameObject додати `LR3PolymorphismDemo`. Призначити `hero`; у `objects` додати чотири компоненти в зазначеному порядку. Посилання мають вказувати на об'єкти сцени, а не prefab assets.
5. Увійти в Play Mode. В меню компонента (три крапки/контекстне меню) вибрати `LR3/Run polymorphism demo (Play Mode)`.
6. У Console видно `LR3: MapObject -> ...Interact` та повідомлення з кожного override: `ResourceObject.Interact`, `Castle.Interact`, `Mine.Interact`, `NeutralArmy.Interact`.
7. У runtime TeamManager перевірити: команда 1 отримала 100 gold, замок і шахту; команда 2 більше не містить захоплених об'єктів. Ресурс зникає наприкінці кадру. Повторний запуск не додає золото; замок і шахта вже свої.
8. Вийти з Play Mode. Для повторення початкового стану знову ввійти в Play Mode.

У компонента немає Start/Update чи автоматичного підключення до prefab/сцени. Звичайний запуск карти не запускає демо. В Edit Mode метод лише попереджає й не змінює об'єкти. **Ручний запуск у Play Mode застосовує реальні ефекти до призначених об'єктів**, тому рекомендована окрема тестова сцена. Шахти та нейтральні армії автоматично до генератора карти не додавалися.

## UML і матеріали для викладача

Відкрити `Assets/Grid/lr3_class_diagram.puml` у PlantUML-сумісному переглядачі. Діаграма містить абстрактні класи, всі п'ять стрілок наслідування, чотири override, залежність `MapObject ..> HeroController`, обидва місця диспетчеризації, TeamManager, агрегацію команд та композицію даних героя. HeroArmy показано як посилання на існуючий компонент.

Показати `MapObject.cs`, `TeamOwnedObject.cs`, чотири похідні класи, фрагмент `HeroController.MoveAlongPath`, `Team.cs`, `TeamManager.cs`, `LR3PolymorphismDemo.cs`, UML і цей README. Для живого прикладу запустити контекстне меню та показати Console і списки команд.

На захисті: «Наслідування задає ієрархію: ResourceObject і NeutralArmy є MapObject, а Castle та Mine додатково успадковують teamId від TeamOwnedObject. Абстрактний Interact задає спільний контракт. Поліморфізм дозволяє викликати його через MapObject: C# обирає override за фактичним типом об'єкта. Однаковий виклик збирає ресурс, захоплює замок/шахту або повідомляє про нейтральну армію. TeamManager підтримує актуальні списки власників».

## Перевірки та межі результату

- Roslyn зі встановленого Unity 6000.6.0f1: усі поточні C# з Assets, Unity references/defines/analyzers з наявного response-файлу. Повний набір після усунення дублікатів компілюється без помилок, без додаткових виключень джерел. 40 перевірок логіки повторно скомпільовано й виконано успішно.
- Залишилися наявні попередження: obsolete FindFirstObjectByType у BattleUnit і несеріалізовні двовимірні масиви у GridBehavior/HexGridManager.
- Пройшли 40 перевірок логіки: диспетчеризація чотирьох override, захоплення/повторне відвідування, перенесення між командами, видалення повторів, null-списки/аргументи, одноразовий ресурс, ручне демо, cleanup та ієрархія через reflection. Компілювалися реальні класи взаємодії/команд/демо; Unity API, HeroController і GridStat у harness замінені заглушками. Lifecycle-методи викликалися явно. Це не перевірка справжнього Unity lifecycle, руху або Play Mode.
- Справжній Unity Editor 6000.6.0f1: імпорт і компіляція успішні. Тимчасовий Editor-тест послідовно запускав `Map`, `BattleScene`, `CastleScene` приблизно на 4 секунди кожну у headless Play Mode. У трьох сценах відсутніх скриптів немає; підсумок тесту `TOTAL ERRORS: 0`.
- На реальній карті перевірені hero/grid/army/inventory, реєстрація героя, відновлення статичних посилань після script reload, чотири поліморфні виклики `Interact` на створених для тесту об'єктах, одноразове нарахування ресурсу та списки власників замку/шахти. Сцени не зберігалися. Тимчасовий Editor-тест прибрано з Assets; звіт `/tmp/csg-lr3/playmode-report.txt`, код `/tmp/csg-lr3/CSGStartupSmokeCheck.cs`.
- Git-конфліктів/маркерів злиття та повторних GUID у Assets не знайдено. Скриптові посилання ігрових сцен і prefab розв'язуються; у сторонніх прикладах TextMesh Pro є наявні відсутні посилання, ці приклади не виправлялися.
- Headless-перевірка не перевіряє відображення UI, кліки, весь рух/бій чи відсутність помилок після тривалої гри. Зазначені проблеми BattleLogUI/BattleInputHandler поза Grid залишаються.
- Тимчасові response-файли, логи та harness: `/tmp/csg-lr3/`; вони не додаються до репозиторію.
- Пройшли 26 статичних перевірок: ієрархія, override, обидва виклики через базовий тип, ручний запуск демо, відповідність UML, унікальні GUID нових assets, відсутність змін AI та точний обсяг змін HeroController. `git diff --check -- Assets/Grid` успішний.
- UML перевірена за джерелами та зв'язками; графічний рендер PlantUML не виконувався.
- Алгоритми GridBehavior/Pathfinder/PathMaker, UI, інвентар, army references, сцени та prefab не змінювалися. Файли `Assets/BattleScripts/AI/` і `Docs/UML/AI/` не змінювалися. Push не виконувався.

## Файли цієї роботи

Змінені (9):

- `Assets/Grid/Castle.cs`
- `Assets/Grid/HeroController.cs`
- `Assets/Grid/HeroDetailsPanel.cs`
- `Assets/Grid/HeroSelectionManager.cs`
- `Assets/Grid/Mine.cs`
- `Assets/Grid/NeutralArmy.cs`
- `Assets/Grid/ResourceObject.cs`
- `Assets/Grid/Team.cs`
- `Assets/Grid/TeamManager.cs`

Створені (6):

- `Assets/Grid/LR3PolymorphismDemo.cs` і `.meta`
- `Assets/Grid/lr3_class_diagram.puml` і `.meta`
- `Assets/Grid/LR3_README.md` і `.meta`

Видалені при усуненні конфліктів (4):

- `Assets/Grid/HeroArmy.cs` і `.meta`
- `Assets/Grid/HeroInventary.cs` і `.meta`

README оновлено результатами повної компіляції після усунення конфліктів.

## Ручний checklist Unity Play Mode

- [ ] Дочекатися повторного імпорту Unity; підтвердити відсутність compilation errors у Console.
- [ ] Запустити окрему демо-сцену: чотири різні override в Console, правильні gold/castles/mines.
- [ ] Повторити демо: золото не додається, списки не дублюються; у Edit Mode ефектів немає.
- [ ] На карті перевірити вибір героя, preview, підтвердження руху й витрати movement points.
- [ ] Зайти саме на entrance чужого замку: teamId та списки castles змінюються; повторно відвідати свій замок.
- [ ] Зібрати ресурс: золото додається один раз, ресурс зникає; шлях після збору працює.
- [ ] Захопити шахту різними командами; видалити runtime шахту/замок і перевірити cleanup списків та entrance.
- [ ] Перевірити H, inventory, Equip/Unequip, характеристики, army reference/сім слотів і блокування кліків через UI.
- [ ] Перевірити EndTurn, path markers та наявну бойову сцену/AI без змін її налаштувань.
- [ ] Відкрити UML у PlantUML і перевірити читабельність перед захистом.

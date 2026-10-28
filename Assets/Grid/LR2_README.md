# ЛР2: агрегація та композиція

ЛР2 готова орієнтовно на **95% за перевіреним кодом**. Це оцінка готовності до захисту, не офіційний бал викладача. Повний інтеграційний результат потребує Play Mode.

| Критерій | Бал |
|---|---:|
| Агрегація | 29/30 |
| Композиція | 29/30 |
| Колекції | 20/20 |
| Демонстрація | 8/10 |
| UML/архітектура | 9/10 |

## Changed

- `TeamManager` реєструє героїв/замки без дублікатів, прибирає навіть попередні повторні посилання, відновлює null-колекції. Реєстрація в іншій команді спершу прибирає об'єкт з усіх старих списків.
- `Castle` і `HeroController` забезпечують наявність менеджера на старті. `GridBehavior` задає teamId до реєстрації створених об'єктів; повторна реєстрація в Start безпечна.
- Фактичний castle prefab — `Aobject.prefab`: саме на нього посилається `Atile.prefab`. До кореня додано `Castle` зі збереженням наявних GUID/спрайта.
- `GridBehavior` зв'язує `Castle.entrance` і `GridStat.castle`. Спрайт залишається в центрі footprint. Після прибуття на entrance герой викликає `Castle.Interact(this)`; для інших об'єктів збережено існуючий MapObject fallback.
- `Player.prefab` містить **існуючий** `HeroArmy` з `Assets/HeroInventory/`, сім порожніх слотів і явне посилання з HeroController. Бойові юніти не створюються.
- `EndTurn()` працює через `TeamManager.GetTeam(currentTeamId).heroes`. Поле `HeroSelectionManager.currentTeamId` типово дорівнює 0 — це команда єдиного героя, якого зараз створює GridBehavior. Вибір чужого героя не змінює активну команду. Повної системи перемикання команд не додано.

## Created

- `HeroInventoryDemo.cs` і `.meta`: компонент уже підключено до Grid/Player.prefab. У Play Mode Start один раз викликає AddItem для нового Training sword (+1 attack) та Wooden shield (+1 defense). Кожен герой отримує окремі екземпляри предметів.
- `lr2_class_diagram.puml` і `.meta`: UML відповідно до фактичних класів і зв'язків.

## Fixed

- Захоплення замку змінює teamId і переносить його в нову Team.castles. OnDestroy прибирає замок зі списків та очищає посилання entrance. HeroController.OnDestroy прибирає героя зі списків.
- Null checks для TeamManager/колекцій, HeroSelectionManager при кліку по герою, ResourceObject при генерації, героя при зборі ресурсу, grid/target/path markers. Перевірки stats/inventory/army у панелі збережені.
- Ресурси тепер створюються дочірніми об'єктами клітинки: наявний GetComponentInChildren<MapObject>() може знайти їх. На entrance ресурси не створюються. Невалідний ресурсний prefab прибирається з попередженням у Console.
- Видалено невикористані `lastTarget`, `selectedTarget` та застаріле серіалізоване `pm` у Player.prefab. `heroPortrait` залишено як публічний Inspector hook і позначено Tooltip: поточний HUD використовує лише текст. Старі порожні GridMover/UnitStack не видалено: немає підстав змінювати їхні можливі зовнішні прив'язки.

## Aggregation

`Team o-- HeroController` і `Team o-- Castle`: Team зберігає посилання на незалежні GameObjects. Вилучення зі списку не викликає Destroy. Реєстрація нормалізує приналежність до однієї команди. Це не нова система володіння GameObjects.

## Composition

`HeroController *-- HeroStats` і `HeroController *-- HeroInventory`: inline serializable C#-дані створюються для кожного героя; Awake відновлює їх, якщо значення null. HeroInventory містить List<Item>. Демонстраційні Item створюються для конкретного інвентаря й не мають зовнішнього власника чи спільного asset. Видалення прибирає посилання, пам'ять звільняє GC. Публічний API залишається простим: сторонній код не повинен навмисно ділити ці об'єкти між героями.

`HeroController --> HeroArmy` — звичайний зв'язок із зовнішнім компонентом, не композиція в UML. Життєвий цикл сторонньої армії цей код не керує. Бонуси екіпірованих Item враховуються в HeroController.Attack/Defense без зміни базових HeroStats. Предмети в рюкзаку бонусів не дають.

## Demo

1. Відкрити карту, дочекатися компіляції та запустити Play заново. Сцену зберігати не потрібно.
2. Вибрати героя кліком, натиснути H. Перевірити stats, movement, два предмети й сім слотів army.
3. Натиснути `LR2: remove training sword`. Залишиться Wooden shield. Закрити/відкрити H: меч не повертається, предмети не додаються кожен кадр. У Play Mode також доступні context menu компонента HeroInventoryDemo: `LR2/Add demo items once` і `LR2/Remove demo sword`.
4. В Inspector runtime TeamManager перевірити, що герой є лише в heroes його команди, а кожний замок — у castles його команди.
5. Клікнути двічі по entrance чужого замку (нижня центральна клітинка footprint). Для довгого шляху повторювати рух і EndTurn у межах доступних очок. Після прибуття перевірити повідомлення `Castle captured!`, teamId та обидва списки castles.
6. Зменшити movementPoints героя; натиснути наявну кнопку EndTurn: рух відновиться для currentTeamId. Якщо додано героя іншої команди, його очки не зміняться.
7. Видалити runtime героя/замок через Inspector і перевірити очищення списків. Це ручна перевірка справжнього Unity OnDestroy.
8. Перевірити рух до звичайної клітинки, preview/підтвердження, витрати movement points, збір ресурсу, H без вибраного героя та кліки всередині панелі.

## UML

Відкрити `lr2_class_diagram.puml` у PlantUML-сумісному переглядачі. Діаграма містить підписи aggregation/composition, кратності, TeamManager, entrance-зв'язок та EndTurn через Team. Стрілка армії не позначена композицією.

## Verification

- Повна компіляція всіх C# у Assets успішна компілятором Roslyn зі встановленого Unity **6000.6.0f1**, з Unity references, define-ами й аналізаторами наявного response-файлу. Артефакти перевірки розміщені лише в /tmp. Помилок компіляції та duplicate class errors немає.
- Є рівно один class HeroArmy — у `Assets/HeroInventory/HeroArmy.cs`.
- Пройшли **27 перевірок логіки**: повторна реєстрація/старі дублікати, передача команди, захоплення, cleanup замку, null, EndTurn лише активної команди, ресурс, AddItem/RemoveItem, одноразове демо, окремі дані для різних героїв. Використано реальні відповідні Grid-класи, але Unity API і HeroController у цьому harness замінені заглушками. OnDestroy замку викликано явно; це не перевірка Unity lifecycle.
- Пройшли **22 статичні перевірки prefab/джерел**: fileID компонентів, GUID Castle/HeroArmy/demo, сім слотів, явне посилання army, inline stats/inventory, castlePrefab клітинки, entrance binding, MapObject fallback та наявність unregistration героя.
- `Pathfinder.cs`, `GridMover.cs`, `PathMaker.cs` побайтово не змінені від початку цього запиту. Методи генерації замків/доріг і GetNeighbours не змінені. У HeroController додано перевірки та виклик взаємодії після прибуття; алгоритм руху та розрахунок вартості для валідних клітинок збережені.
- Hash-порівняння підтвердило: наявні файли поза Assets/Grid не змінені цим запитом. Попередні зміни користувача, включно з Assets/Scenes/Map.unity, залишено як були.
- `git diff --check -- Assets/Grid` успішний.

## Still missing

- Справжній Play Mode, візуальна панель, прохідність конкретної згенерованої карти, prefab import і Unity lifecycle ще потребують ручної інтеграційної перевірки за кроками вище. Успішна компіляція/заглушки не доводять ці пункти.
- Наявні warnings: застарілий FindFirstObjectByType у BattleUnit та несеріалізовний GameObject[,] у GridBehavior/HexGridManager. Двовимірний масив GridBehavior заповнюється в runtime; переробка цієї структури не входила в роботу.
- У збірці мають бути призначені gridPrefab/heroPrefab/resourcePrefab: наявний редакторський fallback через AssetDatabase працює лише в Unity Editor.
- Army slots порожні, перемикання ходів між командами не реалізовано — ці механіки не потрібні для демонстрації ЛР2.

## Усі файли, змінені саме цим запитом

Змінені:
- Assets/Grid/Aobject.prefab
- Assets/Grid/Castle.cs
- Assets/Grid/GridBehavior.cs
- Assets/Grid/GridStat.cs
- Assets/Grid/HeroController.cs
- Assets/Grid/HeroDetailsPanel.cs
- Assets/Grid/HeroSelectionManager.cs
- Assets/Grid/HeroesUI.cs
- Assets/Grid/MouseController.cs
- Assets/Grid/Player.prefab
- Assets/Grid/ResourceObject.cs
- Assets/Grid/TeamManager.cs
- Assets/Grid/LR2_README.md

Створені:
- Assets/Grid/HeroInventoryDemo.cs
- Assets/Grid/HeroInventoryDemo.cs.meta
- Assets/Grid/lr2_class_diagram.puml
- Assets/Grid/lr2_class_diagram.puml.meta

Team.cs, HeroStats.cs, HeroInventory.cs, Item.cs та чужі класи збережено без змін від початку запиту. Коміти не створювалися.

## Оновлення: масштаб та екіпіровка

- Коліщатко миші змінює масштаб утричі швидше від збереженого zoomSpeed; межі minZoom/maxZoom залишено. Прокручування над відкритою панеллю героя прокручує її вміст без масштабування карти.
- Панель H містить окремі секції EQUIPMENT та INVENTORY / BACKPACK. Слоти: Weapon, Shield, Helmet, Armor.
- Equip переносить предмет із рюкзака в екіпіровку. Якщо слот зайнятий, попередній предмет повертається в рюкзак. Unequip повертає предмет у рюкзак. Обидві колекції належать HeroInventory.
- Training sword та Wooden shield стартують у рюкзаку. Екіпірування дає +1 Attack / +1 Defense; зняття прибирає бонуси. Повторне екіпірування не накопичує бонусів, оскільки вони обчислюються з поточного вмісту екіпіровки. Базові HeroStats збережено.
- Демонстраційна кнопка видалення меча доступна, коли меч у рюкзаку: спочатку Unequip, потім Remove.
- Для перевірки: Play → герой → H → Equip меч/щит → перевірити stats → Unequip → перевірити повернення предмета та базової характеристики. Перевірити коліщатко над картою і над панеллю.
- У цьому оновленні змінені CameraController.cs, Item.cs, HeroInventory.cs, HeroController.cs, HeroInventoryDemo.cs, HeroDetailsPanel.cs, Player.prefab, lr2_class_diagram.puml та цей README; нових файлів не створено.
- Перевірка оновлення: всі C#-скрипти скомпільовані без помилок; пройшли 23 перевірки перенесення/заміни предметів, слотів, null-колекцій і відсутності накопичення бонусів. Візуальна перевірка коліщатка та панелі в Play Mode ще потрібна.

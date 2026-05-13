---
description: 卡牌对战系统架构说明，包含文件位置、扩展方式和关键类
globs: Assets/Scripts/**/*.cs
alwaysApply: false
---

# 卡牌对战系统架构

## 关键文件位置

| 类型 | 路径 |
|------|------|
| Architecture 入口 | `Assets/Scripts/Core/GameArchitecture.cs` |
| 游戏启动入口 | `Assets/Scripts/GameManager.cs` |
| 战斗 Model | `Assets/Scripts/Gameplay/Battle/BattleModel.cs` |
| 牌组 Model | `Assets/Scripts/Gameplay/Battle/DeckModel.cs` |
| 战斗 System | `Assets/Scripts/Gameplay/Battle/BattleSystem.cs` |
| 卡牌 System | `Assets/Scripts/Gameplay/Battle/CardSystem.cs` |
| 卡牌显示组件 | `Assets/Scripts/UI/CardDisplayView.cs` |
| UI 层级管理 | `Assets/Scripts/UI/UILayerManager.cs` |
| 弹窗动态加载 | `Assets/Scripts/UI/UIPopupManager.cs` |
| 怪物列表配置 | `Assets/Scripts/Data/MonsterListData.cs` |
| 所有事件 | `Assets/Scripts/Gameplay/Events/BattleEvents.cs` |
| 效果基类 | `Assets/Scripts/Data/CardEffect.cs` |
| 具体效果 | `Assets/Scripts/Data/Effects/` |
| 效果数值提升 | `Assets/Scripts/Gameplay/Battle/CardEffectBoost.cs` |

---

## 战斗核心流程

```
GameManager.StartBattle()
  └─ StartBattleCommand
       └─ BattleSystem.StartBattle()
            ├─ DeckModel.InitDeck()       // 加载 DeckPresetData，建完整牌组
            ├─ BattleModel.InitBattle()   // 重置能量、回合
            ├─ BattleModel.StartMonster() // 加载 MonsterListData 的当前怪物与出牌轮数限制
            ├─ CardSystem.Shuffle()       // Fisher-Yates 洗牌
            ├─ CardSystem.DrawCards(8)    // 首回合抽 8 张
            └─ 发送 BattleStartedEvent / MonsterStartedEvent / TurnStartedEvent
```

`GameGlobalConfigData` 和场景 `GameManager` 都使用 `MonsterListData` 作为战斗怪物队列入口。每个怪物配置包含敌人数据与本怪物最大出牌轮数，一轮最多结算 5 张槽位卡。
当前战斗默认每回合最大能量为 13。

### 出牌流程

```
HandViewController  监听 HandRefreshedEvent
  └─ 实例化（或从对象池取出）CardViewController
       └─ CardDisplayView 统一刷新顶部类型、名称、费用、描述和图片

玩家拖拽卡牌到 CardSlotView
  └─ PlayCardCommand(cardData, slotIndex)
       └─ BattleSystem.TryPlayCard()
            ├─ 验证：槽位为空 & 能量足够
            ├─ 更新 BattleModel.PlaySlots[slotIndex]
            ├─ 扣除能量
            └─ 发送 CardPlayedEvent / EnergyChangedEvent

玩家点击「结束回合」
  └─ EndTurnCommand
       └─ BattleSystem.EndTurn()
            ├─ 验证：5 个槽位必须全部放满，否则不允许结束回合
            ├─ BattleSystem.ResolveSlots()  // 按槽位顺序结算
            │    ├─ CardEffect(出牌开始阶段) // 先处理全部槽位
            │    ├─ CardEffect(出牌阶段) // 再按槽位顺序结算全部槽位
            │    ├─ CardEffect(出牌结束阶段) // 最后再按槽位顺序结算全部槽位
            │    └─ 一轮最多结算 5 张槽位卡；卡牌在任意槽位都会执行效果
            ├─ 若怪物被击败，生成奖励并暂停后续流程
            │    ├─ CardSystem.DiscardHand() // 先将剩余手牌全部弃掉
            │    ├─ CardSystem.DiscardDrawPile() // 再将抽牌堆剩余卡牌全部送入弃牌堆
            │    └─ BattleRewardSystem.TryOfferTurnReward() // 等全部弃牌动画后弹出奖励
            ├─ SelectBattleRewardCommand // 玩家完成所有奖励组选择后进入下一只怪物或胜利
            ├─ 若出牌轮数达到上限但怪物未被击败，战斗失败
            ├─ CardSystem.DiscardHand()     // 手牌全部弃掉
            ├─ EnemyTurn()
            ├─ 回合数 +1，恢复能量
            ├─ CardSystem.DrawCards(8)      // 新回合抽 8 张
            └─ 发送 TurnEndedEvent / TurnStartedEvent
```

当前出牌轮数按每次结束回合后的槽位结算计数。若一张卡击败当前怪物，本轮后续槽位卡牌不再继续结算，并统一进入弃牌堆；若因此触发战斗奖励，剩余手牌和抽牌堆中的卡牌也会先一起送入弃牌堆，等槽位、手牌和抽牌堆的弃牌动画结束后再弹出奖励。
玩家必须先放满 5 个出牌槽，才可以点击「结束回合」并结算本轮出牌；任意空槽都会阻止本轮出牌结算。
卡牌不再配置自身生效位置。只要被放入任意槽位并参与结算，就会按配置时机执行卡牌效果；槽位背景只区分空槽与已放置卡牌。
`CardData` 的效果直接内联配置在卡牌资产中，基于 Odin 多态序列化选择具体 `CardEffect` 类型，不再创建独立效果资产。
`CardEffect` 可配置生效时机：放牌阶段、出牌开始阶段、出牌阶段、出牌结束阶段。放牌阶段效果在卡牌放入槽位后立即执行；卡牌撤回、换槽、与手牌交换或清槽时会取消，换到新槽位后重新按新槽位生效。
出牌结算会先对全部槽位执行出牌开始阶段，再按槽位顺序执行全部出牌阶段效果，最后再按槽位顺序执行全部出牌结束阶段效果。
`DamageCardEffect` 支持固定伤害，也可以开启随机范围，在配置的下限与上限之间随机取一个基础伤害值后再进入伤害公式。
`BoostSlotCardEffect` 默认在出牌开始阶段提高本轮伤害，作用范围支持指定槽位、自身和相邻牌，提升方式支持基础伤害增加、伤害百分比增减、伤害总增和固定值增加。增伤数值可按增伤卡所在槽位线性递增或递减；在伤害总增模式下，线性基础数值就是基础倍率。因为出牌开始阶段会先于全部出牌阶段统一执行，所以增伤卡放在 5 号槽也能影响本轮目标槽位。当前增伤通过 `BattleContext.DealDamage()` 生效。
卡牌伤害按四段公式结算：`基础伤害区间 * 伤害增加区间 * 伤害总增区间 + 固定值区间`。普通伤害百分比增减会在伤害增加区间相加；每个伤害总增都是独立乘区，会在伤害总增区间连续相乘；伤害值添加可以配置到基础伤害区间，也可以配置到最终固定值区间。
`BattleUIController` 监听 `MonsterPlayRoundCountChangedEvent`，在战斗 UI 中显示当前怪物剩余出牌轮数。
手牌、奖励选项、牌堆弹窗和出牌槽都通过 `CardDisplayView` 显示统一卡牌信息；顶部信息区显示类型，名称、费用、描述按显示模式切换；各容器只负责自身交互和额外状态，例如槽位背景的空槽/已放置颜色。
手牌与槽位之间的飞行动画现在也保持 `CardDisplayMode.Full`，飞行中的临时卡牌、回手中的卡牌、槽位中的卡牌，以及从槽位拖到其他槽位或手牌区时的跟手预览都统一显示完整信息。
回合结束或重抽后，如果先发生弃牌，再触发补抽，手牌显示会等待弃牌动画播完后再开始抽牌；若当前抽牌堆还能抽出一部分卡牌，则这部分卡会先完成抽牌动画，只有在数量不够时才会播放洗牌预览动画，再补抽剩余卡牌。
UI 层级由 `UILayerManager` 统一管理：拖动中的卡牌和槽位预览进入 `DragLayer`，奖励与牌堆弹窗进入 `PopupLayer`，胜利/失败结果进入 `SystemLayer`。单张手牌不再使用独立 `Canvas` 抢占排序，避免手牌遮挡奖励等更高优先级 UI。
弹窗类 UI 由 `UIPopupManager` 常驻监听业务事件并动态加载。奖励弹窗与牌堆列表弹窗通过 `AssetReferenceGameObject` 引用 Addressables 预制体并实例化，结果确认面板运行时创建；战斗主 HUD、手牌区和出牌槽仍保留在场景中。只要当前有抽牌堆/弃牌堆相关动画在播放，抽牌堆与弃牌堆按钮都会暂时拒绝打开牌堆列表弹窗。

### 怪物推进与失败流程

```
当前怪物 HP 归零
  └─ BattleSystem.NotifyEnemyHpChanged()
       ├─ 标记当前怪物已击败
       └─ 发送 EnemyDiedEvent

BattleSystem.EndTurn()
  └─ 若当前怪物已击败
       ├─ BattleRewardSystem.TryOfferTurnReward()
       ├─ 触发奖励前先清空槽位、手牌与抽牌堆到弃牌堆
       ├─ 若 MonsterListData 还有下一只怪物，StartMonster() 并开始下一回合
       └─ 若没有下一只怪物，发送 BattleEndedEvent(PlayerWon = true)

当前怪物已出牌轮数 >= MaxPlayRounds 且仍未击败
  └─ BattleSystem.FailBattle()
       ├─ 清理出牌槽
       └─ 发送 BattleEndedEvent(PlayerWon = false)
```

战斗胜利或失败后，`BattleUIController` 显示结果确认 UI。玩家点击确认会发送 `RestartBattleCommand`，使用本次战斗的牌组、怪物列表、奖励配置和数值重新开始战斗。
重开会先重置玩家战斗状态：手牌与手牌 UI、抽牌堆、弃牌堆、出牌槽、能量、重抽次数、奖励待选和怪物出牌轮数都会回到初始状态，再开始新一局。

### 撤牌 / 换牌

```
从槽位拖回手牌区
  └─ ReturnCardToHandCommand
       └─ BattleSystem.TryReturnCardFromSlot()

从槽位拖到另一槽位
  └─ SwapSlotsCommand
       └─ BattleSystem.TrySwapSlots()
```

### 重抽流程

```
玩家点击「重抽」
  └─ RedrawCardsCommand(selectedCards)
       └─ BattleSystem.TryRedrawCards()
            ├─ 验证：最多选择 5 张，RedrawsRemaining > 0，且本回合最大能量 > 0
            ├─ CardSystem.RedrawCards()    // 选中手牌→弃牌堆，重新抽同等数量
            ├─ RedrawsRemaining -1
            ├─ 本回合 MaxEnergy -1，并将 CurrentEnergy 限制到新的上限内
            └─ 发送 RedrawCountChangedEvent / EnergyChangedEvent
```

重抽消耗的是“本回合最大能量”，不是整局永久最大能量；进入下一回合时，`StartTurn()` 会先把最大能量重置回战斗初始值，再恢复本回合能量。
重抽界面当前最多只能选择 5 张手牌，系统层也会拒绝超过 5 张的重抽请求。

### 战斗奖励流程

```
BattleSystem.EndTurn()
  └─ ResolveSlots()
       └─ 若当前怪物被击败，调用 BattleRewardSystem.TryOfferTurnReward()
            ├─ CardSystem.DiscardHand() // 先把剩余手牌送入弃牌堆
            ├─ CardSystem.DiscardDrawPile() // 再把抽牌堆剩余卡牌送入弃牌堆
            ├─ 根据 BattleRewardConfigData 生成本次奖励组
            ├─ 当前卡牌奖励组优先从 CardLibraryData 牌库中按解锁条件筛选
            ├─ 从已解锁卡牌中按权重无放回随机抽 3 张
            ├─ 若奖励组未配置牌库，则兼容旧的配置卡池随机
            └─ 发送 BattleRewardOfferedEvent，等待槽位、手牌与抽牌堆弃牌动画结束后弹出奖励

玩家选择奖励选项
  └─ SelectBattleRewardCommand(offerId, optionId)
       └─ BattleRewardSystem.ClaimReward()
            ├─ 卡牌奖励：CardSystem.AddCardToDeck()，加入弃牌堆并同步 FullDeck
            ├─ 若仍有奖励组，继续发送剩余待选项
            └─ 全部奖励组完成后发送 BattleRewardCompletedEvent，并进入下一只怪物/战斗胜利
```

`BattleRewardConfigData` 支持一次奖励配置多个奖励组；每个奖励组都是多选一。当前奖励类型只实现 `Card`，默认用于卡牌三选一。

`CardData` 使用单选卡牌类型，默认类型为「通用」，当前类型包括「通用」「占卜」「奥术」。类型用于词条生效检索、流派分类和后续奖励筛选，可通过 `Type` 和 `IsType()` 查询。

`CardLibraryData` 是卡牌奖励牌库。牌库条目包含：

- `Card`：可作为奖励的卡牌配置。
- `Weight`：随机权重，数值越高越容易出现。
- `UnlockConditions`：解锁条件列表，全部满足时该卡牌才进入本次奖励候选。

当前支持的解锁条件：

| 条件 | 说明 |
|------|------|
| `Always` | 始终解锁 |
| `MinBattleCount` | 当前战斗次数大于等于指定值 |
| `MaxBattleCount` | 当前战斗次数小于等于指定值 |
| `MinDeckCardCount` | 玩家完整牌组数量大于等于指定值 |
| `HasCardInDeck` | 玩家完整牌组中拥有指定卡 |
| `DoesNotHaveCardInDeck` | 玩家完整牌组中没有指定卡 |

默认配置与 UI：

- `Assets/Data/Preset/CardLibrary/DefaultCardLibrary.asset`：默认卡牌奖励牌库，包含基础、进阶、超级伤害卡及战斗次数解锁条件。
- `Assets/Data/Preset/Reward/BattleRewardConfig.asset`：默认战斗奖励配置，包含 1 个引用默认牌库的卡牌三选一奖励组。
- `Assets/Data/Preset/Enemies/DefaultMonsterList.asset`：默认怪物列表配置，当前包含基础敌人与出牌轮数限制。
- `Assets/Prefabs/BattleRewardOption.prefab`：单个奖励选项视图。
- `Assets/Prefabs/BattleRewardPopup.prefab`：奖励选择弹窗，由 `UIPopupManager` 按需加载到 `PopupLayer`。
- `Assets/Prefabs/CardListPopup.prefab`：牌堆列表弹窗，由 `UIPopupManager` 按需加载到 `PopupLayer`。
- `Assets/Scripts/Editor/BattleRewardSetupUtility.cs`：可通过 Unity 菜单 `Card5/Setup Battle Reward UI` 重新生成默认配置与 UI。

---

## 新增卡牌效果

继承 `CardEffect`，实现 `Execute(BattleContext ctx)`。效果不再创建独立 `ScriptableObject` 资产，而是在 `CardData` 的「卡牌效果」列表中直接选择和配置。
卡牌配置中会直接显示具体生效时机，不再显示“使用默认时机”。新增效果默认显示为 `Play`（出牌阶段）；如果某个效果更适合其他初始时机，可在构造函数中调用 `base(CardEffectTiming.Xxx)` 设置。
每张卡牌生效时都会获得自己的 `BattleContext`。出牌结算中，同一张卡的出牌开始、出牌、出牌结束阶段共享同一个上下文；放牌阶段上下文会保留到撤回、换槽、清槽或弃牌时取消。上下文可用来记录临时信息、注册临时事件，生效结束后会自动清理并注销事件。放牌阶段效果如果会修改可变战斗状态，需要同时重写 `Cancel(BattleContext ctx)` 做反向取消。

```csharp
using System;
using UnityEngine;

[Serializable]
public class XxxCardEffect : CardEffect
{
    [SerializeField] int _amount = 1;

    public override void Execute(BattleContext context)
    {
        // context.BattleModel          — 玩家战斗状态（能量、槽位）
        // context.DeckModel            — 牌组状态（抽牌堆、弃牌堆）
        // context.Enemy                — 敌人（TakeDamage）
        // context.BattleSystem         — 调用战斗系统方法
        // context.LeftNeighbor         — 左邻槽卡牌（可为 null）
        // context.RightNeighbor        — 右邻槽卡牌（可为 null）
        // context.SlotIndex            — 当前槽位索引（0-4）
        // context.CurrentCard          — 当前卡牌数据
        // context.CurrentTiming        — 当前效果生效时机
        // context.DamageDealtThisCard  — 本张卡当前累计造成伤害
        // context.SetValue(key, value) — 记录本张卡生效期间的临时信息
        // context.TryGetValue(key, out value) — 读取临时信息
        // context.RegisterEvent<T>(handler) — 注册随上下文释放自动注销的临时事件
        // context.DealDamage(amount)   — 便捷伤害方法（含事件）
    }

    public override void Cancel(BattleContext context)
    {
        // 仅放牌阶段等需要撤销的效果实现
    }
}
```

---

## 敌人扩展

实现 `IEnemyBehavior`，注入到 `EnemyController`：

```csharp
public class MyEnemyBehavior : IEnemyBehavior
{
    void OnTurnStart() { ... }
    void OnTurnEnd()   { ... }
    void OnTakeDamage() { ... }
}

// 在某处注入：
enemyController.SetBehavior(new MyEnemyBehavior());
```

---

## 命令一览

| 命令 | 返回值 | 说明 |
|------|--------|------|
| `StartBattleCommand` | void | 初始化并启动战斗 |
| `DrawCardCommand` | void | 抽一张牌 |
| `PlayCardCommand` | bool | 将手牌放入指定槽位 |
| `SwapHandWithSlotCommand` | bool | 将手牌与指定槽位中的卡牌直接交换 |
| `ReturnCardToHandCommand` | bool | 从槽位撤回手牌 |
| `SwapSlotsCommand` | bool | 交换/移动两个槽位的卡牌 |
| `EndTurnCommand` | void | 结束回合并触发结算 |
| `RedrawCardsCommand` | bool | 重抽选中的手牌 |
| `AddCardToDeckCommand` | void | 新增卡牌到牌库（写入弃牌堆，同步 FullDeck） |
| `RemoveCardFromDeckCommand` | bool | 从牌库移除一张卡牌（FullDeck + DrawPile/DiscardPile） |
| `SelectBattleRewardCommand` | bool | 领取指定奖励组选项；全部奖励领取完成后恢复战斗流程 |
| `RestartBattleCommand` | void | 战斗结果确认后，使用上次战斗配置重新开始 |

---

## 事件一览

| 事件 | 触发时机 |
|------|---------|
| `BattleStartedEvent` | 战斗初始化完成 |
| `BattleEndedEvent` | 战斗结束（含 `PlayerWon`） |
| `MonsterStartedEvent` | 当前怪物开始，包含怪物序号、总数与出牌轮数限制 |
| `MonsterPlayRoundCountChangedEvent` | 当前怪物已出牌轮数变化 |
| `TurnStartedEvent` | 新回合开始 |
| `TurnEndedEvent` | 回合结算完成 |
| `CardDrawnEvent` | 单张牌被抽取 |
| `HandRefreshedEvent` | 手牌整体刷新（HandViewController 监听此事件重建 View） |
| `CardAddedToHandEvent` | 单张牌加入手牌 |
| `CardRemovedFromHandEvent` | 单张牌离开手牌 |
| `CardReturnedToHandEvent` | 槽位中的卡牌返回手牌 |
| `CardPlayedEvent` | 卡牌放入槽位 |
| `CardRemovedFromSlotEvent` | 卡牌从槽位移除 |
| `HandSlotSwappedEvent` | 手牌与槽位中的卡牌交换完成 |
| `SlotsSwappedEvent` | 两个槽位内容交换 |
| `SlotEffectsResolvedEvent` | 本回合所有槽位效果结算完毕 |
| `DamageDealtEvent` | 对敌人造成伤害 |
| `EnemyHpChangedEvent` | 敌人 HP 变化 |
| `EnemyDiedEvent` | 敌人死亡 |
| `EnergyChangedEvent` | 能量变化 |
| `RedrawCountChangedEvent` | 剩余重抽次数变化 |
| `DrawPileChangedEvent` | 抽牌堆数量变化 |
| `DiscardPileChangedEvent` | 弃牌堆数量变化 |
| `CardAddedToDeckEvent` | 新卡牌加入牌库（弃牌堆 + FullDeck） |
| `CardRemovedFromDeckEvent` | 卡牌从牌库移除 |
| `BattleRewardOfferedEvent` | 击败怪物后的战斗奖励生成，等待玩家选择 |
| `BattleRewardOptionClaimedEvent` | 某个奖励组选项被领取 |
| `BattleRewardCompletedEvent` | 本次所有奖励组领取完成 |

---

## 手牌交互补充

- `HandViewController` 已改为脚本布局，不再依赖 `HorizontalLayoutGroup` 控制手牌位置。
- 手牌数量较多时会自动压缩间距形成重叠，鼠标悬停时会抬起当前卡牌并展开附近卡牌。
- 手牌拖拽只响应左键，并会实时计算插入预览位置，其余卡牌通过 `PrimeTween` 做避让动画。
- 手牌拖到已占用槽位时，通过 `SwapHandWithSlotCommand` 直接与槽位中的卡牌交换。
- 从槽位拖回手牌区时，返回的卡牌会按鼠标落点插入手牌，不再先播放“从槽位飞到手牌末尾”的动画。
- 两个槽位交换卡牌时，会先让目标槽位原有的卡飞回源槽位；等这段动画结束后，再同时显示源槽位的新卡和被拖拽到目标槽位的卡。
- 右键交互更新：右键不会触发拖拽；手牌右键会尝试放入最左侧空槽位，并在飞行动画结束后才显示到槽位；槽位右键会将该槽位中的卡牌撤回手牌。
- 非拖拽触发的出牌使用临时卡牌视图做飞行动画，避免直接瞬移。
- 抽牌时，新手牌会从抽牌堆位置飞入手牌区；手牌弃牌、重抽弃牌和槽位结算弃牌都会播放飞向弃牌堆的动画。
- 回合结束和重抽后的补抽会等待当前弃牌动画结束后再开始；如果抽牌堆还有剩余卡牌，会先把这部分卡从抽牌堆飞入手牌，等这段抽牌动画完成后，才在数量不足时播放弃牌堆回抽牌堆的洗牌预览动画，并继续补完剩余抽牌。
- 只要抽牌、弃牌或洗牌相关 tween 还在播放，抽牌堆和弃牌堆都不能点开列表弹窗，避免动画过程中查看到中间态牌堆内容。

### 本次新增命令与事件

- `SwapHandWithSlotCommand`
- `CardReturnedToHandEvent`
- `HandSlotSwappedEvent`

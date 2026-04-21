---
alwaysApply: true
---
# 项目结构指南

## 目录结构

### 核心目录 `Assets/Scripts/`

```
Assets/Scripts/
├── Core/                        # 核心框架
│   ├── QFramework.cs            # QFramework MVC 框架（第三方）
│   └── GameArchitecture.cs      # 架构入口，注册所有 Model/System
├── Data/                        # 数据配置
│   ├── CardData.cs              # 卡牌 ScriptableObject
│   ├── CardEffectSO.cs          # 卡牌效果基类
│   ├── DeckPresetData.cs        # 牌组预设配置
│   ├── EnemyData.cs             # 敌人配置
│   ├── MarkData.cs              # 印记配置
│   ├── BattleRewardConfigData.cs # 战斗奖励配置
│   ├── GameGlobalConfigData.cs  # 全局游戏配置
│   └── Effects/                 # 具体效果实现
│       ├── DamageEffectSO.cs    # 伤害效果
│       ├── HealEffectSO.cs      # 治疗效果
│       └── ApplyMarkEffectSO.cs # 施加印记效果
├── Editor/                      # Unity 编辑器扩展
│   ├── ConfigCenterWindow.cs    # Odin 配置中心
│   └── BattleRewardSetupUtility.cs # 默认奖励配置/UI 生成工具
├── Gameplay/                    # 游戏机制
│   ├── Battle/                  # 战斗系统
│   │   ├── BattleModel.cs       # 战斗状态数据
│   │   ├── DeckModel.cs         # 牌组状态数据
│   │   ├── BattleContext.cs     # 效果执行上下文
│   │   ├── BattleSystem.cs      # 战斗流程控制
│   │   ├── CardSystem.cs        # 抽/弃牌系统
│   │   └── Commands/            # 战斗命令
│   │       ├── StartBattleCommand.cs
│   │       ├── DrawCardCommand.cs
│   │       ├── PlayCardCommand.cs
│   │       ├── ReturnCardToHandCommand.cs
│   │       ├── SwapSlotsCommand.cs
│   │       ├── EndTurnCommand.cs
│   │       └── RedrawCardsCommand.cs
│   ├── Marks/                   # 印记系统
│   │   ├── MarkModel.cs         # 印记状态数据
│   │   ├── MarkSystem.cs        # 印记逻辑
│   │   └── MarkInstance.cs      # 印记运行时实例
│   ├── Rewards/                 # 战斗奖励系统
│   │   ├── BattleRewardModel.cs # 奖励待选状态
│   │   ├── BattleRewardSystem.cs # 奖励生成与领取
│   │   ├── BattleRewardOffer.cs # 一组多选一奖励
│   │   ├── BattleRewardOption.cs # 单个奖励选项
│   │   └── BattleRewardType.cs  # 奖励类型
│   ├── Enemy/                   # 敌人系统
│   │   └── IEnemyBehavior.cs    # 敌人行为接口
│   └── Events/
│       └── BattleEvents.cs      # 全部事件定义
├── UI/                          # UI 层
│   ├── BattleUIController.cs    # 战斗主 UI（HP/能量/按钮）
│   ├── EnemyController.cs       # 敌人视图 Controller
│   ├── HandViewController.cs    # 手牌区域管理
│   ├── HandDropZone.cs          # 拖放目标标记
│   ├── CardViewController.cs    # 单张手牌视图
│   ├── CardViewPool.cs          # 手牌 View 对象池
│   ├── CardSlotView.cs          # 出牌槽视图
│   ├── DeckPileView.cs          # 抽牌堆/弃牌堆按钮
│   ├── CardListPopupView.cs     # 卡牌列表弹窗
│   ├── CardListEntryView.cs     # 弹窗单条卡牌记录
│   ├── BattleRewardPopupView.cs # 战斗奖励选择弹窗
│   ├── BattleRewardOptionView.cs # 战斗奖励选项视图
│   └── EmptyGraphic.cs          # 空白可交互图形
├── Utilities/                   # 辅助工具类
└── GameManager.cs               # 游戏入口
```

### 数据目录 `Assets/Data/`

- `Preset/` - 存放配置文件，预定义游戏数据（CardData、DeckPresetData、EnemyData、MarkData）
- `Saves/` - 存放存档文件

---

## Model

| 类 | 文件 | 职责 |
|----|------|------|
| `BattleModel` | `Gameplay/Battle/BattleModel.cs` | 战斗状态：玩家 HP、能量、回合数、5 个出牌槽、重抽次数 |
| `DeckModel` | `Gameplay/Battle/DeckModel.cs` | 牌组状态：完整牌组、抽牌堆、手牌、弃牌堆 |
| `MarkModel` | `Gameplay/Marks/MarkModel.cs` | 印记状态：槽位印记字典、卡牌印记字典 |
| `BattleRewardModel` | `Gameplay/Rewards/BattleRewardModel.cs` | 奖励状态：当前待选奖励组、奖励批次编号 |

---

## System

| 类 | 文件 | 职责 |
|----|------|------|
| `BattleSystem` | `Gameplay/Battle/BattleSystem.cs` | 战斗流程：初始化、出牌/撤牌/换牌、结束回合、槽位效果结算 |
| `CardSystem` | `Gameplay/Battle/CardSystem.cs` | 牌操作：抽牌、洗牌、弃牌、重抽（Fisher-Yates 洗牌） |
| `MarkSystem` | `Gameplay/Marks/MarkSystem.cs` | 印记：施加、执行、回合推进（Tick）、清理 |
| `BattleRewardSystem` | `Gameplay/Rewards/BattleRewardSystem.cs` | 奖励：按配置生成奖励组、处理多选一领取、应用奖励 |

---

## Data（ScriptableObject）

| 类 | 菜单路径 | 说明 |
|----|---------|------|
| `CardData` | Card5/Card | 卡牌配置：名称、费用、图片、效果列表 |
| `CardEffectSO`（抽象） | — | 卡牌效果基类，实现 `Execute(BattleContext)` |
| `DamageEffectSO` | Card5/Effects/Damage | 造成伤害（可选目标：敌人/玩家） |
| `HealEffectSO` | Card5/Effects/Heal | 恢复 HP（可选目标：玩家/敌人） |
| `ApplyMarkEffectSO` | Card5/Effects/ApplyMark | 施加印记（当前槽/左槽/右槽/当前卡牌） |
| `DeckPresetData` | Card5/Deck Preset | 牌组预设，包含卡牌及数量列表 |
| `EnemyData` | Card5/Enemy | 敌人配置：名称、最大 HP、描述、头像 |
| `MarkData` | Card5/Mark | 印记配置：名称、图标、持续时间（-1=永久）、触发时机、效果列表 |
| `BattleRewardConfigData` | Card5/Battle Reward Config | 战斗奖励配置：每次奖励包含多个奖励组，当前支持卡牌奖励组三选一 |
| `GameGlobalConfigData` | Card5/Game Global Config | 全局游戏配置：启动牌组、敌人、奖励配置、玩家初始数值、目标帧率 |

---

## Editor 配置中心

入口：Unity 菜单 `Card5/配置中心`。

- `全局配置`：加载或创建 `Assets/Data/Preset/GameGlobalConfig.asset`，可从当前场景 `GameManager` 同步默认值，也可把全局配置应用回当前场景。
- `新建配置`：基于 Odin 下拉选择任意项目内 `ScriptableObject` 类型并创建 `.asset`。
- `配置概览`：统计 `Assets/Data` 下现有配置数量和类型分布。
- `全部配置/按目录` 与 `全部配置/按类型`：自动扫描并展示现有 `ScriptableObject` 配置，兼容当前所有数据资产。
- `表格视图`：按具体 `ScriptableObject` 类型生成批量编辑页，支持同类配置表格浏览、展开内联编辑、Project 多选全部、批量保存。

---

## Utility

> 目前 `Utilities/` 目录暂无自定义工具类，通用能力由以下方式提供：
> - `QFramework.cs` 内置 `BindableProperty<T>`、`TypeEventSystem`、`IocContainer`
> - `CardViewPool`（`UI/`）提供手牌 View 对象池

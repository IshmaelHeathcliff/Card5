---
description:
globs:
alwaysApply: false
---
# 属性系统指南

## 核心组件

### 属性配置系统
- [StatConfig.cs](mdc:Assets/Scripts/Data/Config/StatConfig.cs) - 属性配置定义
    - `StatType` 枚举：`Normal`（普通属性）、`Consumable`（消耗品属性）、`Keyword`（关键词属性）
    - `StatConfig` 类：包含 `ID`、`Name`、`Type`、`Description` 属性
    - 使用 `[ShowInInspector]` 标记在Inspector中显示

### 本地属性系统
- [LocalStat.cs](mdc:Assets/Scripts/GamePlay/Character/Stat/LocalStat.cs) - 本地属性实现
    - `LocalStat` - 基础本地属性类，结合本地和全局属性值
    - `LocalConsumableStat` - 消耗品属性实现（如生命值、蓝条）

### 消耗品属性 (LocalConsumableStat)
- 实现 `IConsumableStat` 和 `IReadonlyBindableProperty<float, float>` 接口
- **当前值管理**：
    - `CurrentValue` 属性自动限制在 `0` 到 `Value`（最大值）之间
    - 值变化时触发 `_onValueChanged` 事件，传递 `(currentValue, maxValue)`
- **构造和初始化**：
    - 构造函数接受 `IConsumableStat localStat` 和 `IConsumableStat globalStat`
    - 初始化时 `CurrentValue` 设置为 `Value`（满值状态）
- **方法说明**：
    - `ChangeCurrentValue(float value)` - 增减当前值
    - `SetCurrentValue(float value)` - 直接设置当前值
    - `SetMaxValue()` - 重置当前值为最大值
- **事件绑定**：
    - `Register(Action<float, float> onValueChanged)` - 注册值变化回调
    - `RegisterWithInitValue(...)` - 注册并立即触发一次回调
    - `UnRegister(...)` - 注销回调

## 属性类型说明

### Normal（普通属性）
- 基础数值属性，如攻击力、防御力、移动速度等
- 通常只有一个数值，不需要当前值/最大值的概念

### Consumable（消耗品属性）
- 具有当前值和最大值概念的属性，如生命值、魔法值、护盾值
- 支持消耗、恢复、设置最大值等操作
- 自动处理数值边界限制
- 提供值变化事件通知

### Keyword（关键词属性）
- 特殊标记性属性，通常用于表示特殊状态或能力
- 可能不包含数值，或者数值具有特殊含义

## 使用模式

### 创建消耗品属性

```csharp
// 创建生命值属性
var healthStat = new LocalConsumableStat(localHealthStat, globalHealthStat);

// 注册值变化监听（如更新UI血条）
healthStat.RegisterWithInitValue((current, max) => {
    UpdateHealthBar(current / max);
});
```

### 属性值操作

```csharp
// 造成伤害
healthStat.ChangeCurrentValue(-damage);

// 治疗
healthStat.ChangeCurrentValue(healAmount);

// 升级时增加最大生命值
healthStat.SetMaxValue(); // 重置为满血状态
```

## 配置文件结构

- 属性配置通过JSON文件定义
- 支持角色基础属性和技能相关属性的分别配置
- 配置文件位于 `Assets/Data/Preset/` 目录下

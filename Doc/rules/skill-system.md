---
description:
globs:
alwaysApply: false
---
# 技能系统指南

## 核心组件

### 技能池系统 (SkillPool)
- [SkillPool.cs](mdc:Assets/Scripts/GamePlay/Skill/SkillPool.cs) - 技能池实现
    - 使用 `Dictionary<SkillRarity, List<SkillConfig>>` 按稀有度分类存储技能
    - `PopRandomSkill(rarity)` 从指定稀有度随机抽取并移除技能
    - 返回 `null` 时表示该稀有度技能池为空
    - `GetCount()` 使用 `SelectMany` 展平所有稀有度列表并计数
    - `AddSkills/RemoveSkills` 使用 `All()` 进行批量操作验证

### 技能抽卡系统 (SkillGachaSystem)
- [SkillGachaSystem.cs](mdc:Assets/Scripts/GamePlay/Skill/SkillGachaSystem.cs) - 技能抽卡实现
    - `GachaSkills(model, count)` 根据权重随机抽取多个技能
    - 每次抽取前重新计算有效权重，确保不从空池抽取
    - `SelectSkill(model, configs, index)` 选择技能并将未选中的技能放回池中
    - 包含边界检查防止 `ArgumentOutOfRangeException`

### 技能池规则系统
- **添加规则 (SkillPoolAddRule)**: 满足条件时向技能池添加技能
    - `SpecificSkillsPoolAddRule` - 拥有指定技能组合时添加新技能
- **移除规则 (SkillPoolRemoveRule)**: 满足条件时从技能池移除技能
    - `SpecificSkillsPoolRemoveRule` - 拥有指定技能组合时移除旧技能
    - 被规则移除的技能不会在 `OnSkillRemoved` 事件中重新加回池中

### 技能释放条件系统
- [SkillReleaseConditions.cs](mdc:Assets/Scripts/GamePlay/Skill/SkillRelease/SkillReleaseConditions.cs)
    - `ISkillReleaseCondition` 接口包含 `IsMet(model)` 方法用于检查当前状态
    - `SpecificSkillsReleaseCondition` - 拥有指定技能组合时触发
    - `AnySkillsCountReleaseCondition` - 拥有指定数量技能时触发
    - `ValueCountCondition` - 计数值达到要求时触发
    - `CompositeAndReleaseCondition` - 所有子条件都满足时触发（使用实时检查）
    - `CompositeOrReleaseCondition` - 任一子条件满足时触发

## 常见问题解决

### 技能池为空导致的异常
- 问题：`PopRandomSkill` 在空池中抽取导致 `ArgumentOutOfRangeException`
- 解决：添加 `Count == 0` 检查，返回 `null`
- 后续处理：在 `GachaSkills` 中添加 `null` 检查，避免将空值加入结果列表

### 技能规则冲突
- 问题：`OnSkillRemoved` 将规则移除的技能重新加回池中
- 解决：在 `OnSkillRemoved` 中检查技能是否为规则移除目标，如果是则不加回池中

### 复合条件的状态检查
- 问题：`CompositeAndReleaseCondition` 使用过时的触发标志
- 解决：使用 `IsMet(model)` 方法进行实时状态检查，确保条件准确性

## 配置文件
- `Assets/Data/Preset/Skills.json` - 技能配置
- `Assets/Data/Preset/SkillPoolAddRules.json` - 技能池添加规则
- `Assets/Data/Preset/SkillPoolRemoveRules.json` - 技能池移除规则
- `Assets/Data/Preset/SkillReleaseRules.json` - 技能释放规则

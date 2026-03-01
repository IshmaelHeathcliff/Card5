using Core;
using Core.Scene;
using Gameplay.Character.Enemy;
using Gameplay.Status;
using Gameplay.Modifier;
using Gameplay.Character.Player;
using Gameplay.Damage.Attackers;
using Data.SaveLoad;
using Gameplay.Items;
using Gameplay.Skill;
using Gameplay.Character;

public class GameFrame : Architecture<GameFrame>
{
    protected override void Init()
    {
        RegisterModel(new PlayersModel());
        RegisterModel(new EnemiesModel());
        RegisterModel(new SceneModel());

        RegisterSystem(new InputSystem());
        RegisterSystem(new ModifierSystem());
        RegisterSystem(new StatusCreateSystem());
        RegisterSystem(new DropSystem());
        RegisterSystem(new SkillSystem());
        RegisterSystem(new SkillGachaSystem()); ;
        RegisterSystem(new SkillReleaseSystem());
        RegisterSystem(new ResourceSystem());
        RegisterSystem(new CountSystem());
        RegisterSystem(new AttackerSystem());
        RegisterSystem(new PositionQuerySystem());

        RegisterUtility(new SaveLoadUtility());
    }
}

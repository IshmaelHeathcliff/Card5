using Gameplay.Character;

namespace Gameplay.Skill
{
    public class SkillCommand : AbstractCommand
    {
        protected readonly ICharacterModel Model;

        public SkillCommand(ICharacterModel model) : base()
        {
            Model = model;
        }

        protected override void OnExecute()
        {
            this.GetSystem<SkillSystem>().SetEnv(Model);
        }
    }

    public class SetSkillSlotCountCommand : SkillCommand
    {
        readonly int _count;

        public SetSkillSlotCountCommand(int count, ICharacterModel model) : base(model)
        {
            _count = count;
        }

        protected override void OnExecute()
        {
            this.GetSystem<SkillSystem>().SetSkillSlotCount(_count, Model);
        }
    }

    public class AcquireSkillCommand : SkillCommand
    {
        readonly string _skillID;

        public AcquireSkillCommand(string skillID, ICharacterModel model) : base(model)
        {
            _skillID = skillID;
        }

        protected override void OnExecute()
        {
            this.GetSystem<SkillSystem>().AcquireSkill(_skillID, Model);
        }
    }

    public class ReleaseSkillCommand : SkillCommand
    {
        readonly string _skillID;

        public ReleaseSkillCommand(string skillID, ICharacterModel model) : base(model)
        {
            _skillID = skillID;
        }

        protected override void OnExecute()
        {
            this.GetSystem<SkillSystem>().ReleaseSkill(_skillID, Model);
        }
    }

    public class RemoveSkillCommand : SkillCommand
    {
        readonly string _skillID;

        public RemoveSkillCommand(string skillID, ICharacterModel model) : base(model)
        {
            _skillID = skillID;
        }

        protected override void OnExecute()
        {
            this.GetSystem<SkillSystem>().RemoveSkill(_skillID, Model);
        }
    }
}

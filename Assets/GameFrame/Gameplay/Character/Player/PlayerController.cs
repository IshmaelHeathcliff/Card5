using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Items;
using Gameplay.Modifier;
using Gameplay.Skill;
using Gameplay.Stat;
using UnityEngine;

namespace Gameplay.Character.Player
{
    public class PlayerController : MyCharacterController<PlayerModel, PlayersModel>
    {
        [SerializeField] Vector3 _initialPosition;

        public void Respawn()
        {
            Model.Position = _initialPosition;
            (CharaterStats.GetStat("Health") as IConsumableStat)?.SetMaxValue();
        }

        protected override void SetStats()
        {
            base.SetStats();
        }

        protected override void MakeSureID()
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = "player";
            }
        }

        protected override void OnInit()
        {
            SkillReleaseSystem skillReleaseSystem = this.GetSystem<SkillReleaseSystem>();
            skillReleaseSystem.RegisterConditions(Model);
            skillReleaseSystem.RegisterRelease(Model);
        }

        protected override void OnDeinit()
        {
            base.OnDeinit();
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            new ResourceGenerator(this.GetSystem<ResourceSystem>(), Model, 1f).StartGenerating(GlobalCancellation.GetCombinedTokenSource(this).Token).Forget();
        }
    }
}

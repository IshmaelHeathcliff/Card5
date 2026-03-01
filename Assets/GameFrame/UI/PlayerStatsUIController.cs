using System.Text;
using Gameplay.Stat;
using Gameplay.Character.Player;
using Sirenix.OdinInspector;
using Gameplay.Skill;
using TMPro;
using UnityEngine;
using Data.SaveLoad;
using System.Collections.Generic;
using Data.Config;

namespace UI
{
    public class PlayerStatsUIController : MonoBehaviour, IController
    {
        [SerializeField] TextMeshProUGUI _text;

        PlayerModel _playerModel;
        List<StatConfig> _characterStats;

        [Button]
        // TODO: 优化属性更新方式，不再一次性更新所有
        void UpdateStatsInfo()
        {
            var info = new StringBuilder();

            foreach (StatConfig stat in _characterStats)
            {
                info.Append(Stats.GenerateStatInfo(_playerModel.Stats.GetStat(stat.ID)));
            }

            _text.text = info.ToString();
        }



        void OnAttackSkillAcquired(SkillAcquiredEvent e)
        {
            if (e.Model != _playerModel)
            {
                return;
            }

            if (e.Skill is not AttackSkill attackSkill)
            {
                return;
            }

            foreach (IStat stat in attackSkill.SkillStats.GetAllStats())
            {
                stat.Register(UpdateStatsInfo);
            }
        }

        void Awake()
        {
            _characterStats = SaveLoadManager.Load<List<StatConfig>>("CharacterStats.json", "Preset");
            this.RegisterEvent<SkillAcquiredEvent>(OnAttackSkillAcquired).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void Start()
        {
            _playerModel = this.GetModel<PlayersModel>().Current;
            foreach (IStat stat in _playerModel.Stats.GetAllStats())
            {
                stat.Register(UpdateStatsInfo);
            }


            UpdateStatsInfo();
        }

        void OnValidate()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}

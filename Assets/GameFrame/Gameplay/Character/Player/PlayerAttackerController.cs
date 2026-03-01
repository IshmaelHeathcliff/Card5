using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Damage;
using Gameplay.Damage.Attackers;
using UnityEngine.InputSystem;
using InputSystem = Core.InputSystem;

namespace Gameplay.Character.Player
{
    public class PlayerAttackerController : AttackerController
    {
        PlayerInput.PlayerActions _playerInput;

        public override async UniTask<List<IAttacker>> GetAttackers(string skillID, string attackerID)
        {
            List<IAttacker> attackers = await base.GetAttackers(skillID, attackerID);

            if (attackerID == "self")
            {
                return attackers;
            }

            // 向最近敌人位置发射
            List<string> selected = new();
            foreach (IAttacker attacker in attackers)
            {
                attacker.Target = this.GetSystem<PositionQuerySystem>().QueryClosest(TargetTag, transform.position, selected);
                if (attacker.Target != null)
                {
                    selected.Add(attacker.Target.GetComponentInChildren<Damageable>().ID);
                }
            }

            AttackerParent.DetachChildren();
            return attackers;
        }

        void AttackAction(InputAction.CallbackContext context)
        {
            // Attack().Forget();
        }

        void RegisterActions()
        {
            _playerInput.Attack.performed += AttackAction;
        }

        void UnregisterActions()
        {
            _playerInput.Attack.performed -= AttackAction;
        }

        void OnEnable()
        {
            _playerInput = this.GetSystem<InputSystem>().PlayerActionMap;
            RegisterActions();
        }

        void OnDisable()
        {
            UnregisterActions();
        }
    }
}

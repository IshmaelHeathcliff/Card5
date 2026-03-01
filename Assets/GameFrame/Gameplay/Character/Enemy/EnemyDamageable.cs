using System;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Character.Player;
using Gameplay.Damage;
using Gameplay.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gameplay.Character.Enemy
{
    public class EnemyDamageable : Damageable
    {
        FSM<EnemyStateID> _fsm;
        DropSystem _dropSystem;

        protected override void OnInit()
        {
            base.OnInit();
            OnHurt = new EasyEvent();
            OnDeath = new EasyEvent();
            _fsm = (CharacterController as IHasFSM<EnemyStateID>)?.FSM;
            _dropSystem = this.GetSystem<DropSystem>();
        }


        void Start()
        {
            SetStats(CharacterController.CharaterStats);

            OnHurt.Register(Hurt).UnRegisterWhenDisabled(this);
            OnDeath.Register(Dead).UnRegisterWhenDisabled(this);
        }

        public override void TakeDamage(float damage)
        {
            if (!IsDamageable)
            {
                return;
            }

            Health.ChangeCurrentValue(-damage);
            // Debug.Log($"TakeDamage: {damage}, Left Health: {Health.CurrentValue}");
            _fsm.ChangeState(EnemyStateID.Hurt);
            OnHurt.Trigger();
        }

        void Hurt()
        {
        }


        void Dead()
        {
            PlayerModel playerModel = this.GetModel<PlayersModel>().Current;
            this.GetSystem<CountSystem>().IncrementKillCount(playerModel, 1);

            this.GetSystem<ResourceSystem>().AcquireResource("Coin", (int)playerModel.Stats.GetStat("CoinOnKill").Value, playerModel);
            this.GetSystem<ResourceSystem>().AcquireResource("Wood", (int)playerModel.Stats.GetStat("WoodOnKill").Value, playerModel);
        }
    }
}

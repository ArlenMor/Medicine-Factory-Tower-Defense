using System;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public interface IFieldPlaceable
    {
        bool IsDead { get; }
        Transform transform { get; }
        event Action<IFieldPlaceable> OnPlaceableDestroyed;
        void TakeDamage(float damage);
    }
}

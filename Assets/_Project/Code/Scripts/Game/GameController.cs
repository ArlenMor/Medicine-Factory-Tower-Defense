using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using UnityEngine;

namespace _Project.Code.Scripts.Game
{
    public class GameController : MonoBehaviour, IManualUpdateRegistrar {
        
        private List<IManualUpdate> _manualUpdates = new();
        private readonly List<IManualUpdate> _pendingAdd = new();
        private readonly List<IManualUpdate> _pendingRemove = new();
        private bool _paused;

        public void ManualAwake(List<IManualUpdate> manualUpdates) {
            _manualUpdates = manualUpdates;

            ManualStart();
        }

        private void ManualStart()
        {
        }

        public void Register(IManualUpdate manualUpdate)
        {
            _pendingAdd.Add(manualUpdate);
        }

        public void Unregister(IManualUpdate manualUpdate)
        {
            _pendingRemove.Add(manualUpdate);
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
        }

        public void Update()
        {
            if (_paused) return;

            GameData.Instance.Stats.TimePlayed += Time.deltaTime;

            if (_pendingAdd.Count > 0)
            {
                _manualUpdates.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            if (_pendingRemove.Count > 0)
            {
                foreach (var item in _pendingRemove)
                    _manualUpdates.Remove(item);
                _pendingRemove.Clear();
            }

            foreach (var manualUpdate in _manualUpdates)
            {
                manualUpdate.ManualUpdate(Time.deltaTime);
            }
        }
    }

    public interface IManualUpdateRegistrar
    {
        void Register(IManualUpdate manualUpdate);
        void Unregister(IManualUpdate manualUpdate);
    }
}
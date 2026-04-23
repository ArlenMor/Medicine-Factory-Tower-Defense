using _Project.Code.Scripts.Bootstrap;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Game.LvlController;
using UnityEngine;

namespace _Project.Code.Scripts.Game
{
    public class GameController : MonoBehaviour, IGamePauseHandler {
        private bool _paused;
        private LevelController _levelController;

        public void ManualAwake(GameContext context) {
            _levelController = new LevelController(context.ManualUpdates, context.PlayerDamageEventProvider, context.TaskService, this, context.);
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
        }

        public void Update()
        {
            if (_paused) return;

            GameData.Instance.Stats.TimePlayed += Time.deltaTime;

            _levelController.ManualUpdate(Time.deltaTime);
        }
    }

    public interface IManualUpdateRegistrar
    {
        void Register(IManualUpdate manualUpdate);
        void Unregister(IManualUpdate manualUpdate);
    }
}
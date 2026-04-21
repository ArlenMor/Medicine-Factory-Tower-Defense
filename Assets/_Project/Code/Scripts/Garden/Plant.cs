using System;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Timer;
using UnityEngine;

namespace _Project.Code.Scripts.Garden
{
    public class Plant: MonoBehaviour
    {
        private GameConfig _config;
        private ITimerService _timerService;

        private TimerHandle _timer;
        private bool _isOnHalfGrown;
        private Action _grownAction;

        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private SpriteRenderer _image;

        public PlantType Type;

        public bool IsGrown { get; private set; }

        public void Initialize(GameConfig config, ITimerService timerService, Action grownAction, bool isAlreadyGrown = false)
        {
            _timerService = timerService;
            _config = config;
            _grownAction = grownAction;

            if (isAlreadyGrown)
            {
                OnGrown();
            }
            else
            {
                _image.sprite = _sprites[0];
                _timer = _timerService.Start(GetGrowthTime(), OnGrown, OnHalfGrown);
            }
        }

        private void OnHalfGrown()
        {
            if (_isOnHalfGrown) return;
            
            _isOnHalfGrown = true;
            _image.sprite = _sprites[1];
        }
        
        private void OnGrown()
        {
            IsGrown = true;
            _image.sprite = _sprites[2];
            _grownAction?.Invoke();
        }

        private float GetGrowthTime()
        {
            var multiplier = GameData.Instance.UpgradesData[UpgradeType.GrowSpeed].Multiplier;
            return _config.GardenConfig.GetGrowableResourceData(Type.GetResourceType()).GrowthTime / multiplier;
        }
    }
}
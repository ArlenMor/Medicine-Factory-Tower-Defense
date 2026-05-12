using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Game;
using UnityEngine;

namespace _Project.Code.Scripts.EnemySystem
{
    public class WaveSpawner : MonoBehaviour, IManualUpdate
    {
        [SerializeField] private Transform _spawnPointA;
        [SerializeField] private Transform _spawnPointB;
        [SerializeField] private BrainView _centerTarget;
        private EnemyConfig _enemyConfig;
        private WaveConfig _waveConfig;

        private float _gameTime;
        private int _currentWaveIndex;
        private readonly List<Enemy> _activeEnemies = new();

        private bool _isSpawningWave;
        private readonly Queue<EnemyType> _spawnQueue = new();
        private float _intraSpawnTimer;
        private float _currentIntraSpawnInterval;

        public void ManualAwake(EnemyConfig enemyConfig)
        {
            _enemyConfig = enemyConfig;
        }

        public void StartLevel(WaveConfig waveConfig)
        {
            _waveConfig = waveConfig;
            _gameTime = 0f;
            _currentWaveIndex = 0;
            _isSpawningWave = false;
            _spawnQueue.Clear();
        }

        public void ManualUpdate(float deltaTime)
        {
            _gameTime += deltaTime;

            TryStartNextWave();
            ProcessSpawnQueue(deltaTime);
            UpdateEnemies(deltaTime);
        }

        private void TryStartNextWave()
        {
            if (_waveConfig == null) return;
            if (_currentWaveIndex >= _waveConfig.Waves.Count) return;
            if (_isSpawningWave) return;

            var wave = _waveConfig.Waves[_currentWaveIndex];
            if (_gameTime >= wave.StartTime)
                StartWave(wave);
        }

        private void StartWave(WaveData wave)
        {
            _spawnQueue.Clear();

            var list = new List<EnemyType>(wave.ScoutCount + wave.GnawerCount + wave.TankCount);

            for (int i = 0; i < wave.ScoutCount; i++)
                list.Add(EnemyType.Scout);
            for (int i = 0; i < wave.GnawerCount; i++)
                list.Add(EnemyType.Gnawer);
            for (int i = 0; i < wave.TankCount; i++)
                list.Add(EnemyType.Tank);

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            foreach (var type in list)
                _spawnQueue.Enqueue(type);

            _currentIntraSpawnInterval = wave.IntraSpawnInterval;
            _intraSpawnTimer = 0f;
            _isSpawningWave = true;
        }

        private void ProcessSpawnQueue(float deltaTime)
        {
            if (!_isSpawningWave) return;

            _intraSpawnTimer -= deltaTime;
            if (_intraSpawnTimer > 0f) return;

            if (_spawnQueue.Count > 0)
            {
                SpawnEnemy(_spawnQueue.Dequeue());
                _intraSpawnTimer = _currentIntraSpawnInterval;
            }

            if (_spawnQueue.Count == 0)
            {
                _isSpawningWave = false;
                _currentWaveIndex++;
            }
        }

        private void SpawnEnemy(EnemyType type)
        {
            var stats = _enemyConfig.GetStats(type);
            var spawnPoint = Vector3.Lerp(_spawnPointA.position, _spawnPointB.position, Random.value);

            var go = Instantiate(stats.Prefab, spawnPoint, Quaternion.identity, transform);
            var enemy = go.GetComponent<Enemy>();
            enemy.Initialize(stats, _centerTarget);
            enemy.OnDied += HandleEnemyDied;
            _activeEnemies.Add(enemy);
        }

        private void UpdateEnemies(float deltaTime)
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                if (_activeEnemies[i] == null)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }

                _activeEnemies[i].Tick(deltaTime);
            }
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            enemy.OnDied -= HandleEnemyDied;
            _activeEnemies.Remove(enemy);
            GameData.Instance.Stats.EnemiesKilled++;
        }
    }
}

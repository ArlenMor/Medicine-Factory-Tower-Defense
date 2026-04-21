using UnityEngine;

namespace _Project.Code.Scripts.EnemySystem
{
    public class EnemyAnimator : MonoBehaviour
    {
        [Header("Walk")]
        [SerializeField] private float _wobbleSpeed = 8f;
        [SerializeField] private float _wobbleAmount = 0.1f;

        [Header("Idle")]
        [SerializeField] private float _idleSpeed = 2.5f;
        [SerializeField] private float _idleAmount = 0.04f;

        [Header("Lunge")]
        [SerializeField] private float _lungeDistance = 0.3f;
        [SerializeField] private float _lungeDuration = 0.35f;

        private float _wobblePhase;
        private float _idlePhase;
        private float _lungeTimer;
        private bool _isLunging;
        private Vector3 _lungeDir;
        private Vector3 _restPosition;

        public bool IsLunging => _isLunging;

        public void TickWalk(float deltaTime)
        {
            _wobblePhase += deltaTime * _wobbleSpeed;
            float sin = Mathf.Sin(_wobblePhase);
            transform.localScale = new Vector3(1f + sin * _wobbleAmount, 1f - sin * _wobbleAmount, 1f);
        }

        public void TickIdle(float deltaTime)
        {
            _idlePhase += deltaTime * _idleSpeed;
            float pulse = Mathf.Sin(_idlePhase) * _idleAmount;
            transform.localScale = new Vector3(1f + pulse, 1f - pulse * 0.5f, 1f);
        }

        public void StartLunge(Vector3 targetPos)
        {
            _restPosition = transform.position;
            _lungeDir = (targetPos - transform.position).normalized;
            _isLunging = true;
            _lungeTimer = 0f;
        }

        public void TickLunge(float deltaTime)
        {
            if (!_isLunging) return;

            _lungeTimer += deltaTime;
            float t = Mathf.Clamp01(_lungeTimer / _lungeDuration);
            float offset = Mathf.Sin(t * Mathf.PI) * _lungeDistance;
            transform.position = _restPosition + _lungeDir * offset;

            if (t >= 1f)
            {
                transform.position = _restPosition;
                _isLunging = false;
            }
        }

        public void ResetScale()
        {
            transform.localScale = Vector3.one;
        }
    }
}

namespace AI.Core
{
    using AI.Interface;
    using AI.States;
    using UnityEngine;
    using UnityEngine.AI;

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class NpcBehaviorLogic : MonoBehaviour, INpcController
    {
        [SerializeField] private Npc _data;
        [SerializeField] private Faction _faction = Faction.Enemy;
        [SerializeField] private float _detectRadius = 10f;

        [SerializeField] private Animator _animator = null;
        [SerializeField] private NavMeshAgent _agent = null;


        private IState _currentState = null;
        private INpcFactory _factory = null;
        private int _maxHealth = 0;
        private bool _defending = false;
        private NpcHandle _handle;
        private float _cellUpdateTick = 0;

        public Npc Data => _data;
        public int Generation { get; private set; }

        public void Initialize(NpcData data, Faction faction, INpcFactory factory)
        {
            _data = new Npc(data.Health, data.Damage, data.AttackRange, data.RetreatRange, data.HitDelay);
            _faction = faction;
            _factory = factory;
            _maxHealth = data.Health;
            _defending = false;
            _currentState = null;
        }

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_maxHealth == 0) _maxHealth = _data.Health;
        }

        private void OnEnable()
        {
            Generation++;
            _handle = new NpcHandle(GetInstanceID(), Generation, this, _faction);
            _cellUpdateTick = Random.Range(.905f, 1.005f);
            NpcLocator.Register(_handle);
            ChangeState(new Idle());
        }

        private void OnDisable()
        {
            NpcLocator.Unregister(_handle);
        }

        private void Update()
        {
            UpdateCell();

            _currentState?.Update(this);
        }

        private void UpdateCell()
        {
            if (_currentState is not (Chase or Retreat)) return;

            _cellUpdateTick -= Time.deltaTime;
            if (_cellUpdateTick > 0) return;

            NpcLocator.UpdateCell(_handle);
            _cellUpdateTick = Random.Range(.905f, 1.005f);
        }

        public void ChangeState(IState newState)
        {
            if (newState == null) return;

            _currentState?.Exit(this);
            _currentState = newState;
            _currentState.Enter(this);
        }

        public Animator GetAnimator()
        {
            return _animator;
        }

        public NavMeshAgent GetAgent()
        {
            return _agent;
        }

        public bool TryFindEnemy(out NpcHandle enemy)
        {
            return NpcLocator.TryFindNearestEnemy(transform.position, _detectRadius, _faction, out enemy);
        }

        public void SetDefending(bool value)
        {
            _defending = value;
        }

        public void ActionDone()
        {
            Think();
        }

        private void Think()
        {
            if (!TryFindEnemy(out var enemy))
            {
                if (_currentState is not Idle) ChangeState(new Idle());
                return;
            }

            var enemyData = enemy.Co.Data;
            float hp = _maxHealth > 0 ? (float)_data.Health / _maxHealth : 1f;
            float distance = Vector3.Distance(transform.position, enemy.Position);
            bool canKill = enemyData.Health <= _data.Damage;
            bool inDanger = enemyData.Damage >= _data.Health;
            bool threatened = distance <= _data.AttackRange * 2;

            float attack = 0.4f + hp * 0.6f + (canKill ? 0.5f : 0f) + Random.Range(0f, 0.2f);
            float retreat = (1f - hp) * 1.2f + (inDanger ? 0.3f : 0f) + Random.Range(0f, 0.2f);
            float defend = threatened ? 0.2f + (1f - hp) * 0.5f + (inDanger ? 0.4f : 0f) + Random.Range(0f, 0.2f) : 0f;

            if (attack >= retreat && attack >= defend)
            {
                if (distance <= _data.AttackRange) ChangeState(new Attack(enemy));
                else ChangeState(new Chase(enemy));
            }
            else if (retreat >= defend)
            {
                ChangeState(new Retreat(enemy));
            }
            else
            {
                ChangeState(new Defend());
            }
        }

        public void Despawn()
        {
            if (_factory != null) _factory.Despawn(gameObject);
            else Destroy(gameObject);
        }

        public void ApplyDamage(int amount)
        {
            if (!_data.IsAlive) return;

            if (_defending) amount /= 5;

            _data.TakeHit(amount);

            if (!_data.IsAlive)
            {
                NpcLocator.Unregister(_handle);
                ChangeState(new Dead());
            }
        }

        public void ApplyHeal(int amount)
        {
            _data.TakeHeal(amount);
        }
    }

    public enum Faction { Neutral, Player, Enemy }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Legacy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Patrol : MonoBehaviour
    {
        [SerializeField] private Transform[] _waypoints;

        private NavMeshAgent _agent;

        private Vector3[] _cachedWaypoints;
        private int _currentWaypoint;

        private bool _isPatrolling = true;

        private Vector3 Target => GetPosition();

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.SetDestination(Target);
        }

        private void Update()
        {
            if (_isPatrolling == false)
                return;

            if (IsDestinationReached() == false)
                return;

            _currentWaypoint = (_currentWaypoint + 1) % _cachedWaypoints.Length;
            _agent.SetDestination(Target);
        }

        public void StartPatrol()
        {
            if (_isPatrolling)
                return;

            _isPatrolling = true;
        }

        public void StopPatrol()
        {
            if (_isPatrolling == false)
                return;

            _agent.SetDestination(transform.position);
            _isPatrolling = false;
        }

        private Vector3 GetPosition()
        {
            _cachedWaypoints ??= new List<Vector3>(_waypoints.Select(x => x.position)).ToArray();
            return _cachedWaypoints[_currentWaypoint];
        }

        private bool IsDestinationReached() => _agent.remainingDistance <= _agent.stoppingDistance;
    }
}
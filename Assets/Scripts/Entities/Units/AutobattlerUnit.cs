using System;
using System.Threading;
using System.Threading.Tasks;
using Managers;
using Pathfinding;
using UnityEngine;

namespace Entities.Unit
{
    public class AutobattlerUnit: MonoBehaviour
    {
        [SerializeField]
        private AIPath AIPath;

        [SerializeField]
        private AIDestinationSetter _destinationSetter;

        [SerializeField] public Transform[] slots;

        private CancellationTokenSource _cancellationToken;

        public UnitTrajectory? activeTrajectory;
        private void Start()
        {
            UnitManager.Instance.RegisterToList(this);
            OnTargetReachedAsync();
        }

        public void OnTargetReached(AutobattlerUnit target)
        {
            _destinationSetter.enabled = false;
            var position = target.transform.position;
            var targetSlot = slots[0].position;
            var shortestDistance = Vector3.Distance(targetSlot, position);
            //TODO: Iterate through ALL available slots from both units to determine the closest distance to a valid slot.
            foreach (var slot in slots)
            {
                var distance = Vector3.Distance(position, target.transform.position);
                if (distance >= shortestDistance) continue;
                shortestDistance = distance;
                targetSlot = slot.position;
            }

            target.MoveTo(targetSlot);
            target.AIPath.canMove = false;
            AIPath.canMove = false;
        }

        private async void OnTargetReachedAsync()
        {
            _cancellationToken = new CancellationTokenSource();
            Debug.Log("Async Task Started on: " + gameObject);
            activeTrajectory = UnitManager.Instance.RegisterTrajectory(gameObject.transform, _destinationSetter.target);
            try
            {
                Debug.Log($"ReachedEndOfPath? {AIPath.reachedEndOfPath}");
                while (!AIPath.reachedEndOfPath)
                {
                    await Task.Yield();
                }
                Debug.Log($"Reached end of path for new ai");
                UnitManager.Instance.AdjustUnitPositionOnTargetReached(activeTrajectory);
            }
            catch (Exception error)
            {
                Debug.Log($"Task was cancelled! Reason: {error}");
                return;
            }
            finally
            {
                _cancellationToken.Dispose();
                _cancellationToken = null;
            }
            Debug.Log("Async Task Ended on: " + gameObject);
           
        }

        public void TurnAI_On()
        {
            _destinationSetter.enabled = true;
        }

        public void TurnAI_Off()
        {
            _destinationSetter.enabled = false;
        }

        public void Cancel()
        {
            _cancellationToken.Cancel();
        }

        private async void MoveTo(Vector3 position)
        {
            var speed = 2;
            var duration = 1f;
            var elapsedTime = 0f;
            var normalizedTime = 0f;
            var originalPosition = transform.position;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime * speed;
                normalizedTime = elapsedTime / duration;
                transform.position = Vector3.Lerp(originalPosition, position, normalizedTime);
                await Task.Yield();
            }
        }
    }
}
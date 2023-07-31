using System;
using System.Threading;
using System.Threading.Tasks;
using Managers;
using Pathfinding;
using UnityEngine;

namespace Entities.Unit
{
    public class OnEndReached: MonoBehaviour
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
            MyTaskAsync();
        }

        private void OnTargetReached(Collision target)
        {
            _destinationSetter.enabled = false;
            var position = target.transform.position;
            var targetPos = position;
            var shortestDistance = Vector3.Distance(targetPos, position);
            var targetSlot = slots[0].position;
            foreach (var slot in slots)
            {
                var distance = Vector3.Distance(targetPos, target.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetSlot = slot.position;
                }
            }

            target.transform.position = targetSlot;
        }

        public void OnTargetReached(OnEndReached target)
        {
            _destinationSetter.enabled = false;
            var position = target.transform.position;
            var shortestDistance = Vector3.Distance(position, position);
            var targetSlot = slots[0].position;
            foreach (var slot in slots)
            {
                var distance = Vector3.Distance(position, target.transform.position);
                if (!(distance < shortestDistance)) continue;
                shortestDistance = distance;
                targetSlot = slot.position;
            }

            target.MoveTo(targetSlot);
            target.AIPath.canMove = false;
            AIPath.canMove = false;
        }

        private void OnCollisionEnter(Collision other)
        {
            
            //OnTargetReached(other);
        }

        private async void MyTaskAsync()
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

        public async void MoveTo(Vector3 position)
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
using System.Collections.Generic;
using Entities.Unit;
using UnityEngine;

namespace Managers
{
    public struct UnitTrajectory
    {
        public OnEndReached targetingUnit;
        public OnEndReached targetedUnit;
    }
    public class UnitManager : MonoBehaviour
    {
        public static UnitManager Instance;
        public List<OnEndReached> Units = new();
        public List<UnitTrajectory> Trajectories = new List<UnitTrajectory>();

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterToList(OnEndReached unit)
        {
            Units.Add(unit);
        }

        public UnitTrajectory RegisterTrajectory(OnEndReached targetingUnit, OnEndReached targetedUnit)
        {
            var trajectory = new UnitTrajectory() { targetingUnit = targetingUnit, targetedUnit = targetedUnit };
            Trajectories.Add(trajectory);
            return trajectory;
        }

        public UnitTrajectory RegisterTrajectory(Transform targetingUnit, Transform targetedUnit)
        {
            var targeting = targetingUnit.GetComponent<OnEndReached>();
            var targeted = targetedUnit.GetComponent<OnEndReached>();

            return RegisterTrajectory(targeting, targeted);
        }

        public void OnTrajectoryCompletedOrCanceled(UnitTrajectory trajectory)
        {
            Trajectories.Remove(trajectory);
        }

        public void AdjustUnitPositionOnTargetReached(UnitTrajectory? _trajectory)
        {
            if (_trajectory == null) return;
            var trajectory = (UnitTrajectory)_trajectory;
            var targetingUnit = trajectory.targetingUnit;
            var targetedUnit = trajectory.targetedUnit;

            if (targetedUnit.activeTrajectory != null)
            {
                targetedUnit.Cancel();
                OnTrajectoryCompletedOrCanceled((UnitTrajectory)targetedUnit.activeTrajectory);
                targetedUnit.activeTrajectory = null;
                targetedUnit.TurnAI_Off();
            }
            
            targetingUnit.OnTargetReached(targetedUnit);
            OnTrajectoryCompletedOrCanceled(trajectory);
            targetingUnit.activeTrajectory = null;

        }
    }
}
using Managers;
using UnityEngine;

namespace Grid
{
    public class GridCell : MonoBehaviour
    {
        public Vector2Int GridPosition;

        public void RegisterSelf()
        {
            GridManager.Instance.RegisterCell(this);
        }
    }
}


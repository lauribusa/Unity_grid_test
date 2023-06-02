using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class GridGenerator : MonoBehaviour
    {
        public Vector2Int GridSize;
        public Vector2 GridCellOffset = new Vector2(0.5f, 0);

        [SerializeField]
        private GameObject _cellPrefab;
        private void Awake()
        {
            GenerateGrid();
        }

        public void GenerateGrid()
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    Vector3 position;
                    if(y % 2 == 0)
                    {
                        position = new Vector3(x, -y * 0.75f, 0) + (Vector3)GridCellOffset;
                    } else
                    {
                        position = new Vector3(x, -y * 0.75f, 0);
                    }
                    GameObject cellObject = Instantiate(_cellPrefab, position + transform.position, Quaternion.identity, transform);
                    cellObject.name = $"Cell_{x}:{y}";
                    var cell = cellObject.AddComponent<GridCell>();
                    cell.GridPosition = new(x, y);
                    cell.RegisterSelf();
                }
            }
        }
    }
}


using Grid;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class GridManager : MonoBehaviour
    {
        private static GridManager _instance;
        public static GridManager Instance { get { return _instance; } private set { _instance = value; } }

        private List<GridCell> _cells = new();

        [SerializeField]
        private GridGraph _graph;

        private void Awake()
        {
            if (FindObjectsOfType<GridManager>().Length > 1)
            {
                Debug.Log("Found another instance of GridManager. Deleting");
                Destroy(this.gameObject);
            }
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            _instance = this;
        }

        /*private void OnDrawGizmos()
        {
            // This holds all graph data
            AstarData data = AstarPath.active.data;

            if(data.graphs.Length > 0)
            {
                return;
            }
            // This creates a Grid Graph
            GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
            

            // Setup a grid graph with some values
            int width = 30;
            int depth = 30;
            float nodeSize = 1;

            gg.center = new Vector3(10, 0, 0);

            // Updates internal size from the above values
            gg.SetDimensions(width, depth, nodeSize);
            gg.is2D = true;
            gg.SetGridShape(InspectorGridMode.Hexagonal);
            
            // Scans all graphs
            AstarPath.active.Scan();
            Debug.Log("Node set");
        }*/

        public void RegisterCell(GridCell gridCell)
        {
            _cells.Add(gridCell);
        }
    }
}


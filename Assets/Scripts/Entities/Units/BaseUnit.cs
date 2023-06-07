using Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

namespace Entities.Unit
{
    public class BaseUnit : MonoBehaviour
    {
        public Transform targetPosition;

        [SerializeField]
        private Seeker seeker;

        public Path path;

        public float speed = 2;

        public float nextWaypointDistance = 3;

        private int currentWaypoint = 0;

        public bool reachedEndOfPath;

        public float repathRate = 0.5f;

        private BlockManager.TraversalProvider traversalProvider;
        private float lastRepath = float.NegativeInfinity;

        [SerializeField]
        private SingleNodeBlocker Blocker;

        public List<SingleNodeBlocker> obstacles;

        public void Start()
        {

            // Start to calculate a new path to the targetPosition object, return the result to the OnPathComplete method.
            // Path requests are asynchronous, so when the OnPathComplete method is called depends on how long it
            // takes to calculate the path. Usually it is called the next frame.
            GetBlockManager();
            seeker.pathCallback += OnPathComplete;
            StartPath();
        }

        private void StartPath()
        {
            var path = ABPath.Construct(transform.position, targetPosition.position, null);

            // Make the path use a specific traversal provider
            path.traversalProvider = traversalProvider;

            // Calculate the path synchronously
            //AstarPath.StartPath(path);
            seeker.StartPath(transform.position, (Vector3)AstarPath.active.GetNearest(targetPosition.position, NNConstraint.Default).node.position);
            path.BlockUntilCalculated();
            if (path.error)
            {
                Debug.Log("No path was found");
            }
            else
            {
                Debug.Log("A path was found with " + path.vectorPath.Count + " nodes");

                // Draw the path in the scene view
                for (int i = 0; i < path.vectorPath.Count - 1; i++)
                {
                    Debug.DrawLine(path.vectorPath[i], path.vectorPath[i + 1], Color.red);
                }
            }
        }

        private void OnDisable()
        {
            seeker.pathCallback-= OnPathComplete;
        }

        private void GetBlockManager()
        {
            var blockManager = ServiceProvider.Instance.BlockManager;
            Blocker.manager = blockManager;
            traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.OnlySelector, obstacles);
        }

        public void OnPathComplete(Path p)
        {
            Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

            // Path pooling. To avoid unnecessary allocations paths are reference counted.
            // Calling Claim will increase the reference count by 1 and Release will reduce
            // it by one, when it reaches zero the path will be pooled and then it may be used
            // by other scripts. The ABPath.Construct and Seeker.StartPath methods will
            // take a path from the pool if possible. See also the documentation page about path pooling.
            p.Claim(this);
            if (!p.error)
            {
                if (path != null) path.Release(this);
                path = p;
                // Reset the waypoint counter so that we start to move towards the first point in the path
                currentWaypoint = 0;
            }
            else
            {
                p.Release(this);
            }
        }

        public void Update()
        {

            Blocker.BlockAtCurrentPosition();

            if (Time.time > lastRepath + repathRate && seeker.IsDone())
            {
                lastRepath = Time.time;

                // Start a new path to the targetPosition, call the the OnPathComplete function
                // when the path has been calculated (which may take a few frames depending on the complexity)
                StartPath();
            }

            if (path == null)
            {
                // We have no path to follow yet, so don't do anything
                return;
            }


            // Check in a loop if we are close enough to the current waypoint to switch to the next one.
            // We do this in a loop because many waypoints might be close to each other and we may reach
            // several of them in the same frame.
            reachedEndOfPath = false;
            // The distance to the next waypoint in the path
            float distanceToWaypoint;
            while (true)
            {
                // If you want maximum performance you can check the squared distance instead to get rid of a
                // square root calculation. But that is outside the scope of this tutorial.
                distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
                if (distanceToWaypoint < nextWaypointDistance)
                {
                    // Check if there is another waypoint or if we have reached the end of the path
                    if (currentWaypoint + 1 < path.vectorPath.Count-1)
                    {
                        currentWaypoint++;
                    }
                    else
                    {
                        // Set a status variable to indicate that the agent has reached the end of the path.
                        // You can use this to trigger some special code if your game requires that.
                        reachedEndOfPath = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // Slow down smoothly upon approaching the end of the path
            // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
            var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

            // Direction to the next waypoint
            // Normalize it so that it has a length of 1 world unit
            Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            // Multiply the direction by our desired speed to get a velocity
            Vector3 velocity = dir * speed * speedFactor;

            // If you are writing a 2D game you may want to remove the CharacterController and instead modify the position directly
            transform.position += velocity * Time.deltaTime;
        }

    }
}


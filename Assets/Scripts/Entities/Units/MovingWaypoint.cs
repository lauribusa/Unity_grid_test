using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWaypoint : MonoBehaviour
{
    public float movingRefreshRate = 2;

    public List<Transform> waypoints = new List<Transform>();

    private Queue<Transform> transforms = new Queue<Transform>();

    private float elapsedTime = 0f;

    private Transform activeTransform;

    [SerializeField]
    private Seeker seeker;

    public Path path;

    public float speed = 2;

    public float nextWaypointDistance = 3;

    private int currentWaypoint = 0;

    public bool reachedEndOfPath;

    public float repathRate = 0.5f;

    private void Awake()
    {
        foreach (var waypoint in waypoints)
        {
            transforms.Enqueue(waypoint);
        }
    }

    private void Start()
    {
        seeker.pathCallback += OnPathComplete;
        MoveToNextWaypoint();
    }
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= movingRefreshRate && seeker.IsDone() && reachedEndOfPath)
        {
            elapsedTime= 0f;
            MoveToNextWaypoint();
            
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
                if (currentWaypoint + 1 < path.vectorPath.Count)
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

    private void MoveToNextWaypoint()
    {
        var nextTransform = transforms.Dequeue();
        if(activeTransform!= null )
        {
            transforms.Enqueue(activeTransform);
        }
        activeTransform = nextTransform;

        seeker.StartPath(transform.position, activeTransform.position);
    }

    public void OnPathComplete(Path p)
    {
        Debug.Log("A path was calculated for the moving target. Did it fail with an error? " + p.error);

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
}

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrafficSystem : MonoBehaviour
{
    [Header("Path Settings")]
    public float waypointSpacing = 15f;
    public float laneOffset = 3f;
    
    [Header("Car Settings")]
    public int numberOfCars = 25;
    public float carSpeed = 10f;
    public float minCarDistance = 5f;
    
    [Header("Visual")]
    public bool showWaypoints = true;
    public Color waypointColor = Color.yellow;

    [Header("Exclusion Zone (Monas)")]
    public float centerExclusionRadius = 50f;  // Skip waypoints near center (Monas area)
    public bool excludeCenter = true;

    private List<Path> paths = new List<Path>();
    private Transform carsParent;
    private Transform waypointsParent;

    [System.Serializable]
    public class Path
    {
        public string name;
        public List<Vector3> waypoints = new List<Vector3>();
        public bool isLoop = false;
    }

    [ContextMenu("Generate Traffic System")]
    public void GenerateTrafficSystem()
    {
        ClearTraffic();
        
        waypointsParent = new GameObject("_TrafficWaypoints").transform;
        waypointsParent.SetParent(transform);
        
        carsParent = new GameObject("_TrafficCars").transform;
        carsParent.SetParent(transform);
        
        // Generate paths along roads
        GenerateRoadPaths();
        
        // Spawn cars on paths
        SpawnCars();
        
        Debug.Log($"Traffic System: Created {paths.Count} paths with {numberOfCars} cars");
    }

    void GenerateRoadPaths()
    {
        paths.Clear();
        
        CityLayoutManager cityManager = FindFirstObjectByType<CityLayoutManager>();
        if (cityManager == null)
        {
            Debug.LogError("CityLayoutManager not found!");
            return;
        }
        
        float blockSize = cityManager.blockSize;
        float roadWidth = cityManager.roadWidth;
        int gridX = cityManager.gridSizeX;
        int gridZ = cityManager.gridSizeZ;
        
        float totalX = gridX * (blockSize + roadWidth) + roadWidth;
        float totalZ = gridZ * (blockSize + roadWidth) + roadWidth;
        float offsetX = -totalX / 2f;
        float offsetZ = -totalZ / 2f;
        
        float roadY = 0.5f; // Slightly above road
        
        // Create horizontal road paths (both directions)
        for (int z = 0; z <= gridZ; z++)
        {
            float zPos = offsetZ + z * (blockSize + roadWidth) + roadWidth / 2f;
            
            // Eastbound lane (positive X direction)
            Path eastPath = new Path { name = $"Road_H{z}_East", isLoop = false };
            for (float x = offsetX + 5f; x < offsetX + totalX - 5f; x += waypointSpacing)
            {
                Vector3 wp = new Vector3(x, roadY, zPos - laneOffset);
                if (!IsInExclusionZone(wp))
                    eastPath.waypoints.Add(wp);
            }
            Vector3 eastEnd = new Vector3(offsetX + totalX - 5f, roadY, zPos - laneOffset);
            if (!IsInExclusionZone(eastEnd))
                eastPath.waypoints.Add(eastEnd);
            if (eastPath.waypoints.Count >= 2) paths.Add(eastPath);
            
            // Westbound lane (negative X direction)
            Path westPath = new Path { name = $"Road_H{z}_West", isLoop = false };
            for (float x = offsetX + totalX - 5f; x > offsetX + 5f; x -= waypointSpacing)
            {
                Vector3 wp = new Vector3(x, roadY, zPos + laneOffset);
                if (!IsInExclusionZone(wp))
                    westPath.waypoints.Add(wp);
            }
            Vector3 westEnd = new Vector3(offsetX + 5f, roadY, zPos + laneOffset);
            if (!IsInExclusionZone(westEnd))
                westPath.waypoints.Add(westEnd);
            if (westPath.waypoints.Count >= 2) paths.Add(westPath);
        }
        
        // Create vertical road paths (both directions)
        for (int x = 0; x <= gridX; x++)
        {
            float xPos = offsetX + x * (blockSize + roadWidth) + roadWidth / 2f;
            
            // Northbound lane (positive Z direction)
            Path northPath = new Path { name = $"Road_V{x}_North", isLoop = false };
            for (float z = offsetZ + 5f; z < offsetZ + totalZ - 5f; z += waypointSpacing)
            {
                Vector3 wp = new Vector3(xPos + laneOffset, roadY, z);
                if (!IsInExclusionZone(wp))
                    northPath.waypoints.Add(wp);
            }
            Vector3 northEnd = new Vector3(xPos + laneOffset, roadY, offsetZ + totalZ - 5f);
            if (!IsInExclusionZone(northEnd))
                northPath.waypoints.Add(northEnd);
            if (northPath.waypoints.Count >= 2) paths.Add(northPath);
            
            // Southbound lane (negative Z direction)
            Path southPath = new Path { name = $"Road_V{x}_South", isLoop = false };
            for (float z = offsetZ + totalZ - 5f; z > offsetZ + 5f; z -= waypointSpacing)
            {
                Vector3 wp = new Vector3(xPos - laneOffset, roadY, z);
                if (!IsInExclusionZone(wp))
                    southPath.waypoints.Add(wp);
            }
            Vector3 southEnd = new Vector3(xPos - laneOffset, roadY, offsetZ + 5f);
            if (!IsInExclusionZone(southEnd))
                southPath.waypoints.Add(southEnd);
            if (southPath.waypoints.Count >= 2) paths.Add(southPath);
        }
        
        // Create visual waypoint markers
        if (showWaypoints)
        {
            foreach (var path in paths)
            {
                Transform pathParent = new GameObject(path.name).transform;
                pathParent.SetParent(waypointsParent);
                
                for (int i = 0; i < path.waypoints.Count; i++)
                {
                    GameObject wp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    wp.name = $"WP_{i}";
                    wp.transform.SetParent(pathParent);
                    wp.transform.position = path.waypoints[i];
                    wp.transform.localScale = Vector3.one * 0.5f;
                    wp.GetComponent<Renderer>().material.color = waypointColor;
                    wp.GetComponent<Collider>().enabled = false; // No collision
                }
            }
        }
    }

    void SpawnCars()
    {
        if (paths.Count == 0) return;
        
        Color[] carColors = { 
            Color.red, Color.blue, Color.white, Color.black, 
            new Color(0.3f, 0.3f, 0.35f), new Color(0.8f, 0.5f, 0.1f), 
            new Color(0.1f, 0.4f, 0.7f), new Color(0.7f, 0.1f, 0.2f),
            new Color(0.9f, 0.9f, 0.9f), new Color(0.2f, 0.6f, 0.3f)
        };
        
        List<Vector3> usedPositions = new List<Vector3>();
        int carsCreated = 0;
        int attempts = 0;
        int maxAttempts = numberOfCars * 5;
        
        // Distribute cars evenly across paths
        int pathIndex = 0;
        
        while (carsCreated < numberOfCars && attempts < maxAttempts)
        {
            attempts++;
            
            // Cycle through paths for even distribution
            Path path = paths[pathIndex % paths.Count];
            pathIndex++;
            
            if (path.waypoints.Count < 2) continue;
            
            // Pick random starting waypoint
            int startIndex = Random.Range(0, path.waypoints.Count - 1);
            Vector3 startPos = path.waypoints[startIndex];
            
            // Check if position is clear
            bool positionClear = true;
            foreach (Vector3 usedPos in usedPositions)
            {
                if (Vector3.Distance(usedPos, startPos) < minCarDistance)
                {
                    positionClear = false;
                    break;
                }
            }
            
            if (!positionClear) continue;
            
            // Create car
            GameObject newCar = CreateCarModel(carsParent, startPos, $"TrafficCar_{carsCreated}", 
                                           carColors[Random.Range(0, carColors.Length)]);
            
            // Add path follower
            PathFollowingCar follower = newCar.AddComponent<PathFollowingCar>();
            follower.waypoints = new List<Vector3>(path.waypoints);
            follower.currentWaypointIndex = startIndex;
            follower.speed = carSpeed + Random.Range(-2f, 3f);
            follower.allPaths = paths;
            
            // Face the direction of travel
            if (startIndex + 1 < path.waypoints.Count)
            {
                Vector3 dir = path.waypoints[startIndex + 1] - startPos;
                dir.y = 0;
                if (dir.magnitude > 0.1f)
                    newCar.transform.rotation = Quaternion.LookRotation(dir);
            }
            
            usedPositions.Add(startPos);
            carsCreated++;
        }
        
        Debug.Log($"TrafficSystem: Created {carsCreated} cars on {paths.Count} paths");
    }

    GameObject CreateCarModel(Transform parent, Vector3 position, string name, Color color)
    {
        GameObject car = new GameObject(name);
        car.transform.SetParent(parent);
        car.transform.position = position;
        
        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(car.transform);
        body.transform.localPosition = new Vector3(0, 0.4f, 0);
        body.transform.localScale = new Vector3(2f, 0.8f, 4.2f);
        body.GetComponent<Renderer>().material.color = color;
        
        // Add collider to car root for detection
        BoxCollider carCollider = car.AddComponent<BoxCollider>();
        carCollider.center = new Vector3(0, 0.5f, 0);
        carCollider.size = new Vector3(2.2f, 1.2f, 4.5f);
        carCollider.isTrigger = true;
        
        // Cabin
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(car.transform);
        cabin.transform.localPosition = new Vector3(0, 0.95f, 0.2f);
        cabin.transform.localScale = new Vector3(1.8f, 0.55f, 1.8f);
        cabin.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.15f);
        Destroy(cabin.GetComponent<Collider>());
        
        // Wheels
        Vector3[] wheelPos = {
            new Vector3(-0.85f, 0.05f, 1.2f), new Vector3(0.85f, 0.05f, 1.2f),
            new Vector3(-0.85f, 0.05f, -1.2f), new Vector3(0.85f, 0.05f, -1.2f)
        };
        
        foreach (var wp in wheelPos)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.SetParent(car.transform);
            wheel.transform.localPosition = wp;
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            wheel.transform.localScale = new Vector3(0.45f, 0.12f, 0.45f);
            wheel.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);
            Destroy(wheel.GetComponent<Collider>());
        }
        
        // Headlights
        for (int i = 0; i < 2; i++)
        {
            GameObject hl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hl.name = $"Headlight";
            hl.transform.SetParent(car.transform);
            hl.transform.localPosition = new Vector3(i == 0 ? -0.55f : 0.55f, 0.35f, 2.1f);
            hl.transform.localScale = new Vector3(0.25f, 0.2f, 0.08f);
            hl.GetComponent<Renderer>().material.color = new Color(1f, 1f, 0.8f);
            Destroy(hl.GetComponent<Collider>());
        }
        
        // Taillights
        for (int i = 0; i < 2; i++)
        {
            GameObject tl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tl.name = $"Taillight";
            tl.transform.SetParent(car.transform);
            tl.transform.localPosition = new Vector3(i == 0 ? -0.55f : 0.55f, 0.35f, -2.1f);
            tl.transform.localScale = new Vector3(0.2f, 0.15f, 0.08f);
            tl.GetComponent<Renderer>().material.color = new Color(0.8f, 0.1f, 0.1f);
            Destroy(tl.GetComponent<Collider>());
        }
        
        // Rigidbody for physics
        Rigidbody rb = car.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        return car;
    }

    bool IsInExclusionZone(Vector3 position)
    {
        if (!excludeCenter) return false;
        Vector3 center = new Vector3(0, position.y, 0);
        return Vector3.Distance(position, center) < centerExclusionRadius;
    }

    [ContextMenu("Clear Traffic")]
    public void ClearTraffic()
    {
        Transform existingWP = transform.Find("_TrafficWaypoints");
        if (existingWP != null) DestroyImmediate(existingWP.gameObject);
        
        Transform existingCars = transform.Find("_TrafficCars");
        if (existingCars != null) DestroyImmediate(existingCars.gameObject);
        
        paths.Clear();
    }
    
    [ContextMenu("Hide Waypoints")]
    public void HideWaypoints()
    {
        Transform wp = transform.Find("_TrafficWaypoints");
        if (wp != null)
        {
            foreach (Transform child in wp)
            {
                foreach (Transform sphere in child)
                {
                    Renderer r = sphere.GetComponent<Renderer>();
                    if (r != null) r.enabled = false;
                }
            }
        }
    }
}

// === PATH FOLLOWING CAR ===
public class PathFollowingCar : MonoBehaviour
{
    public List<Vector3> waypoints;
    public int currentWaypointIndex = 0;
    public float speed = 8f;
    public float rotationSpeed = 3f;
    public float waypointReachDistance = 2f;
    public float obstacleDetectionDistance = 6f;
    
    [HideInInspector]
    public List<TrafficSystem.Path> allPaths;
    
    private bool isStopped = false;
    private float stopTimer = 0f;

    void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        
        // Check for obstacles ahead
        CheckForObstacles();
        
        if (isStopped)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0) isStopped = false;
            return;
        }
        
        // Get current target waypoint
        Vector3 target = waypoints[currentWaypointIndex];
        target.y = transform.position.y; // Keep same height
        
        Vector3 direction = target - transform.position;
        direction.y = 0;
        
        float distance = direction.magnitude;
        
        // Check if reached waypoint
        if (distance < waypointReachDistance)
        {
            currentWaypointIndex++;
            
            // If reached end of path, pick a new path
            if (currentWaypointIndex >= waypoints.Count)
            {
                PickNewPath();
            }
            return;
        }
        
        // Rotate towards target
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        
        // Move forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        
        // Keep on ground
        Vector3 pos = transform.position;
        pos.y = 0.5f;
        transform.position = pos;
    }

    void CheckForObstacles()
    {
        // Raycast forward to check for other cars
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        
        if (Physics.Raycast(rayStart, transform.forward, out hit, obstacleDetectionDistance))
        {
            if (hit.collider.gameObject != gameObject && hit.collider.GetComponent<PathFollowingCar>() != null)
            {
                // Another car ahead, slow down or stop
                float distanceToOther = hit.distance;
                if (distanceToOther < 4f)
                {
                    isStopped = true;
                    stopTimer = 0.5f;
                }
            }
        }
    }

    void PickNewPath()
    {
        if (allPaths == null || allPaths.Count == 0)
        {
            // Just reverse on current path
            waypoints.Reverse();
            currentWaypointIndex = 0;
            return;
        }
        
        // Find connecting path or random new path
        Vector3 currentPos = transform.position;
        
        // Try to find a path that starts near current position
        List<TrafficSystem.Path> nearbyPaths = new List<TrafficSystem.Path>();
        foreach (var path in allPaths)
        {
            if (path.waypoints.Count > 0)
            {
                float distToStart = Vector3.Distance(currentPos, path.waypoints[0]);
                if (distToStart < 20f)
                {
                    nearbyPaths.Add(path);
                }
            }
        }
        
        if (nearbyPaths.Count > 0)
        {
            // Pick a random nearby path
            var newPath = nearbyPaths[Random.Range(0, nearbyPaths.Count)];
            waypoints = new List<Vector3>(newPath.waypoints);
            currentWaypointIndex = 0;
        }
        else
        {
            // Pick any random path
            var newPath = allPaths[Random.Range(0, allPaths.Count)];
            waypoints = new List<Vector3>(newPath.waypoints);
            currentWaypointIndex = 0;
            
            // Teleport to start of new path
            transform.position = waypoints[0];
        }
        
        // Face direction of travel
        if (waypoints.Count > 1)
        {
            Vector3 dir = waypoints[1] - waypoints[0];
            dir.y = 0;
            if (dir.magnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
        }
        
        if (currentWaypointIndex < waypoints.Count)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoints[currentWaypointIndex], 1f);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TrafficSystem))]
public class TrafficSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TrafficSystem traffic = (TrafficSystem)target;
        
        GUILayout.Space(15);
        
        EditorGUILayout.HelpBox(
            "Traffic System with Path Following\n\n" +
            "- Generates waypoints along roads\n" +
            "- Cars follow lane-based paths\n" +
            "- Cars detect and avoid other cars\n" +
            "- Cars pick new paths at road ends\n\n" +
            "NOTE: Regenerate after expanding city!", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        GUI.backgroundColor = new Color(0.2f, 0.7f, 1f);
        if (GUILayout.Button("GENERATE TRAFFIC", GUILayout.Height(45)))
            traffic.GenerateTrafficSystem();
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = new Color(0.9f, 0.7f, 0.3f);
        if (GUILayout.Button("Hide Waypoints", GUILayout.Height(25)))
            traffic.HideWaypoints();
        
        GUI.backgroundColor = new Color(1f, 0.4f, 0.3f);
        if (GUILayout.Button("Clear Traffic", GUILayout.Height(30)))
            traffic.ClearTraffic();
        
        GUI.backgroundColor = Color.white;
    }
}
#endif

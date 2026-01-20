using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnvironmentPopulator : MonoBehaviour
{
    [Header("Street Lamps")]
    public bool generateLamps = true;
    public float lampSpacing = 25f;
    public float lampHeight = 6f;
    public Color lampLightColor = new Color(1f, 0.95f, 0.8f);
    public float lampIntensity = 15f;
    
    [Header("Benches")]
    public bool generateBenches = true;
    public int benchesPerBlock = 2;
    
    [Header("Vehicles")]
    public bool generateVehicles = true;
    public int parkedCarsCount = 15;
    public int movingCarsCount = 10;
    public float carSpeed = 10f;
    
    [Header("NPCs")]
    public bool generateNPCs = true;
    public int npcCount = 20;
    public float npcSpeed = 1.5f;
    
    [Header("Extra Props")]
    public bool generateTrashCans = true;
    public bool generateTrees = true;
    public int treesPerBlock = 2;
    
    [Header("Materials (Optional)")]
    public Material metalMaterial;
    public Material woodMaterial;

    private Transform propsParent;
    private List<Vector3> sidewalkPositions = new List<Vector3>();
    private float cityBoundary = 100f;

    [ContextMenu("Populate Environment")]
    public void PopulateEnvironment()
    {
        ClearProps();
        
        propsParent = new GameObject("_EnvironmentProps").transform;
        propsParent.SetParent(transform);
        
        FindCityInfo();
        
        if (generateLamps) GenerateStreetLamps();
        if (generateBenches) GenerateBenches();
        if (generateVehicles) GenerateVehicles();
        if (generateNPCs) GenerateNPCs();
        if (generateTrashCans) GenerateTrashCans();
        if (generateTrees) GenerateStreetTrees();
        
        Debug.Log($"Environment populated! Cars: {parkedCarsCount} parked + {movingCarsCount} moving, NPCs: {npcCount}");
    }

    void FindCityInfo()
    {
        CityLayoutManager cityManager = FindFirstObjectByType<CityLayoutManager>();
        if (cityManager != null)
        {
            float blockSize = cityManager.blockSize;
            float roadWidth = cityManager.roadWidth;
            int gridX = cityManager.gridSizeX;
            int gridZ = cityManager.gridSizeZ;
            
            float totalX = gridX * (blockSize + roadWidth) + roadWidth;
            float totalZ = gridZ * (blockSize + roadWidth) + roadWidth;
            float offsetX = -totalX / 2f;
            float offsetZ = -totalZ / 2f;
            
            cityBoundary = Mathf.Max(totalX, totalZ) / 2f + 10f;
            
            for (int x = 0; x < gridX; x++)
            {
                for (int z = 0; z < gridZ; z++)
                {
                    float blockX = offsetX + roadWidth + x * (blockSize + roadWidth) + blockSize / 2f;
                    float blockZ = offsetZ + roadWidth + z * (blockSize + roadWidth) + blockSize / 2f;
                    sidewalkPositions.Add(new Vector3(blockX, 0, blockZ));
                }
            }
        }
        else
        {
            // Default positions if no CityLayoutManager
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    sidewalkPositions.Add(new Vector3(x * 50, 0, z * 50));
                }
            }
        }
    }

    void GenerateStreetLamps()
    {
        Transform lampParent = new GameObject("StreetLamps").transform;
        lampParent.SetParent(propsParent);
        
        CityLayoutManager cityManager = FindFirstObjectByType<CityLayoutManager>();
        if (cityManager == null) return;
        
        float blockSize = cityManager.blockSize;
        float roadWidth = cityManager.roadWidth;
        int gridX = cityManager.gridSizeX;
        int gridZ = cityManager.gridSizeZ;
        
        float totalX = gridX * (blockSize + roadWidth) + roadWidth;
        float totalZ = gridZ * (blockSize + roadWidth) + roadWidth;
        float offsetX = -totalX / 2f;
        float offsetZ = -totalZ / 2f;
        
        int lampIndex = 0;
        
        // Lamps along roads
        for (int z = 0; z <= gridZ; z++)
        {
            float zPos = offsetZ + z * (blockSize + roadWidth) + roadWidth / 2f;
            for (float x = offsetX + lampSpacing/2; x < offsetX + totalX; x += lampSpacing)
            {
                CreateStreetLamp(lampParent, new Vector3(x, 0, zPos - roadWidth/2 + 1.5f), lampIndex++);
                CreateStreetLamp(lampParent, new Vector3(x, 0, zPos + roadWidth/2 - 1.5f), lampIndex++);
            }
        }
        
        for (int x = 0; x <= gridX; x++)
        {
            float xPos = offsetX + x * (blockSize + roadWidth) + roadWidth / 2f;
            for (float z = offsetZ + lampSpacing/2; z < offsetZ + totalZ; z += lampSpacing)
            {
                CreateStreetLamp(lampParent, new Vector3(xPos - roadWidth/2 + 1.5f, 0, z), lampIndex++);
            }
        }
    }

    void CreateStreetLamp(Transform parent, Vector3 position, int index)
    {
        GameObject lamp = new GameObject($"StreetLamp_{index}");
        lamp.transform.SetParent(parent);
        lamp.transform.position = position;
        
        // Pole
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(lamp.transform);
        pole.transform.localPosition = new Vector3(0, lampHeight / 2, 0);
        pole.transform.localScale = new Vector3(0.15f, lampHeight / 2, 0.15f);
        pole.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.35f);
        
        // Arm
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "Arm";
        arm.transform.SetParent(lamp.transform);
        arm.transform.localPosition = new Vector3(0.8f, lampHeight - 0.3f, 0);
        arm.transform.localScale = new Vector3(1.6f, 0.1f, 0.1f);
        arm.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.35f);
        
        // Lamp head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "LampHead";
        head.transform.SetParent(lamp.transform);
        head.transform.localPosition = new Vector3(1.5f, lampHeight - 0.5f, 0);
        head.transform.localScale = new Vector3(0.6f, 0.3f, 0.4f);
        head.GetComponent<Renderer>().material.color = new Color(1f, 1f, 0.9f);
        head.GetComponent<Renderer>().material.SetColor("_EmissionColor", lampLightColor * 0.5f);
        
        // Point light
        GameObject lightObj = new GameObject("Light");
        lightObj.transform.SetParent(lamp.transform);
        lightObj.transform.localPosition = new Vector3(1.5f, lampHeight - 0.8f, 0);
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = lampLightColor;
        pointLight.intensity = lampIntensity;
        pointLight.range = 18f;
    }

    void GenerateBenches()
    {
        Transform benchParent = new GameObject("Benches").transform;
        benchParent.SetParent(propsParent);
        
        int benchIndex = 0;
        foreach (Vector3 blockCenter in sidewalkPositions)
        {
            for (int i = 0; i < benchesPerBlock; i++)
            {
                float angle = Random.Range(0, 4) * 90f;
                Vector3 offset = new Vector3(Random.Range(-15f, 15f), 0, Random.Range(-15f, 15f));
                CreateBench(benchParent, blockCenter + offset, angle, benchIndex++);
            }
        }
    }

    void CreateBench(Transform parent, Vector3 position, float rotation, int index)
    {
        GameObject bench = new GameObject($"Bench_{index}");
        bench.transform.SetParent(parent);
        bench.transform.position = position + Vector3.up * 0.3f;
        bench.transform.rotation = Quaternion.Euler(0, rotation, 0);
        
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = "Seat";
        seat.transform.SetParent(bench.transform);
        seat.transform.localPosition = Vector3.zero;
        seat.transform.localScale = new Vector3(2f, 0.1f, 0.5f);
        seat.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.15f);
        
        GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.name = "Back";
        back.transform.SetParent(bench.transform);
        back.transform.localPosition = new Vector3(0, 0.3f, -0.2f);
        back.transform.localScale = new Vector3(2f, 0.5f, 0.08f);
        back.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.15f);
        
        for (int i = 0; i < 2; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = $"Leg_{i}";
            leg.transform.SetParent(bench.transform);
            leg.transform.localPosition = new Vector3(i == 0 ? -0.8f : 0.8f, -0.2f, 0);
            leg.transform.localScale = new Vector3(0.1f, 0.5f, 0.4f);
            leg.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
        }
    }

    void GenerateVehicles()
    {
        Transform vehicleParent = new GameObject("Vehicles").transform;
        vehicleParent.SetParent(propsParent);
        
        CityLayoutManager cityManager = FindFirstObjectByType<CityLayoutManager>();
        if (cityManager == null) return;
        
        float blockSize = cityManager.blockSize;
        float roadWidth = cityManager.roadWidth;
        int gridX = cityManager.gridSizeX;
        int gridZ = cityManager.gridSizeZ;
        
        float totalX = gridX * (blockSize + roadWidth) + roadWidth;
        float totalZ = gridZ * (blockSize + roadWidth) + roadWidth;
        float offsetX = -totalX / 2f;
        float offsetZ = -totalZ / 2f;
        
        // PARKED CARS
        Transform parkedParent = new GameObject("ParkedCars").transform;
        parkedParent.SetParent(vehicleParent);
        
        for (int i = 0; i < parkedCarsCount; i++)
        {
            bool horizontal = Random.value > 0.5f;
            float x, z, rotation;
            
            if (horizontal)
            {
                int roadZ = Random.Range(0, gridZ + 1);
                z = offsetZ + roadZ * (blockSize + roadWidth) + roadWidth / 2f + (Random.value > 0.5f ? 5f : -5f);
                x = Random.Range(offsetX + 15f, offsetX + totalX - 15f);
                rotation = 0f;
            }
            else
            {
                int roadX = Random.Range(0, gridX + 1);
                x = offsetX + roadX * (blockSize + roadWidth) + roadWidth / 2f + (Random.value > 0.5f ? 5f : -5f);
                z = Random.Range(offsetZ + 15f, offsetZ + totalZ - 15f);
                rotation = 90f;
            }
            
            CreateCar(parkedParent, new Vector3(x, 0.4f, z), rotation, $"ParkedCar_{i}", false);
        }
        
        // MOVING CARS - Create separately with explicit mover
        Transform movingParent = new GameObject("MovingCars").transform;
        movingParent.SetParent(vehicleParent);
        
        for (int i = 0; i < movingCarsCount; i++)
        {
            bool horizontal = i % 2 == 0; // Alternate
            float x, z, rotation;
            
            if (horizontal)
            {
                int roadZ = i % (gridZ + 1);
                z = offsetZ + roadZ * (blockSize + roadWidth) + roadWidth / 2f + (i % 4 < 2 ? 2.5f : -2.5f);
                x = Random.Range(offsetX + 20f, offsetX + totalX - 20f);
                rotation = i % 4 < 2 ? 0f : 180f;
            }
            else
            {
                int roadX = i % (gridX + 1);
                x = offsetX + roadX * (blockSize + roadWidth) + roadWidth / 2f + (i % 4 < 2 ? 2.5f : -2.5f);
                z = Random.Range(offsetZ + 20f, offsetZ + totalZ - 20f);
                rotation = i % 4 < 2 ? 90f : 270f;
            }
            
            CreateCar(movingParent, new Vector3(x, 0.4f, z), rotation, $"MovingCar_{i}", true);
        }
        
        Debug.Log($"Created {movingCarsCount} moving cars");
    }

    void CreateCar(Transform parent, Vector3 position, float rotation, string name, bool isMoving)
    {
        Color[] carColors = { 
            Color.red, Color.blue, Color.white, Color.black, 
            new Color(0.3f, 0.3f, 0.3f), new Color(0.8f, 0.6f, 0.2f), 
            new Color(0.2f, 0.5f, 0.8f), new Color(0.6f, 0.1f, 0.1f)
        };
        
        GameObject car = new GameObject(name);
        car.transform.SetParent(parent);
        car.transform.position = position;
        car.transform.rotation = Quaternion.Euler(0, rotation, 0);
        
        Color carColor = carColors[Random.Range(0, carColors.Length)];
        
        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(car.transform);
        body.transform.localPosition = new Vector3(0, 0.4f, 0);
        body.transform.localScale = new Vector3(2f, 0.8f, 4.5f);
        body.GetComponent<Renderer>().material.color = carColor;
        
        // Cabin
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(car.transform);
        cabin.transform.localPosition = new Vector3(0, 0.95f, 0.3f);
        cabin.transform.localScale = new Vector3(1.8f, 0.6f, 2f);
        cabin.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.2f);
        
        // Wheels
        Vector3[] wheelPos = {
            new Vector3(-0.9f, 0, 1.3f), new Vector3(0.9f, 0, 1.3f),
            new Vector3(-0.9f, 0, -1.3f), new Vector3(0.9f, 0, -1.3f)
        };
        
        foreach (var wp in wheelPos)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.SetParent(car.transform);
            wheel.transform.localPosition = wp;
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            wheel.transform.localScale = new Vector3(0.5f, 0.15f, 0.5f);
            wheel.GetComponent<Renderer>().material.color = Color.black;
        }
        
        // Headlights
        for (int i = 0; i < 2; i++)
        {
            GameObject hl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hl.name = $"Headlight_{i}";
            hl.transform.SetParent(car.transform);
            hl.transform.localPosition = new Vector3(i == 0 ? -0.6f : 0.6f, 0.4f, 2.3f);
            hl.transform.localScale = new Vector3(0.3f, 0.25f, 0.1f);
            hl.GetComponent<Renderer>().material.color = Color.yellow;
        }
        
        // Taillights
        for (int i = 0; i < 2; i++)
        {
            GameObject tl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tl.name = $"Taillight_{i}";
            tl.transform.SetParent(car.transform);
            tl.transform.localPosition = new Vector3(i == 0 ? -0.6f : 0.6f, 0.4f, -2.3f);
            tl.transform.localScale = new Vector3(0.25f, 0.2f, 0.1f);
            tl.GetComponent<Renderer>().material.color = Color.red;
        }
        
        // ADD MOVER FOR MOVING CARS
        if (isMoving)
        {
            CarMover mover = car.AddComponent<CarMover>();
            mover.speed = carSpeed + Random.Range(-2f, 3f);
            mover.boundary = cityBoundary;
            Debug.Log($"Added CarMover to {name} with speed {mover.speed}");
        }
    }

    void GenerateNPCs()
    {
        Transform npcParent = new GameObject("NPCs").transform;
        npcParent.SetParent(propsParent);
        
        Color[] shirtColors = { 
            Color.red, Color.blue, Color.green, new Color(1f, 0.5f, 0f),
            Color.white, new Color(0.6f, 0.2f, 0.6f), Color.cyan, Color.yellow
        };
        Color[] pantsColors = { 
            Color.black, new Color(0.2f, 0.2f, 0.35f), 
            new Color(0.35f, 0.25f, 0.15f), new Color(0.3f, 0.3f, 0.3f)
        };
        Color[] skinColors = {
            new Color(0.95f, 0.8f, 0.7f),
            new Color(0.85f, 0.7f, 0.55f),
            new Color(0.6f, 0.45f, 0.35f),
            new Color(0.45f, 0.3f, 0.2f)
        };
        
        for (int i = 0; i < npcCount; i++)
        {
            Vector3 pos;
            if (sidewalkPositions.Count > 0)
            {
                Vector3 blockCenter = sidewalkPositions[Random.Range(0, sidewalkPositions.Count)];
                pos = blockCenter + new Vector3(Random.Range(-16f, 16f), 0, Random.Range(-16f, 16f));
            }
            else
            {
                pos = new Vector3(Random.Range(-60f, 60f), 0, Random.Range(-60f, 60f));
            }
            
            CreateNPC(npcParent, pos, i, 
                shirtColors[Random.Range(0, shirtColors.Length)],
                pantsColors[Random.Range(0, pantsColors.Length)],
                skinColors[Random.Range(0, skinColors.Length)]);
        }
        
        Debug.Log($"Created {npcCount} NPCs");
    }

    void CreateNPC(Transform parent, Vector3 position, int index, Color shirtColor, Color pantsColor, Color skinColor)
    {
        GameObject npc = new GameObject($"NPC_{index}");
        npc.transform.SetParent(parent);
        npc.transform.position = position;
        npc.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        
        float scale = Random.Range(0.9f, 1.1f);
        
        // LEGS (2 separate cylinders)
        GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLeg.name = "LeftLeg";
        leftLeg.transform.SetParent(npc.transform);
        leftLeg.transform.localPosition = new Vector3(-0.12f * scale, 0.4f * scale, 0);
        leftLeg.transform.localScale = new Vector3(0.18f * scale, 0.4f * scale, 0.18f * scale);
        leftLeg.GetComponent<Renderer>().material.color = pantsColor;
        
        GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLeg.name = "RightLeg";
        rightLeg.transform.SetParent(npc.transform);
        rightLeg.transform.localPosition = new Vector3(0.12f * scale, 0.4f * scale, 0);
        rightLeg.transform.localScale = new Vector3(0.18f * scale, 0.4f * scale, 0.18f * scale);
        rightLeg.GetComponent<Renderer>().material.color = pantsColor;
        
        // TORSO
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        torso.name = "Torso";
        torso.transform.SetParent(npc.transform);
        torso.transform.localPosition = new Vector3(0, 1.1f * scale, 0);
        torso.transform.localScale = new Vector3(0.35f * scale, 0.45f * scale, 0.22f * scale);
        torso.GetComponent<Renderer>().material.color = shirtColor;
        
        // ARMS
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArm.name = "LeftArm";
        leftArm.transform.SetParent(npc.transform);
        leftArm.transform.localPosition = new Vector3(-0.28f * scale, 1.0f * scale, 0);
        leftArm.transform.localScale = new Vector3(0.1f * scale, 0.35f * scale, 0.1f * scale);
        leftArm.GetComponent<Renderer>().material.color = shirtColor;
        
        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArm.name = "RightArm";
        rightArm.transform.SetParent(npc.transform);
        rightArm.transform.localPosition = new Vector3(0.28f * scale, 1.0f * scale, 0);
        rightArm.transform.localScale = new Vector3(0.1f * scale, 0.35f * scale, 0.1f * scale);
        rightArm.GetComponent<Renderer>().material.color = shirtColor;
        
        // HEAD
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(npc.transform);
        head.transform.localPosition = new Vector3(0, 1.65f * scale, 0);
        head.transform.localScale = new Vector3(0.28f * scale, 0.3f * scale, 0.26f * scale);
        head.GetComponent<Renderer>().material.color = skinColor;
        
        // HAIR (optional cap/hair)
        if (Random.value > 0.3f)
        {
            GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Hair";
            hair.transform.SetParent(npc.transform);
            hair.transform.localPosition = new Vector3(0, 1.75f * scale, -0.02f);
            hair.transform.localScale = new Vector3(0.26f * scale, 0.15f * scale, 0.24f * scale);
            hair.GetComponent<Renderer>().material.color = Random.value > 0.5f ? 
                Color.black : new Color(0.4f, 0.25f, 0.1f);
        }
        
        // ADD WALKER COMPONENT
        NPCWalker walker = npc.AddComponent<NPCWalker>();
        walker.walkSpeed = npcSpeed + Random.Range(-0.3f, 0.5f);
        walker.walkRadius = Random.Range(8f, 20f);
    }

    void GenerateTrashCans()
    {
        Transform trashParent = new GameObject("TrashCans").transform;
        trashParent.SetParent(propsParent);
        
        int index = 0;
        foreach (Vector3 blockCenter in sidewalkPositions)
        {
            Vector3[] corners = {
                blockCenter + new Vector3(16f, 0, 16f),
                blockCenter + new Vector3(-16f, 0, -16f)
            };
            
            foreach (var corner in corners)
            {
                CreateTrashCan(trashParent, corner, index++);
            }
        }
    }

    void CreateTrashCan(Transform parent, Vector3 position, int index)
    {
        GameObject trashCan = new GameObject($"TrashCan_{index}");
        trashCan.transform.SetParent(parent);
        trashCan.transform.position = position;
        
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(trashCan.transform);
        body.transform.localPosition = new Vector3(0, 0.5f, 0);
        body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        body.GetComponent<Renderer>().material.color = new Color(0.15f, 0.25f, 0.15f);
        
        GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lid.name = "Lid";
        lid.transform.SetParent(trashCan.transform);
        lid.transform.localPosition = new Vector3(0, 1.05f, 0);
        lid.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f);
        lid.GetComponent<Renderer>().material.color = new Color(0.1f, 0.2f, 0.1f);
    }

    void GenerateStreetTrees()
    {
        Transform treeParent = new GameObject("StreetTrees").transform;
        treeParent.SetParent(propsParent);
        
        int index = 0;
        foreach (Vector3 blockCenter in sidewalkPositions)
        {
            for (int i = 0; i < treesPerBlock; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-14f, 14f), 0, Random.Range(-14f, 14f));
                CreateTree(treeParent, blockCenter + offset, index++);
            }
        }
    }

    void CreateTree(Transform parent, Vector3 position, int index)
    {
        GameObject tree = new GameObject($"Tree_{index}");
        tree.transform.SetParent(parent);
        tree.transform.position = position;
        
        float height = Random.Range(5f, 8f);
        
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, height * 0.25f, 0);
        trunk.transform.localScale = new Vector3(0.4f, height * 0.25f, 0.4f);
        trunk.GetComponent<Renderer>().material.color = new Color(0.4f, 0.28f, 0.15f);
        
        GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foliage.name = "Foliage";
        foliage.transform.SetParent(tree.transform);
        foliage.transform.localPosition = new Vector3(0, height * 0.65f, 0);
        foliage.transform.localScale = new Vector3(height * 0.5f, height * 0.45f, height * 0.5f);
        foliage.GetComponent<Renderer>().material.color = new Color(0.15f, 0.45f, 0.15f);
    }

    [ContextMenu("Clear Props")]
    public void ClearProps()
    {
        Transform existing = transform.Find("_EnvironmentProps");
        if (existing != null) DestroyImmediate(existing.gameObject);
        sidewalkPositions.Clear();
    }
}

// === CAR MOVER - Moves forward, turns at boundary ===
public class CarMover : MonoBehaviour
{
    public float speed = 10f;
    public float boundary = 100f;
    
    private bool initialized = false;
    
    void Start()
    {
        initialized = true;
    }
    
    void Update()
    {
        if (!initialized) return;
        
        // Move forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        
        // Check boundaries and turn around
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) > boundary || Mathf.Abs(pos.z) > boundary)
        {
            transform.Rotate(0, 180, 0);
            // Push back inside boundary
            pos.x = Mathf.Clamp(pos.x, -boundary + 5, boundary - 5);
            pos.z = Mathf.Clamp(pos.z, -boundary + 5, boundary - 5);
            transform.position = pos;
        }
    }
}

// === NPC WALKER - Wanders around ===
public class NPCWalker : MonoBehaviour
{
    public float walkSpeed = 1.5f;
    public float walkRadius = 15f;
    
    private Vector3 startPos;
    private Vector3 targetPos;
    private float waitTimer;
    private bool initialized = false;
    private float animTimer = 0f;
    
    void Start()
    {
        startPos = transform.position;
        PickNewTarget();
        initialized = true;
    }
    
    void Update()
    {
        if (!initialized) return;
        
        // Simple walk animation (bob up and down)
        animTimer += Time.deltaTime * walkSpeed * 4f;
        
        // Wait before moving to next target
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }
        
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0;
        
        if (toTarget.magnitude < 0.5f)
        {
            waitTimer = Random.Range(1f, 3f);
            PickNewTarget();
            return;
        }
        
        // Rotate towards target
        Quaternion targetRot = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 4f);
        
        // Move forward
        transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime, Space.Self);
        
        // Bob animation
        Vector3 pos = transform.position;
        pos.y = Mathf.Sin(animTimer) * 0.05f;
        transform.position = pos;
    }
    
    void PickNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * walkRadius;
        targetPos = startPos + new Vector3(randomCircle.x, 0, randomCircle.y);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EnvironmentPopulator))]
public class EnvironmentPopulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EnvironmentPopulator pop = (EnvironmentPopulator)target;
        
        GUILayout.Space(15);
        
        EditorGUILayout.HelpBox(
            "Environment Props:\n" +
            "- Street Lamps (with lights)\n" +
            "- Benches\n" +
            "- Parked & Moving Cars\n" +
            "- Walking NPCs\n" +
            "- Trash Cans & Trees", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
        if (GUILayout.Button("POPULATE ENVIRONMENT", GUILayout.Height(45)))
            pop.PopulateEnvironment();
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = new Color(1f, 0.4f, 0.3f);
        if (GUILayout.Button("Clear All Props", GUILayout.Height(30)))
            pop.ClearProps();
        
        GUI.backgroundColor = Color.white;
    }
}
#endif

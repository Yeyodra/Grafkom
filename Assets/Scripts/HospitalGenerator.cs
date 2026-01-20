using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HospitalGenerator : MonoBehaviour
{
    [Header("Hospital Settings")]
    public int hospitalGridX = 4;  // Which grid X position (0-based)
    public int hospitalGridZ = 2;  // Which grid Z position (0-based)
    public bool expandCityIfNeeded = true;
    public int minCitySize = 5;
    
    [Header("Building Settings")]
    public float mainBuildingHeight = 22f;
    public int floors = 5;
    
    [Header("Colors")]
    public Color hospitalWhite = new Color(0.95f, 0.95f, 0.97f);
    public Color hospitalRed = new Color(0.85f, 0.15f, 0.15f);
    public Color hospitalGray = new Color(0.6f, 0.6f, 0.65f);
    public Color glassBlue = new Color(0.5f, 0.7f, 0.9f);
    public Color roadColor = new Color(0.2f, 0.2f, 0.2f);

    private Transform hospitalParent;
    private float blockSize;
    private float roadWidth;
    private Vector3 blockCenter;

    [ContextMenu("Generate Hospital")]
    public void GenerateHospital()
    {
        ClearHospital();
        
        CityLayoutManager cityManager = FindFirstObjectByType<CityLayoutManager>();
        if (cityManager == null)
        {
            Debug.LogError("CityLayoutManager not found! Generate city first.");
            return;
        }
        
        // Expand city if needed
        if (expandCityIfNeeded)
        {
            bool needsRegenerate = false;
            if (cityManager.gridSizeX < minCitySize)
            {
                cityManager.gridSizeX = minCitySize;
                needsRegenerate = true;
            }
            if (cityManager.gridSizeZ < minCitySize)
            {
                cityManager.gridSizeZ = minCitySize;
                needsRegenerate = true;
            }
            if (hospitalGridX >= cityManager.gridSizeX)
            {
                cityManager.gridSizeX = hospitalGridX + 1;
                needsRegenerate = true;
            }
            if (hospitalGridZ >= cityManager.gridSizeZ)
            {
                cityManager.gridSizeZ = hospitalGridZ + 1;
                needsRegenerate = true;
            }
            
            if (needsRegenerate)
            {
                cityManager.GenerateCityLayout();
                Debug.Log($"City expanded to {cityManager.gridSizeX}x{cityManager.gridSizeZ}");
            }
        }
        
        // Get city parameters
        blockSize = cityManager.blockSize;
        roadWidth = cityManager.roadWidth;
        
        // Calculate block center position
        float totalX = cityManager.gridSizeX * (blockSize + roadWidth) + roadWidth;
        float totalZ = cityManager.gridSizeZ * (blockSize + roadWidth) + roadWidth;
        float offsetX = -totalX / 2f;
        float offsetZ = -totalZ / 2f;
        
        float blockX = offsetX + roadWidth + hospitalGridX * (blockSize + roadWidth) + blockSize / 2f;
        float blockZ = offsetZ + roadWidth + hospitalGridZ * (blockSize + roadWidth) + blockSize / 2f;
        blockCenter = new Vector3(blockX, 0, blockZ);
        
        // Remove existing block at this position
        RemoveExistingBlock(cityManager, hospitalGridX, hospitalGridZ);
        
        // Create hospital at block position
        hospitalParent = new GameObject("_Hospital").transform;
        hospitalParent.position = blockCenter;
        
        // Hospital fills the block
        float usableSize = blockSize - 6f;
        
        CreateMainBuilding(usableSize);
        CreateEmergencyWing(usableSize);
        CreateHelipad();
        CreateParkingArea(usableSize);
        CreateAmbulanceBay(usableSize);
        CreateRedCrossSign(usableSize);
        
        Debug.Log($"Hospital generated at block ({hospitalGridX}, {hospitalGridZ})");
    }
    
    void RemoveExistingBlock(CityLayoutManager cityManager, int x, int z)
    {
        Transform cityBlocks = cityManager.transform.Find("_CityBlocks");
        if (cityBlocks == null) return;
        
        string[] blockTypes = { "Commercial", "Residential", "Office", "Mixed" };
        foreach (string type in blockTypes)
        {
            string blockName = $"Block_{x}_{z}_{type}";
            Transform block = cityBlocks.Find(blockName);
            if (block != null)
            {
                DestroyImmediate(block.gameObject);
                Debug.Log($"Removed {blockName} for hospital");
                return;
            }
        }
    }

    void CreateMainBuilding(float blockUsableSize)
    {
        Transform main = new GameObject("MainBuilding").transform;
        main.SetParent(hospitalParent);
        main.localPosition = new Vector3(0, 0, -blockUsableSize * 0.1f);
        
        float buildingWidth = blockUsableSize * 0.6f;
        float buildingDepth = blockUsableSize * 0.5f;
        
        CreateBox(main, new Vector3(0, 0.25f, 0), 
            new Vector3(buildingWidth + 2, 0.5f, buildingDepth + 2), "Foundation", hospitalGray);
        
        float floorHeight = mainBuildingHeight / floors;
        for (int f = 0; f < floors; f++)
        {
            float y = 0.5f + f * floorHeight + floorHeight / 2f;
            
            CreateBox(main, new Vector3(0, y - floorHeight/2 + 0.1f, 0),
                new Vector3(buildingWidth, 0.2f, buildingDepth), $"Floor_{f}", hospitalGray);
            
            CreateBox(main, new Vector3(0, y, 0),
                new Vector3(buildingWidth, floorHeight - 0.2f, buildingDepth), $"Walls_{f}", hospitalWhite);
            
            int numWindows = Mathf.FloorToInt(buildingWidth / 5f);
            for (int w = 0; w < numWindows; w++)
            {
                float wx = -buildingWidth/2 + 3 + w * (buildingWidth - 6) / Mathf.Max(1, numWindows - 1);
                CreateBox(main, new Vector3(wx, y, buildingDepth/2 + 0.1f),
                    new Vector3(2.5f, floorHeight * 0.5f, 0.2f), $"Window_F{f}_W{w}", glassBlue);
            }
        }
        
        CreateBox(main, new Vector3(0, 0.5f + mainBuildingHeight + 0.3f, 0),
            new Vector3(buildingWidth + 1, 0.6f, buildingDepth + 1), "Roof", hospitalGray);
        
        CreateBox(main, new Vector3(0, 2, buildingDepth/2 + 2), new Vector3(8, 4, 4), "Entrance", glassBlue);
        CreateBox(main, new Vector3(0, 4.5f, buildingDepth/2 + 2), new Vector3(10, 0.5f, 5), "EntranceRoof", hospitalWhite);
    }

    void CreateEmergencyWing(float blockUsableSize)
    {
        Transform emergency = new GameObject("EmergencyWing").transform;
        emergency.SetParent(hospitalParent);
        emergency.localPosition = new Vector3(-blockUsableSize * 0.35f, 0, blockUsableSize * 0.2f);
        
        float wingWidth = blockUsableSize * 0.25f;
        float wingDepth = blockUsableSize * 0.3f;
        float wingHeight = 8f;
        
        CreateBox(emergency, new Vector3(0, wingHeight/2, 0), new Vector3(wingWidth, wingHeight, wingDepth), "EmergencyMain", hospitalWhite);
        CreateBox(emergency, new Vector3(0, wingHeight - 1, wingDepth/2 + 0.1f), new Vector3(wingWidth - 2, 1.5f, 0.2f), "RedStripe", hospitalRed);
        CreateBox(emergency, new Vector3(0, 2.5f, wingDepth/2 + 1), new Vector3(6, 5, 2), "EmergencyDoors", glassBlue);
        CreateBox(emergency, new Vector3(0, 5.5f, wingDepth/2 + 3), new Vector3(12, 0.4f, 6), "EmergencyCanopy", hospitalRed);
        CreateBox(emergency, new Vector3(-5, 2.75f, wingDepth/2 + 5), new Vector3(0.4f, 5.5f, 0.4f), "Support1", hospitalGray);
        CreateBox(emergency, new Vector3(5, 2.75f, wingDepth/2 + 5), new Vector3(0.4f, 5.5f, 0.4f), "Support2", hospitalGray);
    }

    void CreateHelipad()
    {
        Transform helipad = new GameObject("Helipad").transform;
        helipad.SetParent(hospitalParent);
        helipad.localPosition = new Vector3(0, mainBuildingHeight + 0.8f, -blockSize * 0.1f);
        
        CreateBox(helipad, Vector3.zero, new Vector3(12, 0.15f, 12), "Platform", hospitalGray);
        CreateBox(helipad, new Vector3(-2.5f, 0.1f, 0), new Vector3(1.2f, 0.08f, 8), "H_Left", hospitalWhite);
        CreateBox(helipad, new Vector3(2.5f, 0.1f, 0), new Vector3(1.2f, 0.08f, 8), "H_Right", hospitalWhite);
        CreateBox(helipad, new Vector3(0, 0.1f, 0), new Vector3(6f, 0.08f, 1.2f), "H_Middle", hospitalWhite);
        
        CreateBox(helipad, new Vector3(-5, 0.25f, -5), new Vector3(0.4f, 0.35f, 0.4f), "Light1", hospitalRed);
        CreateBox(helipad, new Vector3(5, 0.25f, -5), new Vector3(0.4f, 0.35f, 0.4f), "Light2", hospitalRed);
        CreateBox(helipad, new Vector3(-5, 0.25f, 5), new Vector3(0.4f, 0.35f, 0.4f), "Light3", hospitalRed);
        CreateBox(helipad, new Vector3(5, 0.25f, 5), new Vector3(0.4f, 0.35f, 0.4f), "Light4", hospitalRed);
    }

    void CreateParkingArea(float blockUsableSize)
    {
        Transform parking = new GameObject("ParkingArea").transform;
        parking.SetParent(hospitalParent);
        parking.localPosition = new Vector3(blockUsableSize * 0.35f, 0, 0);
        
        float parkingWidth = blockUsableSize * 0.25f;
        float parkingDepth = blockUsableSize * 0.6f;
        
        CreateBox(parking, new Vector3(0, 0.08f, 0), new Vector3(parkingWidth, 0.08f, parkingDepth), "ParkingSurface", roadColor);
        
        int rows = 3;
        int cols = Mathf.Max(1, Mathf.FloorToInt(parkingWidth / 4f));
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                float x = -parkingWidth/2 + 3 + col * 4f;
                float z = -parkingDepth/2 + 5 + row * (parkingDepth - 10) / Mathf.Max(1, rows - 1);
                CreateBox(parking, new Vector3(x, 0.13f, z), new Vector3(0.12f, 0.02f, 4), $"ParkLine_{row}_{col}", Color.white);
            }
        }
        
        CreateBox(parking, new Vector3(-parkingWidth/2 + 3, 0.13f, parkingDepth/2 - 3), 
            new Vector3(3.5f, 0.02f, 4), "HandicapSpot", new Color(0.2f, 0.4f, 0.8f));
    }

    void CreateAmbulanceBay(float blockUsableSize)
    {
        Transform ambulance = new GameObject("AmbulanceBay").transform;
        ambulance.SetParent(hospitalParent);
        ambulance.localPosition = new Vector3(-blockUsableSize * 0.35f, 0, blockUsableSize * 0.4f);
        
        CreateBox(ambulance, new Vector3(0, 3.5f, 0), new Vector3(14, 0.4f, 8), "BayCanopy", hospitalWhite);
        CreateBox(ambulance, new Vector3(-6, 1.75f, -3), new Vector3(0.4f, 3.5f, 0.4f), "Pillar1", hospitalGray);
        CreateBox(ambulance, new Vector3(6, 1.75f, -3), new Vector3(0.4f, 3.5f, 0.4f), "Pillar2", hospitalGray);
        CreateBox(ambulance, new Vector3(-6, 1.75f, 3), new Vector3(0.4f, 3.5f, 0.4f), "Pillar3", hospitalGray);
        CreateBox(ambulance, new Vector3(6, 1.75f, 3), new Vector3(0.4f, 3.5f, 0.4f), "Pillar4", hospitalGray);
        CreateBox(ambulance, new Vector3(0, 0.12f, 0), new Vector3(14, 0.2f, 8), "BayFloor", hospitalGray);
        
        for (int i = 0; i < 3; i++)
        {
            float x = -4 + i * 4;
            CreateBox(ambulance, new Vector3(x, 0.24f, 0), new Vector3(0.1f, 0.02f, 6), $"BayLine_{i}", Color.white);
        }
    }

    void CreateRedCrossSign(float blockUsableSize)
    {
        Transform sign = new GameObject("RedCrossSign").transform;
        sign.SetParent(hospitalParent);
        
        float buildingDepth = blockUsableSize * 0.5f;
        sign.localPosition = new Vector3(0, mainBuildingHeight * 0.6f, buildingDepth/2 - blockUsableSize * 0.1f + 0.5f);
        
        CreateBox(sign, Vector3.zero, new Vector3(2f, 6, 0.4f), "CrossVertical", hospitalRed);
        CreateBox(sign, Vector3.zero, new Vector3(6, 2f, 0.4f), "CrossHorizontal", hospitalRed);
        CreateBox(sign, new Vector3(0, 0, -0.25f), new Vector3(8, 8, 0.15f), "CrossBackground", hospitalWhite);
    }

    void CreateBox(Transform parent, Vector3 localPos, Vector3 scale, string name, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = scale;
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        obj.GetComponent<Renderer>().material = mat;
    }

    [ContextMenu("Clear Hospital")]
    public void ClearHospital()
    {
        GameObject existing = GameObject.Find("_Hospital");
        if (existing != null) DestroyImmediate(existing);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HospitalGenerator))]
public class HospitalGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        HospitalGenerator generator = (HospitalGenerator)target;
        
        GUILayout.Space(15);
        
        EditorGUILayout.HelpBox(
            "Hospital Generator - Integrated\n\n" +
            "Places hospital in a city block:\n" +
            "- Replaces existing block at (X, Z)\n" +
            "- Auto-expands city if needed\n" +
            "- Uses city roads (no separate ground)\n\n" +
            "Generate city first!", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("GENERATE HOSPITAL", GUILayout.Height(45)))
        {
            generator.GenerateHospital();
        }
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
        if (GUILayout.Button("Clear Hospital", GUILayout.Height(25)))
        {
            generator.ClearHospital();
        }
        
        GUI.backgroundColor = Color.white;
    }
}
#endif

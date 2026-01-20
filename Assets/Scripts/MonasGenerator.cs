using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MonasGenerator : MonoBehaviour
{
    [Header("Monas Settings")]
    public float monumentHeight = 80f;
    public float baseSize = 30f;
    public int minCityGridSize = 7;  // Minimum city size untuk Monas
    
    [Header("Colors")]
    public Color marbleWhite = new Color(0.95f, 0.93f, 0.9f);
    public Color marbleGray = new Color(0.75f, 0.73f, 0.7f);
    public Color goldColor = new Color(1f, 0.85f, 0.2f);
    public Color bronzeColor = new Color(0.6f, 0.45f, 0.2f);
    public Color groundColor = new Color(0.4f, 0.55f, 0.35f);

    private Transform monasParent;

    [ContextMenu("Generate Monas")]
    public void GenerateMonas()
    {
        ClearMonas();
        
        // Expand city if needed
        ExpandCityForMonas();
        
        // Clear center blocks for Monas plaza
        ClearCenterBlocks();
        
        // Position at city center
        Vector3 centerPos = Vector3.zero;
        
        monasParent = new GameObject("_Monas").transform;
        monasParent.position = centerPos;
        
        CreateBasePlatform();
        CreateMainPlatform();
        CreateObelisk();
        CreateGoldenFlame();
        CreateSurroundingPark();
        CreateStairs();
        
        // Regenerate traffic
        RegenerateTraffic();
        
        Debug.Log("Monas (Monumen Nasional) generated!");
    }
    
    void ExpandCityForMonas()
    {
        CityLayoutManager cityManager = FindFirstObjectByType<CityLayoutManager>();
        if (cityManager == null)
        {
            Debug.LogWarning("CityLayoutManager not found!");
            return;
        }
        
        bool needsRegenerate = false;
        
        if (cityManager.gridSizeX < minCityGridSize)
        {
            cityManager.gridSizeX = minCityGridSize;
            needsRegenerate = true;
        }
        if (cityManager.gridSizeZ < minCityGridSize)
        {
            cityManager.gridSizeZ = minCityGridSize;
            needsRegenerate = true;
        }
        
        if (needsRegenerate)
        {
            cityManager.GenerateCityLayout();
            Debug.Log($"City expanded to {cityManager.gridSizeX}x{cityManager.gridSizeZ} for Monas");
        }
    }
    
    void ClearCenterBlocks()
    {
        CityLayoutManager cityManager = FindFirstObjectByType<CityLayoutManager>();
        if (cityManager == null) return;
        
        Transform cityBlocks = cityManager.transform.Find("_CityBlocks");
        if (cityBlocks == null) return;
        
        // Calculate center blocks (2x2 or 3x3 area in center)
        int centerX = cityManager.gridSizeX / 2;
        int centerZ = cityManager.gridSizeZ / 2;
        
        string[] blockTypes = { "Commercial", "Residential", "Office", "Mixed" };
        
        // Clear a 3x3 area around center
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int x = centerX + dx;
                int z = centerZ + dz;
                
                foreach (string type in blockTypes)
                {
                    string blockName = $"Block_{x}_{z}_{type}";
                    Transform block = cityBlocks.Find(blockName);
                    if (block != null)
                    {
                        DestroyImmediate(block.gameObject);
                    }
                }
            }
        }
        
        Debug.Log("Cleared center blocks for Monas plaza");
    }
    
    void RegenerateTraffic()
    {
        TrafficSystem trafficSystem = FindFirstObjectByType<TrafficSystem>();
        if (trafficSystem != null)
        {
            trafficSystem.GenerateTrafficSystem();
            Debug.Log("Traffic system regenerated");
        }
    }

    void CreateBasePlatform()
    {
        Transform basePlatform = new GameObject("BasePlatform").transform;
        basePlatform.SetParent(monasParent);
        basePlatform.localPosition = Vector3.zero;
        
        // Ground level platform - large square
        CreateBox(basePlatform, new Vector3(0, 0.5f, 0), 
            new Vector3(baseSize * 2, 1f, baseSize * 2), "GroundPlatform", marbleGray);
        
        // First tier - raised platform
        CreateBox(basePlatform, new Vector3(0, 2f, 0), 
            new Vector3(baseSize * 1.5f, 3f, baseSize * 1.5f), "FirstTier", marbleWhite);
        
        // Second tier
        CreateBox(basePlatform, new Vector3(0, 5f, 0), 
            new Vector3(baseSize * 1.2f, 3f, baseSize * 1.2f), "SecondTier", marbleWhite);
        
        // Corner decorations on first tier
        float cornerOffset = baseSize * 0.7f;
        Vector3[] corners = {
            new Vector3(-cornerOffset, 3.5f, -cornerOffset),
            new Vector3(cornerOffset, 3.5f, -cornerOffset),
            new Vector3(-cornerOffset, 3.5f, cornerOffset),
            new Vector3(cornerOffset, 3.5f, cornerOffset)
        };
        
        for (int i = 0; i < corners.Length; i++)
        {
            CreateBox(basePlatform, corners[i], new Vector3(3f, 4f, 3f), $"CornerPillar_{i}", marbleGray);
        }
    }

    void CreateMainPlatform()
    {
        Transform mainPlatform = new GameObject("MainPlatform").transform;
        mainPlatform.SetParent(monasParent);
        mainPlatform.localPosition = new Vector3(0, 6.5f, 0);
        
        // Museum base (the main rectangular structure)
        CreateBox(mainPlatform, new Vector3(0, 4f, 0), 
            new Vector3(baseSize * 0.8f, 8f, baseSize * 0.8f), "MuseumBase", marbleWhite);
        
        // Decorative band around museum
        CreateBox(mainPlatform, new Vector3(0, 7f, 0), 
            new Vector3(baseSize * 0.85f, 1f, baseSize * 0.85f), "DecorativeBand", bronzeColor);
        
        // Relief panels (4 sides)
        float panelOffset = baseSize * 0.41f;
        CreateBox(mainPlatform, new Vector3(0, 4f, panelOffset), 
            new Vector3(baseSize * 0.5f, 5f, 0.5f), "ReliefFront", bronzeColor);
        CreateBox(mainPlatform, new Vector3(0, 4f, -panelOffset), 
            new Vector3(baseSize * 0.5f, 5f, 0.5f), "ReliefBack", bronzeColor);
        CreateBox(mainPlatform, new Vector3(panelOffset, 4f, 0), 
            new Vector3(0.5f, 5f, baseSize * 0.5f), "ReliefRight", bronzeColor);
        CreateBox(mainPlatform, new Vector3(-panelOffset, 4f, 0), 
            new Vector3(0.5f, 5f, baseSize * 0.5f), "ReliefLeft", bronzeColor);
    }

    void CreateObelisk()
    {
        Transform obelisk = new GameObject("Obelisk").transform;
        obelisk.SetParent(monasParent);
        obelisk.localPosition = new Vector3(0, 14.5f, 0);
        
        float obeliskHeight = monumentHeight - 25f;
        
        // Main obelisk shaft - tapered using multiple sections
        int sections = 8;
        float sectionHeight = obeliskHeight / sections;
        float baseWidth = 8f;
        float topWidth = 3f;
        
        for (int i = 0; i < sections; i++)
        {
            float t = (float)i / sections;
            float width = Mathf.Lerp(baseWidth, topWidth, t);
            float nextWidth = Mathf.Lerp(baseWidth, topWidth, (float)(i + 1) / sections);
            float avgWidth = (width + nextWidth) / 2f;
            
            float y = i * sectionHeight + sectionHeight / 2f;
            
            CreateBox(obelisk, new Vector3(0, y, 0), 
                new Vector3(avgWidth, sectionHeight + 0.1f, avgWidth), 
                $"ObeliskSection_{i}", marbleWhite);
        }
        
        // Top cap before flame
        CreateBox(obelisk, new Vector3(0, obeliskHeight + 1f, 0), 
            new Vector3(4f, 2f, 4f), "ObeliskTop", marbleGray);
        
        // Pelataran (observation deck) platform
        CreateBox(obelisk, new Vector3(0, obeliskHeight - 5f, 0), 
            new Vector3(6f, 1f, 6f), "ObservationDeck", marbleGray);
    }

    void CreateGoldenFlame()
    {
        Transform flame = new GameObject("GoldenFlame").transform;
        flame.SetParent(monasParent);
        
        float flameY = monumentHeight - 5f;
        flame.localPosition = new Vector3(0, flameY, 0);
        
        // Flame base (lidah api)
        GameObject flameBase = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flameBase.name = "FlameBase";
        flameBase.transform.SetParent(flame);
        flameBase.transform.localPosition = new Vector3(0, 0, 0);
        flameBase.transform.localScale = new Vector3(4f, 3f, 4f);
        ApplyMaterial(flameBase, goldColor, 0.8f, 0.9f);
        
        // Flame middle
        GameObject flameMid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flameMid.name = "FlameMid";
        flameMid.transform.SetParent(flame);
        flameMid.transform.localPosition = new Vector3(0, 2.5f, 0);
        flameMid.transform.localScale = new Vector3(3f, 4f, 3f);
        ApplyMaterial(flameMid, goldColor, 0.8f, 0.9f);
        
        // Flame tip
        GameObject flameTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flameTip.name = "FlameTip";
        flameTip.transform.SetParent(flame);
        flameTip.transform.localPosition = new Vector3(0, 5.5f, 0);
        flameTip.transform.localScale = new Vector3(1.5f, 3f, 1.5f);
        ApplyMaterial(flameTip, goldColor, 0.9f, 1f);
        
        // Flame top point
        GameObject flameTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flameTop.name = "FlameTop";
        flameTop.transform.SetParent(flame);
        flameTop.transform.localPosition = new Vector3(0, 7.5f, 0);
        flameTop.transform.localScale = new Vector3(0.6f, 1.5f, 0.6f);
        ApplyMaterial(flameTop, new Color(1f, 0.95f, 0.5f), 0.9f, 1f);
    }

    void CreateSurroundingPark()
    {
        Transform park = new GameObject("MedanMerdeka").transform;
        park.SetParent(monasParent);
        park.localPosition = Vector3.zero;
        
        // Large green park area
        float parkSize = baseSize * 3f;
        CreateBox(park, new Vector3(0, -0.05f, 0), 
            new Vector3(parkSize, 0.1f, parkSize), "ParkGround", groundColor);
        
        // Pathways to monument (4 directions)
        float pathWidth = 8f;
        float pathLength = parkSize / 2f - baseSize;
        
        // North path
        CreateBox(park, new Vector3(0, 0.05f, baseSize + pathLength/2), 
            new Vector3(pathWidth, 0.1f, pathLength), "PathNorth", marbleGray);
        // South path
        CreateBox(park, new Vector3(0, 0.05f, -(baseSize + pathLength/2)), 
            new Vector3(pathWidth, 0.1f, pathLength), "PathSouth", marbleGray);
        // East path
        CreateBox(park, new Vector3(baseSize + pathLength/2, 0.05f, 0), 
            new Vector3(pathLength, 0.1f, pathWidth), "PathEast", marbleGray);
        // West path
        CreateBox(park, new Vector3(-(baseSize + pathLength/2), 0.05f, 0), 
            new Vector3(pathLength, 0.1f, pathWidth), "PathWest", marbleGray);
        
        // Decorative fountains at corners
        float fountainDist = baseSize * 1.8f;
        Vector3[] fountainPos = {
            new Vector3(fountainDist, 0, fountainDist),
            new Vector3(-fountainDist, 0, fountainDist),
            new Vector3(fountainDist, 0, -fountainDist),
            new Vector3(-fountainDist, 0, -fountainDist)
        };
        
        for (int i = 0; i < fountainPos.Length; i++)
        {
            CreateFountain(park, fountainPos[i], $"Fountain_{i}");
        }
    }
    
    void CreateFountain(Transform parent, Vector3 pos, string name)
    {
        Transform fountain = new GameObject(name).transform;
        fountain.SetParent(parent);
        fountain.localPosition = pos;
        
        // Basin
        GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basin.name = "Basin";
        basin.transform.SetParent(fountain);
        basin.transform.localPosition = new Vector3(0, 0.3f, 0);
        basin.transform.localScale = new Vector3(6f, 0.3f, 6f);
        ApplyMaterial(basin, marbleGray);
        
        // Water
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water.name = "Water";
        water.transform.SetParent(fountain);
        water.transform.localPosition = new Vector3(0, 0.35f, 0);
        water.transform.localScale = new Vector3(5.5f, 0.1f, 5.5f);
        ApplyMaterial(water, new Color(0.3f, 0.5f, 0.7f, 0.8f));
        
        // Center piece
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        center.name = "CenterPiece";
        center.transform.SetParent(fountain);
        center.transform.localPosition = new Vector3(0, 1f, 0);
        center.transform.localScale = new Vector3(1f, 1.5f, 1f);
        ApplyMaterial(center, marbleWhite);
    }

    void CreateStairs()
    {
        Transform stairs = new GameObject("Stairs").transform;
        stairs.SetParent(monasParent);
        stairs.localPosition = Vector3.zero;
        
        // Main stairs on 4 sides
        float stairWidth = 12f;
        float stairDepth = 8f;
        
        // Front stairs (South)
        for (int i = 0; i < 5; i++)
        {
            float y = 0.3f + i * 0.4f;
            float z = baseSize + 2f - i * 1.5f;
            CreateBox(stairs, new Vector3(0, y, z), 
                new Vector3(stairWidth, 0.4f, 1.5f), $"StairFront_{i}", marbleGray);
        }
        
        // Back stairs (North)
        for (int i = 0; i < 5; i++)
        {
            float y = 0.3f + i * 0.4f;
            float z = -(baseSize + 2f - i * 1.5f);
            CreateBox(stairs, new Vector3(0, y, z), 
                new Vector3(stairWidth, 0.4f, 1.5f), $"StairBack_{i}", marbleGray);
        }
    }

    void CreateBox(Transform parent, Vector3 localPos, Vector3 scale, string name, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = scale;
        ApplyMaterial(obj, color);
    }
    
    void ApplyMaterial(GameObject obj, Color color, float metallic = 0.1f, float smoothness = 0.3f)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null) return;
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        rend.sharedMaterial = mat;
        
        // Keep collider for solid objects!
    }

    [ContextMenu("Clear Monas")]
    public void ClearMonas()
    {
        GameObject existing = GameObject.Find("_Monas");
        if (existing != null) DestroyImmediate(existing);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MonasGenerator))]
public class MonasGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MonasGenerator generator = (MonasGenerator)target;
        
        GUILayout.Space(15);
        
        EditorGUILayout.HelpBox(
            "Monas - Monumen Nasional\n\n" +
            "Landmark Jakarta, Indonesia\n" +
            "- Obelisk dengan api emas di puncak\n" +
            "- Base museum dengan relief\n" +
            "- Taman Medan Merdeka\n" +
            "- Air mancur dekoratif", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
        if (GUILayout.Button("GENERATE MONAS", GUILayout.Height(45)))
        {
            generator.GenerateMonas();
        }
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
        if (GUILayout.Button("Clear Monas", GUILayout.Height(25)))
        {
            generator.ClearMonas();
        }
        
        GUI.backgroundColor = Color.white;
    }
}
#endif

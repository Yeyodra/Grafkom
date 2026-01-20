using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BuildingGenerator : MonoBehaviour
{
    public enum BuildingStyle { Random, Office, Skyscraper, Warehouse, Modern, Residential, Commercial }
    public enum GenerateMode { Replace, Add }
    
    [Header("Generation Settings")]
    public int numberOfBuildings = 10;
    public float areaSize = 100f;
    public float minSpacing = 15f;
    public BuildingStyle preferredStyle = BuildingStyle.Random;
    public GenerateMode mode = GenerateMode.Add;  // NEW: Add or Replace mode
    
    [Header("Building Size Ranges")]
    public float minHeight = 10f;
    public float maxHeight = 40f;
    public float minWidth = 6f;
    public float maxWidth = 14f;
    
    [Header("Materials (Optional)")]
    public Material baseMaterial;
    public Material bodyMaterial;
    public Material glassMaterial;
    public Material roofMaterial;
    
    private List<Vector3> usedPositions = new List<Vector3>();
    private static int buildingIndex = 1;
    
    [ContextMenu("Generate Buildings")]
    public void GenerateBuildings()
    {
        Transform existingParent = transform.Find("GeneratedBuildings");
        
        if (mode == GenerateMode.Replace)
        {
            // Clear existing
            if (existingParent != null)
                DestroyImmediate(existingParent.gameObject);
            usedPositions.Clear();
            buildingIndex = 1;
            existingParent = null;
        }
        else
        {
            // Add mode - collect existing positions
            if (existingParent != null)
            {
                foreach (Transform child in existingParent)
                {
                    usedPositions.Add(child.position);
                }
            }
        }
        
        // Create or get parent
        GameObject parent;
        if (existingParent != null)
        {
            parent = existingParent.gameObject;
        }
        else
        {
            parent = new GameObject("GeneratedBuildings");
            parent.transform.SetParent(transform);
        }
        
        int generated = 0;
        int attempts = 0;
        int maxAttempts = numberOfBuildings * 20;
        
        while (generated < numberOfBuildings && attempts < maxAttempts)
        {
            attempts++;
            Vector3 pos = GetRandomPosition();
            
            if (IsPositionValid(pos))
            {
                BuildingStyle style = preferredStyle == BuildingStyle.Random 
                    ? (BuildingStyle)Random.Range(1, 7) 
                    : preferredStyle;
                    
                CreateBuilding(pos, parent.transform, style);
                usedPositions.Add(pos);
                generated++;
            }
        }
        
        Debug.Log($"BuildingGenerator: Added {generated} {preferredStyle} buildings (Total attempts: {attempts})");
    }
    
    [ContextMenu("Clear All Generated Buildings")]
    public void ClearGeneratedBuildings()
    {
        Transform existingParent = transform.Find("GeneratedBuildings");
        if (existingParent != null)
            DestroyImmediate(existingParent.gameObject);
        usedPositions.Clear();
        buildingIndex = 1;
        Debug.Log("BuildingGenerator: Cleared all generated buildings");
    }
    
    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-areaSize / 2f, areaSize / 2f);
        float z = Random.Range(-areaSize / 2f, areaSize / 2f);
        return new Vector3(x, 0, z);
    }
    
    bool IsPositionValid(Vector3 pos)
    {
        foreach (Vector3 usedPos in usedPositions)
            if (Vector3.Distance(pos, usedPos) < minSpacing) return false;
        return true;
    }
    
    void CreateBuilding(Vector3 position, Transform parent, BuildingStyle style)
    {
        GameObject building = new GameObject($"Building_{style}_{buildingIndex:D2}");
        building.transform.SetParent(parent);
        building.transform.position = position;
        
        switch (style)
        {
            case BuildingStyle.Office: CreateOfficeBuilding(building.transform); break;
            case BuildingStyle.Skyscraper: CreateSkyscraper(building.transform); break;
            case BuildingStyle.Warehouse: CreateWarehouse(building.transform); break;
            case BuildingStyle.Modern: CreateModernBuilding(building.transform); break;
            case BuildingStyle.Residential: CreateResidential(building.transform); break;
            case BuildingStyle.Commercial: CreateCommercial(building.transform); break;
        }
        
        buildingIndex++;
    }
    
    void CreateOfficeBuilding(Transform parent)
    {
        float width = Random.Range(8f, 12f);
        float height = Random.Range(15f, 30f);
        float depth = Random.Range(8f, 12f);
        
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 1.5f, 0), new Vector3(width + 2, 3, depth + 2), "Base", baseMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 3 + height/2, 0), new Vector3(width, height, depth), "Body", bodyMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 3 + height + 0.5f, 0), new Vector3(width + 1, 1, depth + 1), "Roof", roofMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 2, depth/2 + 1), new Vector3(4, 4, 2), "Entrance", baseMaterial);
        
        for (int i = 0; i < 3; i++)
        {
            float xOff = Random.Range(-width/3, width/3);
            float zOff = Random.Range(-depth/3, depth/3);
            CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(xOff, 3 + height + 1.5f, zOff), new Vector3(1.5f, 1, 1.5f), $"AC_{i}", null);
        }
    }
    
    void CreateSkyscraper(Transform parent)
    {
        float baseWidth = Random.Range(10f, 14f);
        float totalHeight = Random.Range(40f, 60f);
        
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 2.5f, 0), new Vector3(baseWidth + 4, 5, baseWidth + 4), "Base", baseMaterial);
        
        float tier1Height = totalHeight * 0.4f;
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 5 + tier1Height/2, 0), new Vector3(baseWidth, tier1Height, baseWidth), "Tier1", bodyMaterial);
        
        float tier2Height = totalHeight * 0.35f;
        float tier2Width = baseWidth * 0.75f;
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 5 + tier1Height + tier2Height/2, 0), new Vector3(tier2Width, tier2Height, tier2Width), "Tier2", bodyMaterial);
        
        float tier3Height = totalHeight * 0.25f;
        float tier3Width = baseWidth * 0.5f;
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 5 + tier1Height + tier2Height + tier3Height/2, 0), new Vector3(tier3Width, tier3Height, tier3Width), "Tier3", glassMaterial ?? bodyMaterial);
        
        float spireHeight = Random.Range(5f, 10f);
        CreatePrimitive(PrimitiveType.Cylinder, parent, new Vector3(0, 5 + totalHeight + spireHeight/2, 0), new Vector3(0.5f, spireHeight/2, 0.5f), "Spire", null);
        CreatePrimitive(PrimitiveType.Cylinder, parent, new Vector3(tier3Width/3, 5 + totalHeight + 0.1f, 0), new Vector3(3, 0.1f, 3), "Helipad", roofMaterial);
    }
    
    void CreateWarehouse(Transform parent)
    {
        float width = Random.Range(15f, 25f);
        float height = Random.Range(6f, 10f);
        float depth = Random.Range(20f, 35f);
        
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height/2, 0), new Vector3(width, height, depth), "Main", bodyMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height + 1, 0), new Vector3(width + 2, 0.5f, depth + 2), "Roof", roofMaterial);
        
        for (int i = 0; i < 3; i++)
        {
            float zPos = -depth/3 + (i * depth/3);
            CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(width/2 + 1, 2, zPos), new Vector3(2, 4, 5), $"Dock_{i}", baseMaterial);
        }
        
        for (int i = 0; i < 4; i++)
        {
            float zPos = -depth/2.5f + (i * depth/4);
            CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(-width/2 + 0.1f, 3, zPos), new Vector3(0.2f, 5, 4), $"Door_{i}", glassMaterial ?? baseMaterial);
        }
        
        CreatePrimitive(PrimitiveType.Cylinder, parent, new Vector3(0, height + 2.5f, 0), new Vector3(2, 1, 2), "Vent", null);
    }
    
    void CreateModernBuilding(Transform parent)
    {
        float width = Random.Range(10f, 16f);
        float height = Random.Range(20f, 35f);
        float depth = Random.Range(8f, 12f);
        
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height/2, 0), new Vector3(width, height, depth), "GlassBody", glassMaterial ?? bodyMaterial);
        
        float wingWidth = width * 0.4f;
        float wingHeight = height * 0.6f;
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(width/2 + wingWidth/2, wingHeight/2, 0), new Vector3(wingWidth, wingHeight, depth * 0.8f), "Wing", bodyMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(-width/4, height - 3, depth/2 + 2), new Vector3(width/2, 4, 4), "Cantilever", glassMaterial ?? bodyMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 2.5f, depth/2 + 3), new Vector3(width - 2, 5, 6), "Lobby", glassMaterial ?? baseMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height + 0.5f, 0), new Vector3(width - 2, 1, depth - 2), "RoofGarden", roofMaterial);
    }
    
    void CreateResidential(Transform parent)
    {
        float width = Random.Range(12f, 18f);
        float floors = Random.Range(5, 12);
        float floorHeight = 3f;
        float height = floors * floorHeight;
        float depth = Random.Range(10f, 14f);
        
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height/2, 0), new Vector3(width, height, depth), "Main", bodyMaterial);
        
        for (int f = 1; f <= floors; f++)
        {
            float y = f * floorHeight - 1;
            for (int i = 0; i < 3; i++)
            {
                float x = -width/3 + (i * width/3);
                CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(x, y, depth/2 + 0.75f), new Vector3(width/4, 0.2f, 1.5f), $"Balcony_F{f}_{i}", baseMaterial);
                CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(x, y + 0.5f, depth/2 + 1.4f), new Vector3(width/4, 1, 0.1f), $"Rail_F{f}_{i}", null);
            }
        }
        
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, 3, depth/2 + 2), new Vector3(6, 0.3f, 4), "Canopy", roofMaterial);
        CreatePrimitive(PrimitiveType.Cylinder, parent, new Vector3(-2.5f, 1.5f, depth/2 + 3.5f), new Vector3(0.3f, 1.5f, 0.3f), "Pillar_L", null);
        CreatePrimitive(PrimitiveType.Cylinder, parent, new Vector3(2.5f, 1.5f, depth/2 + 3.5f), new Vector3(0.3f, 1.5f, 0.3f), "Pillar_R", null);
        CreatePrimitive(PrimitiveType.Cylinder, parent, new Vector3(width/3, height + 2, -depth/4), new Vector3(2, 2, 2), "WaterTank", null);
    }
    
    void CreateCommercial(Transform parent)
    {
        float width = Random.Range(25f, 40f);
        float height = Random.Range(8f, 15f);
        float depth = Random.Range(20f, 30f);
        
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height/2, 0), new Vector3(width, height, depth), "Main", bodyMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height/3, depth/2 + 0.5f), new Vector3(width - 4, height * 0.6f, 1), "Storefront", glassMaterial ?? bodyMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height/2 + 1, depth/2 + 4), new Vector3(width/2, 1, 8), "Overhang", roofMaterial);
        
        for (int i = 0; i < 4; i++)
        {
            float x = -width/4 + 0.5f + (i * width/6);
            CreatePrimitive(PrimitiveType.Cylinder, parent, new Vector3(x, height/4, depth/2 + 7), new Vector3(0.4f, height/4, 0.4f), $"Column_{i}", null);
        }
        
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(0, height + 2, 0), new Vector3(width/2, 4, 1), "Signage", roofMaterial);
        CreatePrimitive(PrimitiveType.Cube, parent, new Vector3(-width/2 - 8, 3, 0), new Vector3(12, 6, depth * 0.8f), "Parking", baseMaterial);
    }
    
    GameObject CreatePrimitive(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, string name, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = scale;
        if (mat != null) obj.GetComponent<Renderer>().material = mat;
        return obj;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        BuildingGenerator gen = (BuildingGenerator)target;
        
        GUILayout.Space(10);
        
        // Mode indicator
        string modeText = gen.mode == BuildingGenerator.GenerateMode.Add 
            ? "Mode: ADD (keeps existing buildings)" 
            : "Mode: REPLACE (clears existing)";
        EditorGUILayout.HelpBox(modeText, MessageType.Info);
        
        GUILayout.Space(5);
        
        EditorGUILayout.HelpBox(
            "Building Styles:\n" +
            "- Office: Standard box with AC units\n" +
            "- Skyscraper: Tall tiered tower with spire\n" +
            "- Warehouse: Low industrial with loading docks\n" +
            "- Modern: Glass contemporary with cantilevers\n" +
            "- Residential: Apartment with balconies\n" +
            "- Commercial: Shopping mall with parking", 
            MessageType.None);
        
        GUILayout.Space(10);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button($"GENERATE {gen.numberOfBuildings} {gen.preferredStyle.ToString().ToUpper()}", GUILayout.Height(40)))
            gen.GenerateBuildings();
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear All Generated Buildings", GUILayout.Height(25)))
            gen.ClearGeneratedBuildings();
        
        GUI.backgroundColor = Color.white;
    }
}
#endif

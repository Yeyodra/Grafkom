using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneColorizer : MonoBehaviour
{
    [Header("Ground")]
    public Color groundColor = new Color(0.35f, 0.55f, 0.35f);
    
    [Header("Roads")]
    public Color roadColor = new Color(0.2f, 0.2f, 0.2f);
    
    [Header("Trees")]
    public Color foliageColor = new Color(0.2f, 0.55f, 0.2f);
    public Color trunkColor = new Color(0.4f, 0.25f, 0.1f);
    
    [Header("Vehicles")]
    public Color car1Color = new Color(0.8f, 0.1f, 0.1f);
    public Color car2Color = new Color(0.1f, 0.2f, 0.8f);
    public Color wheelColor = new Color(0.15f, 0.15f, 0.15f);
    
    [Header("Street Furniture")]
    public Color benchColor = new Color(0.45f, 0.3f, 0.15f);
    public Color trashBinColor = new Color(0.3f, 0.5f, 0.3f);
    public Color hydrantColor = new Color(0.9f, 0.2f, 0.1f);
    public Color fountainColor = new Color(0.6f, 0.6f, 0.65f);
    
    [Header("Street Lamps")]
    public Color lampPostColor = new Color(0.25f, 0.25f, 0.25f);
    
    [Header("Props")]
    public Color crateColor = new Color(0.6f, 0.45f, 0.25f);
    public Color barrierColor = new Color(1f, 0.5f, 0f);
    public Color ballColor = new Color(0.9f, 0.1f, 0.3f);
    
    [Header("Building Colors - Commercial (Blue tones)")]
    public Color[] commercialColors = new Color[]
    {
        new Color(0.2f, 0.4f, 0.7f),
        new Color(0.3f, 0.5f, 0.8f),
        new Color(0.4f, 0.6f, 0.9f),
        new Color(0.25f, 0.35f, 0.6f),
    };
    
    [Header("Building Colors - Residential (Warm tones)")]
    public Color[] residentialColors = new Color[]
    {
        new Color(0.9f, 0.85f, 0.7f),
        new Color(0.85f, 0.75f, 0.65f),
        new Color(0.95f, 0.9f, 0.8f),
        new Color(0.8f, 0.7f, 0.6f),
    };
    
    [Header("Building Colors - Office (Gray tones)")]
    public Color[] officeColors = new Color[]
    {
        new Color(0.6f, 0.6f, 0.65f),
        new Color(0.5f, 0.55f, 0.6f),
        new Color(0.45f, 0.5f, 0.55f),
        new Color(0.7f, 0.7f, 0.75f),
    };
    
    [Header("Building Colors - Mixed (Terracotta tones)")]
    public Color[] mixedColors = new Color[]
    {
        new Color(0.7f, 0.5f, 0.3f),
        new Color(0.6f, 0.4f, 0.35f),
        new Color(0.55f, 0.45f, 0.4f),
        new Color(0.65f, 0.55f, 0.45f),
    };
    
    [Header("Building Settings")]
    [Range(0f, 0.15f)]
    public float colorVariation = 0.08f;

    [ContextMenu("Apply All Colors")]
    public void ApplyAllColors()
    {
        // Ground
        ApplyColor("Ground", groundColor);
        
        // Roads
        ApplyColor("Road_Main", roadColor);
        ApplyColor("Road_Cross", roadColor);
        
        // Trees
        for (int i = 1; i <= 5; i++)
        {
            ApplyColor($"Tree_0{i}_Foliage", foliageColor);
            ApplyColor($"Tree_0{i}_Trunk", trunkColor);
        }
        
        // Cars
        ApplyColor("Car_01_Body", car1Color);
        ApplyColor("Car_01_Top", car1Color * 0.9f);
        ApplyColor("Car_02_Body", car2Color);
        ApplyColor("Car_02_Top", car2Color * 0.9f);
        
        // Wheels
        string[] wheels = {"Car_01_Wheel_FL", "Car_01_Wheel_FR", "Car_01_Wheel_BL", "Car_01_Wheel_BR"};
        foreach (var w in wheels) ApplyColor(w, wheelColor);
        
        // Bench
        ApplyColor("Bench_01_Seat", benchColor);
        ApplyColor("Bench_01_Back", benchColor);
        ApplyColor("Bench_01_Leg_L", benchColor * 0.7f);
        ApplyColor("Bench_01_Leg_R", benchColor * 0.7f);
        
        // Trash & Hydrant
        ApplyColor("TrashBin_01", trashBinColor);
        ApplyColor("Hydrant_01_Body", hydrantColor);
        ApplyColor("Hydrant_01_Top", hydrantColor * 0.8f);
        
        // Fountain
        ApplyColor("Fountain_01_Base", fountainColor);
        ApplyColor("Fountain_01_Pillar", fountainColor * 0.9f);
        ApplyColor("Fountain_01_Bowl", fountainColor);
        ApplyColor("Fountain_01_Top", fountainColor * 0.8f);
        
        // Street Lamps
        ApplyColor("StreetLamp_01", lampPostColor);
        ApplyColor("StreetLamp_02", lampPostColor);
        
        // Props
        ApplyColor("Crate_01", crateColor);
        ApplyColor("Crate_02", crateColor * 0.9f);
        ApplyColor("Crate_03", crateColor * 1.1f);
        ApplyColor("Barrier_01", barrierColor);
        ApplyColor("Ball_01", ballColor);
        
        // Buildings
        ColorizeAllBuildings();
        
        Debug.Log("SceneColorizer: All colors applied!");
    }
    
    [ContextMenu("Colorize Buildings Only")]
    public void ColorizeAllBuildings()
    {
        int count = 0;
        
        // Try _CityBlocks first (from CityLayoutManager)
        GameObject cityBlocks = GameObject.Find("_CityBlocks");
        if (cityBlocks != null)
        {
            foreach (Transform block in cityBlocks.transform)
            {
                string blockName = block.name.ToLower();
                Color[] palette = GetBuildingPalette(blockName);
                
                foreach (Transform building in block)
                {
                    ColorizeBuilding(building.gameObject, palette);
                    count++;
                }
            }
        }
        
        // Also try buildings under _CityLayoutManager
        GameObject cityLayout = GameObject.Find("_CityLayoutManager");
        if (cityLayout != null)
        {
            // Check for _CityBlocks child
            Transform blocksChild = cityLayout.transform.Find("_CityBlocks");
            if (blocksChild != null)
            {
                foreach (Transform block in blocksChild)
                {
                    string blockName = block.name.ToLower();
                    Color[] palette = GetBuildingPalette(blockName);
                    
                    foreach (Transform building in block)
                    {
                        ColorizeBuilding(building.gameObject, palette);
                        count++;
                    }
                }
            }
        }
        
        // Try individual Building_ objects in scene
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Building_") || obj.name.Contains("_Building"))
            {
                string parentName = obj.transform.parent != null ? obj.transform.parent.name.ToLower() : "";
                Color[] palette = GetBuildingPalette(parentName);
                ColorizeBuilding(obj, palette);
                count++;
            }
        }
        
        Debug.Log($"SceneColorizer: Colorized {count} buildings!");
    }
    
    Color[] GetBuildingPalette(string contextName)
    {
        if (contextName.Contains("commercial"))
            return commercialColors;
        else if (contextName.Contains("residential"))
            return residentialColors;
        else if (contextName.Contains("office"))
            return officeColors;
        else if (contextName.Contains("mixed"))
            return mixedColors;
        else
            return new Color[] { officeColors[0], commercialColors[0], residentialColors[0], mixedColors[0] };
    }
    
    void ColorizeBuilding(GameObject building, Color[] palette)
    {
        Renderer[] renderers = building.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            Color baseColor = palette[Random.Range(0, palette.Length)];
            Color variedColor = AddColorVariation(baseColor);
            
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_BaseColor", variedColor);
            mat.color = variedColor;
            rend.sharedMaterial = mat;
        }
    }
    
    Color AddColorVariation(Color baseColor)
    {
        float r = Mathf.Clamp01(baseColor.r + Random.Range(-colorVariation, colorVariation));
        float g = Mathf.Clamp01(baseColor.g + Random.Range(-colorVariation, colorVariation));
        float b = Mathf.Clamp01(baseColor.b + Random.Range(-colorVariation, colorVariation));
        return new Color(r, g, b);
    }
    
    void ApplyColor(string objectName, Color color)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                // Create new material instance
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (mat == null) mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                rend.sharedMaterial = mat;
            }
        }
    }
    
    [ContextMenu("Apply Ground Color Only")]
    public void ApplyGroundOnly()
    {
        ApplyColor("Ground", groundColor);
        Debug.Log("Ground color applied!");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneColorizer))]
public class SceneColorizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SceneColorizer colorizer = (SceneColorizer)target;
        
        GUILayout.Space(15);
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("APPLY ALL COLORS", GUILayout.Height(40)))
        {
            colorizer.ApplyAllColors();
        }
        
        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.9f);
        if (GUILayout.Button("COLORIZE BUILDINGS ONLY", GUILayout.Height(35)))
        {
            colorizer.ColorizeAllBuildings();
        }
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Apply Ground Only", GUILayout.Height(25)))
        {
            colorizer.ApplyGroundOnly();
        }
        
        GUI.backgroundColor = Color.white;
    }
}
#endif

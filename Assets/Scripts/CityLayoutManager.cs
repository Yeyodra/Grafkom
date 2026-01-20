using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CityLayoutManager : MonoBehaviour
{
    [Header("City Grid Settings")]
    public float blockSize = 40f;
    public float roadWidth = 12f;
    public int gridSizeX = 4;
    public int gridSizeZ = 4;
    
    [Header("Road Settings")]
    public float roadHeight = 0.15f;
    public Material roadMaterial;
    public Material sidewalkMaterial;
    
    [Header("Building Settings")]
    public float buildingMargin = 3f;
    public float minBuildingHeight = 8f;
    public float maxBuildingHeight = 45f;
    
    [Header("Materials")]
    public Material buildingBaseMat;
    public Material buildingBodyMat;
    public Material buildingGlassMat;
    public Material buildingRoofMat;

    private Transform roadsParent;
    private Transform blocksParent;

    [ContextMenu("Generate City Layout")]
    public void GenerateCityLayout()
    {
        ClearCity();
        
        // Create parent containers
        roadsParent = new GameObject("_CityRoads").transform;
        roadsParent.SetParent(transform);
        
        blocksParent = new GameObject("_CityBlocks").transform;
        blocksParent.SetParent(transform);
        
        // Calculate total size
        float totalX = gridSizeX * (blockSize + roadWidth) + roadWidth;
        float totalZ = gridSizeZ * (blockSize + roadWidth) + roadWidth;
        float offsetX = -totalX / 2f;
        float offsetZ = -totalZ / 2f;
        
        // Generate road grid
        GenerateRoads(offsetX, offsetZ, totalX, totalZ);
        
        // Generate city blocks with buildings
        GenerateCityBlocks(offsetX, offsetZ);
        
        // Add special features
        AddIntersectionDetails();
        AddSidewalks(offsetX, offsetZ, totalX, totalZ);
        
        Debug.Log($"City Layout Generated: {gridSizeX}x{gridSizeZ} blocks");
    }

    void GenerateRoads(float offsetX, float offsetZ, float totalX, float totalZ)
    {
        Transform roadContainer = new GameObject("Roads").transform;
        roadContainer.SetParent(roadsParent);
        
        // Horizontal roads (along X axis)
        for (int z = 0; z <= gridSizeZ; z++)
        {
            float zPos = offsetZ + z * (blockSize + roadWidth) + roadWidth / 2f;
            CreateRoadSegment(roadContainer, 
                new Vector3(0, roadHeight, zPos), 
                new Vector3(totalX, 0.1f, roadWidth), 
                $"Road_H_{z}");
        }
        
        // Vertical roads (along Z axis)
        for (int x = 0; x <= gridSizeX; x++)
        {
            float xPos = offsetX + x * (blockSize + roadWidth) + roadWidth / 2f;
            CreateRoadSegment(roadContainer, 
                new Vector3(xPos, roadHeight, 0), 
                new Vector3(roadWidth, 0.1f, totalZ), 
                $"Road_V_{x}");
        }
        
        // Main boulevard (wider center road)
        if (gridSizeX >= 2)
        {
            int centerX = gridSizeX / 2;
            float xPos = offsetX + centerX * (blockSize + roadWidth) + roadWidth / 2f;
            // Widen center road
            Transform centerRoad = roadContainer.Find($"Road_V_{centerX}");
            if (centerRoad != null)
            {
                centerRoad.localScale = new Vector3(roadWidth * 1.5f, 0.1f, totalZ);
                centerRoad.name = "MainBoulevard";
            }
        }
    }

    void CreateRoadSegment(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = name;
        road.transform.SetParent(parent);
        road.transform.position = position;
        road.transform.localScale = scale;
        if (roadMaterial != null)
            road.GetComponent<Renderer>().material = roadMaterial;
        
        // Add lane markings
        if (scale.x > scale.z) // Horizontal road
        {
            CreateLaneMarking(parent, position, scale.x, true);
        }
        else // Vertical road
        {
            CreateLaneMarking(parent, position, scale.z, false);
        }
    }

    void CreateLaneMarking(Transform parent, Vector3 roadPos, float length, bool horizontal)
    {
        float dashLength = 3f;
        float gapLength = 2f;
        float totalDashUnit = dashLength + gapLength;
        int numDashes = Mathf.FloorToInt(length / totalDashUnit);
        
        for (int i = 0; i < numDashes; i++)
        {
            GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dash.name = $"LaneMarking_{i}";
            dash.transform.SetParent(parent);
            
            float offset = -length / 2f + i * totalDashUnit + dashLength / 2f;
            
            if (horizontal)
            {
                dash.transform.position = new Vector3(roadPos.x + offset, roadPos.y + 0.06f, roadPos.z);
                dash.transform.localScale = new Vector3(dashLength, 0.02f, 0.2f);
            }
            else
            {
                dash.transform.position = new Vector3(roadPos.x, roadPos.y + 0.06f, roadPos.z + offset);
                dash.transform.localScale = new Vector3(0.2f, 0.02f, dashLength);
            }
            
            dash.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    void GenerateCityBlocks(float offsetX, float offsetZ)
    {
        string[] blockTypes = { "Commercial", "Residential", "Office", "Mixed" };
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                float blockX = offsetX + roadWidth + x * (blockSize + roadWidth) + blockSize / 2f;
                float blockZ = offsetZ + roadWidth + z * (blockSize + roadWidth) + blockSize / 2f;
                
                string blockType = blockTypes[(x + z) % blockTypes.Length];
                CreateCityBlock(blockX, blockZ, x, z, blockType);
            }
        }
    }

    void CreateCityBlock(float centerX, float centerZ, int gridX, int gridZ, string blockType)
    {
        GameObject block = new GameObject($"Block_{gridX}_{gridZ}_{blockType}");
        block.transform.SetParent(blocksParent);
        block.transform.position = new Vector3(centerX, 0, centerZ);
        
        float usableSize = blockSize - buildingMargin * 2;
        
        switch (blockType)
        {
            case "Commercial":
                // One large building or 2 medium
                if (Random.value > 0.5f)
                {
                    CreateBuilding(block.transform, Vector3.zero, usableSize * 0.8f, usableSize * 0.8f, 
                        Random.Range(10f, 20f), BuildingStyle.Commercial);
                }
                else
                {
                    CreateBuilding(block.transform, new Vector3(-usableSize/4, 0, 0), usableSize * 0.4f, usableSize * 0.8f,
                        Random.Range(12f, 25f), BuildingStyle.Commercial);
                    CreateBuilding(block.transform, new Vector3(usableSize/4, 0, 0), usableSize * 0.4f, usableSize * 0.8f,
                        Random.Range(12f, 25f), BuildingStyle.Commercial);
                }
                break;
                
            case "Residential":
                // Multiple smaller buildings
                float spacing = usableSize / 3f;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector3 pos = new Vector3(
                            -usableSize/4 + i * usableSize/2,
                            0,
                            -usableSize/4 + j * usableSize/2
                        );
                        CreateBuilding(block.transform, pos, usableSize * 0.35f, usableSize * 0.35f,
                            Random.Range(15f, 35f), BuildingStyle.Residential);
                    }
                }
                break;
                
            case "Office":
                // Tall buildings
                bool isCorner = (gridX == 0 || gridX == gridSizeX - 1) && (gridZ == 0 || gridZ == gridSizeZ - 1);
                if (isCorner)
                {
                    // Skyscraper on corners
                    CreateBuilding(block.transform, Vector3.zero, usableSize * 0.6f, usableSize * 0.6f,
                        Random.Range(35f, 55f), BuildingStyle.Skyscraper);
                }
                else
                {
                    CreateBuilding(block.transform, new Vector3(-usableSize/4, 0, 0), usableSize * 0.35f, usableSize * 0.7f,
                        Random.Range(20f, 40f), BuildingStyle.Office);
                    CreateBuilding(block.transform, new Vector3(usableSize/4, 0, 0), usableSize * 0.35f, usableSize * 0.7f,
                        Random.Range(25f, 45f), BuildingStyle.Modern);
                }
                break;
                
            case "Mixed":
                // Mix of styles
                CreateBuilding(block.transform, new Vector3(-usableSize/3, 0, -usableSize/3), usableSize * 0.3f, usableSize * 0.3f,
                    Random.Range(8f, 15f), BuildingStyle.Warehouse);
                CreateBuilding(block.transform, new Vector3(usableSize/3, 0, -usableSize/3), usableSize * 0.3f, usableSize * 0.3f,
                    Random.Range(15f, 30f), BuildingStyle.Office);
                CreateBuilding(block.transform, new Vector3(0, 0, usableSize/3), usableSize * 0.5f, usableSize * 0.3f,
                    Random.Range(20f, 35f), BuildingStyle.Modern);
                break;
        }
    }

    enum BuildingStyle { Office, Skyscraper, Warehouse, Modern, Residential, Commercial }

    void CreateBuilding(Transform parent, Vector3 localPos, float width, float depth, float height, BuildingStyle style)
    {
        GameObject building = new GameObject($"Building_{style}");
        building.transform.SetParent(parent);
        building.transform.localPosition = localPos;
        
        switch (style)
        {
            case BuildingStyle.Office:
                CreateOfficeStyle(building.transform, width, depth, height);
                break;
            case BuildingStyle.Skyscraper:
                CreateSkyscraperStyle(building.transform, width, depth, height);
                break;
            case BuildingStyle.Warehouse:
                CreateWarehouseStyle(building.transform, width, depth, height);
                break;
            case BuildingStyle.Modern:
                CreateModernStyle(building.transform, width, depth, height);
                break;
            case BuildingStyle.Residential:
                CreateResidentialStyle(building.transform, width, depth, height);
                break;
            case BuildingStyle.Commercial:
                CreateCommercialStyle(building.transform, width, depth, height);
                break;
        }
    }

    void CreateOfficeStyle(Transform parent, float w, float d, float h)
    {
        // Base
        CreatePrimitive(parent, new Vector3(0, 1.5f, 0), new Vector3(w + 2, 3, d + 2), "Base", buildingBaseMat);
        // Main body
        CreatePrimitive(parent, new Vector3(0, 3 + h/2, 0), new Vector3(w, h, d), "Body", buildingBodyMat);
        // Roof
        CreatePrimitive(parent, new Vector3(0, 3 + h + 0.5f, 0), new Vector3(w + 1, 1, d + 1), "Roof", buildingRoofMat);
        // Entrance
        CreatePrimitive(parent, new Vector3(0, 2, d/2 + 1), new Vector3(4, 4, 2), "Entrance", buildingBaseMat);
    }

    void CreateSkyscraperStyle(Transform parent, float w, float d, float h)
    {
        // Wide base
        CreatePrimitive(parent, new Vector3(0, 2.5f, 0), new Vector3(w + 4, 5, d + 4), "Base", buildingBaseMat);
        // Tier 1
        float t1h = h * 0.5f;
        CreatePrimitive(parent, new Vector3(0, 5 + t1h/2, 0), new Vector3(w, t1h, d), "Tier1", buildingBodyMat);
        // Tier 2
        float t2h = h * 0.35f;
        CreatePrimitive(parent, new Vector3(0, 5 + t1h + t2h/2, 0), new Vector3(w * 0.75f, t2h, d * 0.75f), "Tier2", buildingGlassMat ?? buildingBodyMat);
        // Tier 3
        float t3h = h * 0.15f;
        CreatePrimitive(parent, new Vector3(0, 5 + t1h + t2h + t3h/2, 0), new Vector3(w * 0.5f, t3h, d * 0.5f), "Tier3", buildingGlassMat ?? buildingBodyMat);
        // Spire
        CreatePrimitiveCylinder(parent, new Vector3(0, 5 + h + 4, 0), new Vector3(0.5f, 4, 0.5f), "Spire");
    }

    void CreateWarehouseStyle(Transform parent, float w, float d, float h)
    {
        h = Mathf.Min(h, 12f); // Warehouses are short
        CreatePrimitive(parent, new Vector3(0, h/2, 0), new Vector3(w, h, d), "Main", buildingBodyMat);
        CreatePrimitive(parent, new Vector3(0, h + 0.5f, 0), new Vector3(w + 2, 1, d + 2), "Roof", buildingRoofMat);
        // Loading docks
        CreatePrimitive(parent, new Vector3(w/2 + 1, 2, 0), new Vector3(2, 4, d * 0.6f), "Dock", buildingBaseMat);
    }

    void CreateModernStyle(Transform parent, float w, float d, float h)
    {
        // Glass main body
        CreatePrimitive(parent, new Vector3(0, h/2, 0), new Vector3(w, h, d), "GlassBody", buildingGlassMat ?? buildingBodyMat);
        // Cantilever
        CreatePrimitive(parent, new Vector3(-w/4, h - 3, d/2 + 2), new Vector3(w/2, 4, 4), "Cantilever", buildingGlassMat ?? buildingBodyMat);
        // Ground lobby
        CreatePrimitive(parent, new Vector3(0, 2.5f, d/2 + 2), new Vector3(w - 2, 5, 4), "Lobby", buildingGlassMat ?? buildingBaseMat);
    }

    void CreateResidentialStyle(Transform parent, float w, float d, float h)
    {
        CreatePrimitive(parent, new Vector3(0, h/2, 0), new Vector3(w, h, d), "Main", buildingBodyMat);
        // Balconies
        int floors = Mathf.FloorToInt(h / 3f);
        for (int f = 1; f <= Mathf.Min(floors, 8); f++)
        {
            float y = f * 3f;
            CreatePrimitive(parent, new Vector3(0, y, d/2 + 0.75f), new Vector3(w * 0.8f, 0.2f, 1.5f), $"Balcony_{f}", buildingBaseMat);
        }
        // Entrance canopy
        CreatePrimitive(parent, new Vector3(0, 3, d/2 + 1.5f), new Vector3(5, 0.3f, 3), "Canopy", buildingRoofMat);
    }

    void CreateCommercialStyle(Transform parent, float w, float d, float h)
    {
        h = Mathf.Min(h, 18f);
        CreatePrimitive(parent, new Vector3(0, h/2, 0), new Vector3(w, h, d), "Main", buildingBodyMat);
        // Glass storefront
        CreatePrimitive(parent, new Vector3(0, h/3, d/2 + 0.5f), new Vector3(w - 2, h * 0.5f, 1), "Storefront", buildingGlassMat ?? buildingBodyMat);
        // Overhang
        CreatePrimitive(parent, new Vector3(0, h/2 + 1, d/2 + 3), new Vector3(w * 0.8f, 1, 6), "Overhang", buildingRoofMat);
        // Signage
        CreatePrimitive(parent, new Vector3(0, h + 2, 0), new Vector3(w/2, 3, 0.5f), "Signage", buildingRoofMat);
    }

    void CreatePrimitive(Transform parent, Vector3 localPos, Vector3 scale, string name, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = scale;
        if (mat != null) obj.GetComponent<Renderer>().material = mat;
    }

    void CreatePrimitiveCylinder(Transform parent, Vector3 localPos, Vector3 scale, string name)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = scale;
    }

    void AddIntersectionDetails()
    {
        // Add crosswalks at intersections
        Transform intersections = new GameObject("Intersections").transform;
        intersections.SetParent(roadsParent);
        
        float totalX = gridSizeX * (blockSize + roadWidth) + roadWidth;
        float totalZ = gridSizeZ * (blockSize + roadWidth) + roadWidth;
        float offsetX = -totalX / 2f;
        float offsetZ = -totalZ / 2f;
        
        for (int x = 0; x <= gridSizeX; x++)
        {
            for (int z = 0; z <= gridSizeZ; z++)
            {
                float xPos = offsetX + x * (blockSize + roadWidth) + roadWidth / 2f;
                float zPos = offsetZ + z * (blockSize + roadWidth) + roadWidth / 2f;
                
                // Crosswalk stripes
                CreateCrosswalk(intersections, new Vector3(xPos, roadHeight + 0.05f, zPos + roadWidth/2 - 1), true);
                CreateCrosswalk(intersections, new Vector3(xPos, roadHeight + 0.05f, zPos - roadWidth/2 + 1), true);
                CreateCrosswalk(intersections, new Vector3(xPos + roadWidth/2 - 1, roadHeight + 0.05f, zPos), false);
                CreateCrosswalk(intersections, new Vector3(xPos - roadWidth/2 + 1, roadHeight + 0.05f, zPos), false);
            }
        }
    }

    void CreateCrosswalk(Transform parent, Vector3 pos, bool horizontal)
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "CrosswalkStripe";
            stripe.transform.SetParent(parent);
            
            if (horizontal)
            {
                stripe.transform.position = pos + new Vector3(-2.5f + i * 1f, 0, 0);
                stripe.transform.localScale = new Vector3(0.5f, 0.02f, 3f);
            }
            else
            {
                stripe.transform.position = pos + new Vector3(0, 0, -2.5f + i * 1f);
                stripe.transform.localScale = new Vector3(3f, 0.02f, 0.5f);
            }
            stripe.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    void AddSidewalks(float offsetX, float offsetZ, float totalX, float totalZ)
    {
        Transform sidewalks = new GameObject("Sidewalks").transform;
        sidewalks.SetParent(roadsParent);
        
        float sidewalkWidth = 2f;
        float sidewalkHeight = 0.25f;
        
        // Around each block
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                float blockX = offsetX + roadWidth + x * (blockSize + roadWidth) + blockSize / 2f;
                float blockZ = offsetZ + roadWidth + z * (blockSize + roadWidth) + blockSize / 2f;
                
                // Four sides of sidewalk
                // North
                CreatePrimitive(sidewalks, 
                    new Vector3(blockX, sidewalkHeight/2, blockZ + blockSize/2 - sidewalkWidth/2),
                    new Vector3(blockSize, sidewalkHeight, sidewalkWidth), 
                    $"Sidewalk_N_{x}_{z}", sidewalkMaterial);
                // South
                CreatePrimitive(sidewalks, 
                    new Vector3(blockX, sidewalkHeight/2, blockZ - blockSize/2 + sidewalkWidth/2),
                    new Vector3(blockSize, sidewalkHeight, sidewalkWidth), 
                    $"Sidewalk_S_{x}_{z}", sidewalkMaterial);
                // East
                CreatePrimitive(sidewalks, 
                    new Vector3(blockX + blockSize/2 - sidewalkWidth/2, sidewalkHeight/2, blockZ),
                    new Vector3(sidewalkWidth, sidewalkHeight, blockSize - sidewalkWidth * 2), 
                    $"Sidewalk_E_{x}_{z}", sidewalkMaterial);
                // West
                CreatePrimitive(sidewalks, 
                    new Vector3(blockX - blockSize/2 + sidewalkWidth/2, sidewalkHeight/2, blockZ),
                    new Vector3(sidewalkWidth, sidewalkHeight, blockSize - sidewalkWidth * 2), 
                    $"Sidewalk_W_{x}_{z}", sidewalkMaterial);
            }
        }
    }

    [ContextMenu("Clear City")]
    public void ClearCity()
    {
        Transform existingRoads = transform.Find("_CityRoads");
        if (existingRoads != null) DestroyImmediate(existingRoads.gameObject);
        
        Transform existingBlocks = transform.Find("_CityBlocks");
        if (existingBlocks != null) DestroyImmediate(existingBlocks.gameObject);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CityLayoutManager))]
public class CityLayoutManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CityLayoutManager manager = (CityLayoutManager)target;
        
        GUILayout.Space(15);
        
        EditorGUILayout.HelpBox(
            "City Layout Generator\n\n" +
            "Creates organized city blocks with:\n" +
            "- Grid-based road system\n" +
            "- Lane markings & crosswalks\n" +
            "- Sidewalks around blocks\n" +
            "- Various building styles per zone\n\n" +
            "Block Types: Commercial, Residential, Office, Mixed", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("GENERATE CITY LAYOUT", GUILayout.Height(45)))
            manager.GenerateCityLayout();
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("Clear City", GUILayout.Height(30)))
            manager.ClearCity();
        
        GUI.backgroundColor = Color.white;
    }
}
#endif

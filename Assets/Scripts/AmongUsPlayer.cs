using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AmongUsPlayer : MonoBehaviour
{
    [Header("Colors")]
    public Color bodyColor = new Color(0.8f, 0.1f, 0.1f); // Red
    public Color visorColor = new Color(0.6f, 0.85f, 0.95f); // Light blue visor
    public Color backpackColor = new Color(0.6f, 0.08f, 0.08f); // Darker red
    public Color shadowColor = new Color(0.5f, 0.05f, 0.05f); // Even darker
    
    [Header("Size")]
    public float scale = 6.0f;
    
    private Transform modelRoot;

    void Start()
    {
        // Auto-generate if no model exists
        if (transform.Find("AmongUsModel") == null)
        {
            GenerateAmongUsModel();
        }
    }

    [ContextMenu("Generate Among Us Model")]
    public void GenerateAmongUsModel()
    {
        ClearModel();
        
        modelRoot = new GameObject("AmongUsModel").transform;
        modelRoot.SetParent(transform);
        modelRoot.localPosition = new Vector3(0, -1.0f, 0); // Lower to ground
        modelRoot.localRotation = Quaternion.identity;
        modelRoot.localScale = Vector3.one * scale;
        
        CreateBody();
        CreateVisor();
        CreateBackpack();
        CreateLegs();
        
        // Disable the original mesh renderer if exists
        MeshRenderer originalRenderer = GetComponent<MeshRenderer>();
        if (originalRenderer != null) originalRenderer.enabled = false;
        
        Debug.Log("Among Us player model generated!");
    }

    void CreateBody()
    {
        Transform body = new GameObject("Body").transform;
        body.SetParent(modelRoot);
        body.localPosition = new Vector3(0, 0.35f, 0);
        
        // Main bean-shaped body using capsule
        GameObject mainBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        mainBody.name = "MainBody";
        mainBody.transform.SetParent(body);
        mainBody.transform.localPosition = Vector3.zero;
        mainBody.transform.localScale = new Vector3(0.5f, 0.5f, 0.35f);
        ApplyMaterial(mainBody, bodyColor);
        
        // Bottom rounder part (where legs connect)
        GameObject bottomPart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bottomPart.name = "BottomPart";
        bottomPart.transform.SetParent(body);
        bottomPart.transform.localPosition = new Vector3(0, -0.3f, 0);
        bottomPart.transform.localScale = new Vector3(0.52f, 0.3f, 0.38f);
        ApplyMaterial(bottomPart, bodyColor);
        
        // Top dome (head area)
        GameObject topDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        topDome.name = "TopDome";
        topDome.transform.SetParent(body);
        topDome.transform.localPosition = new Vector3(0, 0.35f, 0);
        topDome.transform.localScale = new Vector3(0.45f, 0.25f, 0.32f);
        ApplyMaterial(topDome, bodyColor);
    }

    void CreateVisor()
    {
        Transform visor = new GameObject("Visor").transform;
        visor.SetParent(modelRoot);
        visor.localPosition = new Vector3(0, 0.55f, 0);
        
        // Visor frame (darker background)
        GameObject visorFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visorFrame.name = "VisorFrame";
        visorFrame.transform.SetParent(visor);
        visorFrame.transform.localPosition = new Vector3(0, 0f, 0.16f);
        visorFrame.transform.localScale = new Vector3(0.32f, 0.14f, 0.05f);
        ApplyMaterial(visorFrame, new Color(0.15f, 0.15f, 0.2f));
        
        // Main visor glass (light blue, not too shiny)
        GameObject visorMain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visorMain.name = "VisorGlass";
        visorMain.transform.SetParent(visor);
        visorMain.transform.localPosition = new Vector3(0, 0f, 0.18f);
        visorMain.transform.localScale = new Vector3(0.28f, 0.11f, 0.03f);
        ApplyMaterial(visorMain, visorColor, 0.3f, 0.6f);
        
        // Visor shine (small white reflection)
        GameObject visorShine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visorShine.name = "VisorShine";
        visorShine.transform.SetParent(visor);
        visorShine.transform.localPosition = new Vector3(-0.06f, 0.02f, 0.19f);
        visorShine.transform.localScale = new Vector3(0.04f, 0.02f, 0.01f);
        ApplyMaterial(visorShine, Color.white, 0.5f, 0.8f);
    }

    void CreateBackpack()
    {
        Transform backpack = new GameObject("Backpack").transform;
        backpack.SetParent(modelRoot);
        backpack.localPosition = new Vector3(0, 0.35f, -0.2f);
        
        // Main backpack body
        GameObject packMain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        packMain.name = "PackMain";
        packMain.transform.SetParent(backpack);
        packMain.transform.localPosition = Vector3.zero;
        packMain.transform.localScale = new Vector3(0.2f, 0.35f, 0.12f);
        ApplyMaterial(packMain, backpackColor);
        
        // Rounded top
        GameObject packTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        packTop.name = "PackTop";
        packTop.transform.SetParent(backpack);
        packTop.transform.localPosition = new Vector3(0, 0.16f, 0);
        packTop.transform.localScale = new Vector3(0.2f, 0.08f, 0.12f);
        ApplyMaterial(packTop, backpackColor);
        
        // Rounded bottom
        GameObject packBottom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        packBottom.name = "PackBottom";
        packBottom.transform.SetParent(backpack);
        packBottom.transform.localPosition = new Vector3(0, -0.16f, 0);
        packBottom.transform.localScale = new Vector3(0.2f, 0.08f, 0.12f);
        ApplyMaterial(packBottom, backpackColor);
    }

    void CreateLegs()
    {
        Transform legs = new GameObject("Legs").transform;
        legs.SetParent(modelRoot);
        legs.localPosition = new Vector3(0, 0, 0);
        
        // Left leg - connected to body bottom
        GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLeg.name = "LeftLeg";
        leftLeg.transform.SetParent(legs);
        leftLeg.transform.localPosition = new Vector3(-0.1f, 0.08f, 0.02f);
        leftLeg.transform.localScale = new Vector3(0.14f, 0.12f, 0.14f);
        ApplyMaterial(leftLeg, bodyColor);
        
        // Right leg - connected to body bottom  
        GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLeg.name = "RightLeg";
        rightLeg.transform.SetParent(legs);
        rightLeg.transform.localPosition = new Vector3(0.1f, 0.08f, 0.02f);
        rightLeg.transform.localScale = new Vector3(0.14f, 0.12f, 0.14f);
        ApplyMaterial(rightLeg, bodyColor);
    }

    void ApplyMaterial(GameObject obj, Color color, float metallic = 0.1f, float smoothness = 0.4f)
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
        
        // Remove collider from visual parts (keep only on parent)
        Collider col = obj.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
    }

    [ContextMenu("Clear Model")]
    public void ClearModel()
    {
        Transform existing = transform.Find("AmongUsModel");
        if (existing != null) DestroyImmediate(existing.gameObject);
        
        // Re-enable original renderer
        MeshRenderer originalRenderer = GetComponent<MeshRenderer>();
        if (originalRenderer != null) originalRenderer.enabled = true;
    }
    
    [ContextMenu("Randomize Color")]
    public void RandomizeColor()
    {
        Color[] amongUsColors = new Color[]
        {
            new Color(0.8f, 0.1f, 0.1f),   // Red
            new Color(0.1f, 0.1f, 0.8f),   // Blue
            new Color(0.1f, 0.7f, 0.1f),   // Green
            new Color(0.95f, 0.8f, 0.1f),  // Yellow
            new Color(0.9f, 0.5f, 0.1f),   // Orange
            new Color(0.7f, 0.1f, 0.7f),   // Purple
            new Color(0.1f, 0.8f, 0.8f),   // Cyan
            new Color(0.95f, 0.6f, 0.7f),  // Pink
            new Color(0.4f, 0.25f, 0.1f),  // Brown
            new Color(0.2f, 0.2f, 0.2f),   // Black
            new Color(0.9f, 0.9f, 0.9f),   // White
            new Color(0.4f, 0.8f, 0.4f),   // Lime
        };
        
        bodyColor = amongUsColors[Random.Range(0, amongUsColors.Length)];
        backpackColor = bodyColor * 0.75f;
        shadowColor = bodyColor * 0.6f;
        
        GenerateAmongUsModel();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AmongUsPlayer))]
public class AmongUsPlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        AmongUsPlayer player = (AmongUsPlayer)target;
        
        GUILayout.Space(15);
        
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("GENERATE AMONG US MODEL", GUILayout.Height(40)))
        {
            player.GenerateAmongUsModel();
        }
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.9f);
        if (GUILayout.Button("RANDOMIZE COLOR", GUILayout.Height(30)))
        {
            player.RandomizeColor();
        }
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
        if (GUILayout.Button("Clear Model", GUILayout.Height(25)))
        {
            player.ClearModel();
        }
        
        GUI.backgroundColor = Color.white;
    }
}
#endif

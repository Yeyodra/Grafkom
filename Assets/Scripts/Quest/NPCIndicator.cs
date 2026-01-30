using UnityEngine;

public class NPCIndicator : MonoBehaviour
{
    public enum IndicatorType { Exclamation, Question, None }
    
    [Header("Settings")]
    public IndicatorType currentType = IndicatorType.Exclamation;
    public float heightOffset = 2.5f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;
    public float rotateSpeed = 50f;
    
    [Header("Colors")]
    public Color exclamationColor = new Color(1f, 0.9f, 0.2f); // Yellow
    public Color questionColor = new Color(0.2f, 0.8f, 1f); // Cyan
    
    private GameObject indicatorObject;
    private Transform mainCamera;
    private Vector3 basePosition;
    private MeshRenderer meshRenderer;
    private Material indicatorMaterial;
    
    void Start()
    {
        mainCamera = Camera.main?.transform;
        CreateIndicator();
        UpdateIndicatorType(currentType);
    }
    
    void CreateIndicator()
    {
        // Create parent for indicator
        indicatorObject = new GameObject("Indicator");
        indicatorObject.transform.SetParent(transform);
        indicatorObject.transform.localPosition = new Vector3(0, heightOffset, 0);
        basePosition = indicatorObject.transform.localPosition;
        
        // Create the "!" or "?" shape using primitives
        CreateExclamationMark();
    }
    
    void CreateExclamationMark()
    {
        // Main body (vertical bar)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "IndicatorBody";
        body.transform.SetParent(indicatorObject.transform);
        body.transform.localPosition = new Vector3(0, 0.15f, 0);
        body.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
        
        // Remove collider
        Collider col = body.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        meshRenderer = body.GetComponent<MeshRenderer>();
        indicatorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (indicatorMaterial.shader == null)
            indicatorMaterial = new Material(Shader.Find("Standard"));
        indicatorMaterial.SetColor("_BaseColor", exclamationColor);
        indicatorMaterial.color = exclamationColor;
        indicatorMaterial.SetFloat("_Smoothness", 0.8f);
        meshRenderer.material = indicatorMaterial;
        
        // Dot at bottom
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = "IndicatorDot";
        dot.transform.SetParent(indicatorObject.transform);
        dot.transform.localPosition = new Vector3(0, -0.15f, 0);
        dot.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        
        // Remove collider
        Collider dotCol = dot.GetComponent<Collider>();
        if (dotCol != null) Destroy(dotCol);
        
        MeshRenderer dotRenderer = dot.GetComponent<MeshRenderer>();
        dotRenderer.material = indicatorMaterial;
    }
    
    void Update()
    {
        if (indicatorObject == null) return;
        
        // Billboard - face camera
        if (mainCamera != null)
        {
            Vector3 lookDir = mainCamera.position - indicatorObject.transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                indicatorObject.transform.rotation = Quaternion.LookRotation(-lookDir);
            }
        }
        
        // Bob up and down
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        indicatorObject.transform.localPosition = basePosition + new Vector3(0, bob, 0);
    }
    
    public void UpdateIndicatorType(IndicatorType type)
    {
        currentType = type;
        
        if (indicatorObject == null) return;
        
        switch (type)
        {
            case IndicatorType.Exclamation:
                indicatorObject.SetActive(true);
                SetColor(exclamationColor);
                // Keep as "!" shape (vertical bar + dot)
                Transform body = indicatorObject.transform.Find("IndicatorBody");
                if (body != null)
                {
                    body.localPosition = new Vector3(0, 0.15f, 0);
                    body.localScale = new Vector3(0.15f, 0.4f, 0.15f);
                }
                break;
                
            case IndicatorType.Question:
                indicatorObject.SetActive(true);
                SetColor(questionColor);
                // Modify to look more like "?" (curved top)
                Transform qBody = indicatorObject.transform.Find("IndicatorBody");
                if (qBody != null)
                {
                    qBody.localPosition = new Vector3(0, 0.2f, 0);
                    qBody.localScale = new Vector3(0.15f, 0.3f, 0.15f);
                    qBody.localRotation = Quaternion.Euler(0, 0, 15f);
                }
                break;
                
            case IndicatorType.None:
                indicatorObject.SetActive(false);
                break;
        }
    }
    
    void SetColor(Color color)
    {
        if (indicatorMaterial != null)
        {
            indicatorMaterial.SetColor("_BaseColor", color);
            indicatorMaterial.color = color;
        }
    }
    
    public void Show()
    {
        if (indicatorObject != null)
            indicatorObject.SetActive(true);
    }
    
    public void Hide()
    {
        if (indicatorObject != null)
            indicatorObject.SetActive(false);
    }
}

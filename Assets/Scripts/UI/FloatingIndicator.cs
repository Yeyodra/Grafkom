using UnityEngine;

public class FloatingIndicator : MonoBehaviour
{
    [Header("Settings")]
    public float hoverSpeed = 2f;
    public float hoverHeight = 0.3f;
    public Color indicatorColor = Color.yellow;
    public float indicatorSize = 0.5f;
    
    [Header("Icon Type")]
    public IndicatorType type = IndicatorType.Trash;
    
    public enum IndicatorType
    {
        Trash,
        Exclamation,
        Question
    }
    
    private Vector3 startLocalPos;
    private GameObject visualObject;
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        startLocalPos = transform.localPosition;
        CreateVisual();
    }
    
    void CreateVisual()
    {
        // Create visual indicator based on type
        switch (type)
        {
            case IndicatorType.Trash:
                CreateTrashIcon();
                break;
            case IndicatorType.Exclamation:
                CreateExclamationIcon();
                break;
            case IndicatorType.Question:
                CreateQuestionIcon();
                break;
        }
    }
    
    void CreateTrashIcon()
    {
        // Simple trash icon - kotak dengan tutup
        visualObject = new GameObject("TrashIcon");
        visualObject.transform.SetParent(transform);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localScale = Vector3.one * indicatorSize;
        
        // Body (cube)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(visualObject.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        
        // Remove collider
        Destroy(body.GetComponent<Collider>());
        
        // Set color
        meshRenderer = body.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        meshRenderer.material.color = indicatorColor;
        
        // Lid (flat cube on top)
        GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lid.transform.SetParent(visualObject.transform);
        lid.transform.localPosition = new Vector3(0, 0.6f, 0);
        lid.transform.localScale = new Vector3(1f, 0.2f, 1f);
        
        Destroy(lid.GetComponent<Collider>());
        lid.GetComponent<MeshRenderer>().material = meshRenderer.material;
    }
    
    void CreateExclamationIcon()
    {
        // Exclamation mark - capsule + sphere
        visualObject = new GameObject("ExclamationIcon");
        visualObject.transform.SetParent(transform);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localScale = Vector3.one * indicatorSize;
        
        // Line (capsule)
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        line.transform.SetParent(visualObject.transform);
        line.transform.localPosition = new Vector3(0, 0.3f, 0);
        line.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        
        Destroy(line.GetComponent<Collider>());
        meshRenderer = line.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        meshRenderer.material.color = indicatorColor;
        
        // Dot (sphere)
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.transform.SetParent(visualObject.transform);
        dot.transform.localPosition = new Vector3(0, -0.4f, 0);
        dot.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        Destroy(dot.GetComponent<Collider>());
        dot.GetComponent<MeshRenderer>().material = meshRenderer.material;
    }
    
    void CreateQuestionIcon()
    {
        // Question mark - simplified dengan sphere
        visualObject = new GameObject("QuestionIcon");
        visualObject.transform.SetParent(transform);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localScale = Vector3.one * indicatorSize;
        
        // Hook part (sphere)
        GameObject hook = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hook.transform.SetParent(visualObject.transform);
        hook.transform.localPosition = new Vector3(0, 0.2f, 0);
        hook.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        Destroy(hook.GetComponent<Collider>());
        meshRenderer = hook.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        meshRenderer.material.color = indicatorColor;
        
        // Dot (sphere)
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.transform.SetParent(visualObject.transform);
        dot.transform.localPosition = new Vector3(0, -0.4f, 0);
        dot.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        
        Destroy(dot.GetComponent<Collider>());
        dot.GetComponent<MeshRenderer>().material = meshRenderer.material;
    }
    
    void Update()
    {
        // Billboard - always face camera
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // Flip karena LookAt face away
        }
        
        // Hover animation (naik turun)
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.localPosition = startLocalPos + new Vector3(0, hoverOffset, 0);
    }
    
    public void SetColor(Color color)
    {
        indicatorColor = color;
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }
}

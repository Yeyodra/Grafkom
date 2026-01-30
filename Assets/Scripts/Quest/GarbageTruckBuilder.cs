using UnityEngine;

public class GarbageTruckBuilder : MonoBehaviour
{
    [Header("Colors")]
    public Color cabinColor = new Color(0.1f, 0.4f, 0.1f); // Hijau tua
    public Color bakColor = new Color(0.2f, 0.6f, 0.2f);   // Hijau muda
    public Color wheelColor = new Color(0.15f, 0.15f, 0.15f); // Hitam
    
    [ContextMenu("Build Garbage Truck")]
    public void BuildTruck()
    {
        // Clear existing children
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        // === CABIN ===
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(transform);
        cabin.transform.localPosition = new Vector3(-1.8f, 1f, 0);
        cabin.transform.localScale = new Vector3(2f, 2f, 2.5f);
        SetColor(cabin, cabinColor);
        
        // Cabin window (darker)
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
        window.name = "Window";
        window.transform.SetParent(cabin.transform);
        window.transform.localPosition = new Vector3(0.35f, 0.2f, 0);
        window.transform.localScale = new Vector3(0.3f, 0.4f, 0.9f);
        SetColor(window, new Color(0.2f, 0.3f, 0.4f)); // Biru gelap
        Destroy(window.GetComponent<Collider>());
        
        // === BAK SAMPAH ===
        GameObject bak = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bak.name = "Bak";
        bak.transform.SetParent(transform);
        bak.transform.localPosition = new Vector3(1.2f, 0.8f, 0);
        bak.transform.localScale = new Vector3(4f, 1.8f, 2.5f);
        SetColor(bak, bakColor);
        
        // Bak rim (top edge)
        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rim.name = "BakRim";
        rim.transform.SetParent(bak.transform);
        rim.transform.localPosition = new Vector3(0, 0.55f, 0);
        rim.transform.localScale = new Vector3(1.05f, 0.1f, 1.05f);
        SetColor(rim, cabinColor);
        Destroy(rim.GetComponent<Collider>());
        
        // === WHEELS ===
        CreateWheel("Wheel_FL", new Vector3(-2f, -0.3f, 1.1f));
        CreateWheel("Wheel_FR", new Vector3(-2f, -0.3f, -1.1f));
        CreateWheel("Wheel_BL", new Vector3(1.5f, -0.3f, 1.1f));
        CreateWheel("Wheel_BR", new Vector3(1.5f, -0.3f, -1.1f));
        
        // === CHASSIS (bottom) ===
        GameObject chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chassis.name = "Chassis";
        chassis.transform.SetParent(transform);
        chassis.transform.localPosition = new Vector3(0, 0.1f, 0);
        chassis.transform.localScale = new Vector3(6f, 0.3f, 2f);
        SetColor(chassis, new Color(0.2f, 0.2f, 0.2f));
        
        // === DROP ZONE (invisible trigger) ===
        GameObject dropZone = new GameObject("DropZone");
        dropZone.transform.SetParent(transform);
        dropZone.transform.localPosition = new Vector3(1.2f, 1.5f, 0);
        
        BoxCollider dropCollider = dropZone.AddComponent<BoxCollider>();
        dropCollider.isTrigger = true;
        dropCollider.size = new Vector3(5f, 3f, 4f);
        
        Debug.Log("Garbage Truck built successfully!");
    }
    
    void CreateWheel(string name, Vector3 position)
    {
        GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = name;
        wheel.transform.SetParent(transform);
        wheel.transform.localPosition = position;
        wheel.transform.localRotation = Quaternion.Euler(90, 0, 0);
        wheel.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
        SetColor(wheel, wheelColor);
        Destroy(wheel.GetComponent<Collider>());
    }
    
    void SetColor(GameObject obj, Color color)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            renderer.material = mat;
        }
    }
    
    void Start()
    {
        // Auto-build jika belum ada Cabin (truck belum di-build)
        if (transform.Find("Cabin") == null)
        {
            BuildTruck();
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class TrashSpawner : MonoBehaviour
{
    public static TrashSpawner Instance { get; private set; }
    
    [Header("Spawn Settings")]
    public Transform spawnPointsParent;
    public int trashToSpawn = 5;
    
    [Header("Trash Prefab Settings")]
    public Color trashColor = new Color(0.4f, 0.3f, 0.2f); // Coklat
    public float trashScale = 0.5f;
    
    private List<Transform> allSpawnPoints = new List<Transform>();
    private List<GameObject> spawnedTrash = new List<GameObject>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Auto-find spawn points jika belum di-assign
        if (spawnPointsParent == null)
        {
            spawnPointsParent = transform.Find("TrashSpawnPoints");
        }
        
        if (spawnPointsParent != null)
        {
            CollectSpawnPoints();
        }
    }
    
    void CollectSpawnPoints()
    {
        allSpawnPoints.Clear();
        foreach (Transform child in spawnPointsParent)
        {
            allSpawnPoints.Add(child);
        }
        Debug.Log($"TrashSpawner: Found {allSpawnPoints.Count} spawn points");
    }
    
    public void SpawnTrash()
    {
        if (allSpawnPoints.Count == 0)
        {
            Debug.LogWarning("Tidak ada spawn points!");
            return;
        }
        
        // Clear existing trash
        ClearTrash();
        
        // Shuffle dan pilih random spawn points
        List<Transform> shuffled = new List<Transform>(allSpawnPoints);
        ShuffleList(shuffled);
        
        // Spawn trash di titik yang dipilih
        int spawnCount = Mathf.Min(trashToSpawn, shuffled.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject trash = CreateTrashObject(shuffled[i].position);
            spawnedTrash.Add(trash);
        }
        
        Debug.Log($"Spawned {spawnCount} trash objects");
    }
    
    GameObject CreateTrashObject(Vector3 position)
    {
        // Create trash container
        GameObject trash = new GameObject("Trash");
        trash.transform.position = position;
        trash.tag = "Untagged";
        trash.layer = 0;
        
        // Create visual (cube sebagai trash bag)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "TrashVisual";
        visual.transform.SetParent(trash.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * trashScale;
        visual.transform.localRotation = Quaternion.Euler(
            Random.Range(-15f, 15f),
            Random.Range(0f, 360f),
            Random.Range(-15f, 15f)
        );
        
        // Set material
        MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = trashColor;
            renderer.material = mat;
        }
        
        // Remove visual collider (kita pakai sphere collider di parent)
        Destroy(visual.GetComponent<Collider>());
        
        // Add TrashCollectible component
        TrashCollectible collectible = trash.AddComponent<TrashCollectible>();
        collectible.questId = "collect_trash";
        
        // Add floating indicator
        GameObject indicator = new GameObject("FloatingIndicator");
        indicator.transform.SetParent(trash.transform);
        indicator.transform.localPosition = new Vector3(0, 1.5f, 0);
        FloatingIndicator floatingInd = indicator.AddComponent<FloatingIndicator>();
        floatingInd.type = FloatingIndicator.IndicatorType.Trash;
        floatingInd.indicatorColor = Color.yellow;
        
        collectible.floatingIndicator = indicator;
        
        return trash;
    }
    
    public void ClearTrash()
    {
        foreach (GameObject trash in spawnedTrash)
        {
            if (trash != null)
            {
                Destroy(trash);
            }
        }
        spawnedTrash.Clear();
    }
    
    public int GetRemainingTrashCount()
    {
        // Count non-null trash objects
        int count = 0;
        foreach (GameObject trash in spawnedTrash)
        {
            if (trash != null)
            {
                count++;
            }
        }
        return count;
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}

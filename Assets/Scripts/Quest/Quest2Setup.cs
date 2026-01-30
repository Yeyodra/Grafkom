using UnityEngine;

public class Quest2Setup : MonoBehaviour
{
    [Header("References")]
    public GameObject npcPetugasKebersihan;
    public GameObject garbageTruck;
    public TrashSpawner trashSpawner;
    
    [Header("NPC Color")]
    public Color npcColor = new Color(0.2f, 0.6f, 0.2f); // Hijau
    
    [Header("Truck Position")]
    public Vector3 truckOffset = new Vector3(3f, 0, 0); // Offset dari NPC
    
    void Start()
    {
        SetupNPCColor();
        SetupGarbageTruck();
    }
    
    [ContextMenu("Setup Quest 2")]
    public void SetupQuest2()
    {
        SetupNPCColor();
        SetupGarbageTruck();
    }
    
    void SetupNPCColor()
    {
        if (npcPetugasKebersihan == null)
        {
            npcPetugasKebersihan = GameObject.Find("NPC_PetugasKebersihan");
        }
        
        if (npcPetugasKebersihan != null)
        {
            // Set warna via AmongUsPlayer
            AmongUsPlayer amongUs = npcPetugasKebersihan.GetComponent<AmongUsPlayer>();
            if (amongUs != null)
            {
                amongUs.bodyColor = npcColor;
                amongUs.ApplyColor(); // Apply warna
                Debug.Log("NPC Petugas Kebersihan warna set ke hijau");
            }
            
            // Fallback: set MeshRenderer color langsung
            MeshRenderer renderer = npcPetugasKebersihan.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = npcColor;
            }
        }
        else
        {
            Debug.LogWarning("NPC_PetugasKebersihan tidak ditemukan!");
        }
    }
    
    void SetupGarbageTruck()
    {
        if (garbageTruck == null)
        {
            garbageTruck = GameObject.Find("GarbageTruck");
        }
        
        // Jika truck belum ada, buat baru
        if (garbageTruck == null && npcPetugasKebersihan != null)
        {
            garbageTruck = new GameObject("GarbageTruck");
            garbageTruck.transform.position = npcPetugasKebersihan.transform.position + truckOffset;
            
            // Add builder dan truck components
            GarbageTruckBuilder builder = garbageTruck.AddComponent<GarbageTruckBuilder>();
            GarbageTruck truck = garbageTruck.AddComponent<GarbageTruck>();
            
            Debug.Log("Garbage Truck dibuat di samping NPC");
        }
    }
    
    public void StartQuest2()
    {
        // Show inventory UI
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.Show();
        }
        
        // Spawn trash
        if (trashSpawner == null)
        {
            trashSpawner = FindObjectOfType<TrashSpawner>();
        }
        
        if (trashSpawner != null)
        {
            trashSpawner.SpawnTrash();
        }
        
        Debug.Log("Quest 2 dimulai!");
    }
}

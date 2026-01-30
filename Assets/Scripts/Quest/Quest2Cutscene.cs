using UnityEngine;
using System.Collections;

public class Quest2Cutscene : MonoBehaviour
{
    [Header("References")]
    public Transform npcPetugasKebersihan;
    public GarbageTruck garbageTruck;
    
    [Header("Camera Settings")]
    public float zoomAmount = 10f;
    public float zoomDuration = 0.8f;
    
    [Header("NPC Recovery")]
    public Color sickColor = new Color(0.6f, 0.8f, 0.6f);    // Hijau pucat
    public Color healthyColor = new Color(0.2f, 0.9f, 0.3f); // Hijau cerah
    public float colorTransitionDuration = 1.5f;
    
    [Header("Timing")]
    public float delayBeforeDialog = 0.5f;
    public float delayAfterDialog = 1f;
    public float delayBeforeDriveAway = 1f;
    
    private Camera mainCamera;
    private float originalFOV;
    private bool isPlaying = false;
    
    // Ending dialog lines
    private string[] endingDialogLines = new string[]
    {
        "Wah... kamu hebat sekali!",
        "Terima kasih banyak! Kota jadi bersih berkat kamu.",
        "Sepertinya saya sudah agak baikan... Saya bisa lanjutkan dari sini!"
    };
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
            originalFOV = mainCamera.fieldOfView;
        
        // Auto-find references jika belum di-assign
        if (npcPetugasKebersihan == null)
        {
            GameObject npc = GameObject.Find("NPC_PetugasKebersihan");
            if (npc != null)
                npcPetugasKebersihan = npc.transform;
        }
        
        if (garbageTruck == null)
        {
            garbageTruck = FindFirstObjectByType<GarbageTruck>();
        }
    }
    
    public void PlayEndingCutscene()
    {
        if (isPlaying) return;
        StartCoroutine(EndingSequence());
    }
    
    IEnumerator EndingSequence()
    {
        isPlaying = true;
        Debug.Log("Quest2 Ending Cutscene started");
        
        // Disable player movement
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.enabled = false;
        
        // Hide inventory UI
        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
            inventoryUI.Hide();
        
        // 1. Camera zoom in ke NPC
        yield return StartCoroutine(CameraZoomIn());
        
        yield return new WaitForSeconds(delayBeforeDialog);
        
        // 2. Play ending dialog
        bool dialogComplete = false;
        if (DialogueManager.Instance != null && npcPetugasKebersihan != null)
        {
            DialogueManager.Instance.StartDialogue(
                "Petugas Kebersihan",
                endingDialogLines,
                new Color(0.2f, 0.8f, 0.3f), // Hijau
                () => { dialogComplete = true; }
            );
            
            // Wait for dialog to complete
            while (!dialogComplete)
            {
                yield return null;
            }
        }
        
        yield return new WaitForSeconds(delayAfterDialog);
        
        // 3. NPC color transition (sembuh)
        yield return StartCoroutine(NPCRecoveryEffect());
        
        // 4. NPC bounce (happy)
        yield return StartCoroutine(HappyBounce());
        
        yield return new WaitForSeconds(delayBeforeDriveAway);
        
        // 5. Truck drive away
        if (garbageTruck != null)
        {
            garbageTruck.DriveAway();
            Debug.Log("Garbage truck driving away");
        }
        
        // Wait a bit for truck to start moving
        yield return new WaitForSeconds(1.5f);
        
        // 6. Camera zoom out
        yield return StartCoroutine(CameraZoomOut());
        
        // 7. Re-enable player
        if (player != null)
            player.enabled = true;
        
        isPlaying = false;
        Debug.Log("Quest2 Ending Cutscene completed");
        
        // Quest completion is handled by QuestManager
    }
    
    IEnumerator CameraZoomIn()
    {
        if (mainCamera == null) yield break;
        
        float elapsed = 0;
        float startFOV = mainCamera.fieldOfView;
        float targetFOV = startFOV - zoomAmount;
        
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }
        
        mainCamera.fieldOfView = targetFOV;
    }
    
    IEnumerator CameraZoomOut()
    {
        if (mainCamera == null) yield break;
        
        float elapsed = 0;
        float startFOV = mainCamera.fieldOfView;
        
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
            yield return null;
        }
        
        mainCamera.fieldOfView = originalFOV;
    }
    
    IEnumerator NPCRecoveryEffect()
    {
        if (npcPetugasKebersihan == null) yield break;
        
        // Find the body renderer (Among Us style)
        Renderer bodyRenderer = null;
        Transform body = npcPetugasKebersihan.Find("Body");
        if (body != null)
        {
            bodyRenderer = body.GetComponent<Renderer>();
        }
        else
        {
            // Try to find any renderer in children
            bodyRenderer = npcPetugasKebersihan.GetComponentInChildren<Renderer>();
        }
        
        if (bodyRenderer == null) yield break;
        
        // Create sparkle effect
        StartCoroutine(SpawnRecoverySparkles());
        
        // Transition color from sick to healthy
        float elapsed = 0;
        Material mat = bodyRenderer.material;
        Color startColor = mat.color;
        
        while (elapsed < colorTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / colorTransitionDuration);
            
            Color newColor = Color.Lerp(startColor, healthyColor, t);
            mat.color = newColor;
            
            // Also set _BaseColor for URP
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", newColor);
            
            yield return null;
        }
        
        mat.color = healthyColor;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", healthyColor);
    }
    
    IEnumerator SpawnRecoverySparkles()
    {
        if (npcPetugasKebersihan == null) yield break;
        
        Vector3 center = npcPetugasKebersihan.position + Vector3.up * 0.5f;
        int sparkleCount = 15;
        
        for (int i = 0; i < sparkleCount; i++)
        {
            CreateSparkle(center);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    void CreateSparkle(Vector3 center)
    {
        GameObject sparkle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sparkle.name = "RecoverySparkle";
        sparkle.transform.position = center + Random.insideUnitSphere * 0.5f;
        sparkle.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
        
        // Remove collider
        Collider col = sparkle.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        // Set glowing green material
        Renderer rend = sparkle.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            mat = new Material(Shader.Find("Standard"));
        
        Color sparkleColor = new Color(0.5f, 1f, 0.5f); // Light green
        mat.color = sparkleColor;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", sparkleColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", sparkleColor * 2f);
        rend.material = mat;
        
        // Animate and destroy
        StartCoroutine(AnimateSparkle(sparkle));
    }
    
    IEnumerator AnimateSparkle(GameObject sparkle)
    {
        float duration = Random.Range(0.8f, 1.2f);
        float elapsed = 0;
        Vector3 startPos = sparkle.transform.position;
        Vector3 endPos = startPos + Vector3.up * Random.Range(0.8f, 1.5f);
        Vector3 startScale = sparkle.transform.localScale;
        
        while (elapsed < duration && sparkle != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            sparkle.transform.position = Vector3.Lerp(startPos, endPos, t);
            sparkle.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            yield return null;
        }
        
        if (sparkle != null)
            Destroy(sparkle);
    }
    
    IEnumerator HappyBounce()
    {
        if (npcPetugasKebersihan == null) yield break;
        
        // Face the player
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform != null)
        {
            Vector3 dir = (playerTransform.position - npcPetugasKebersihan.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                npcPetugasKebersihan.rotation = Quaternion.LookRotation(dir);
        }
        
        // Bounce 3 times
        Vector3 startPos = npcPetugasKebersihan.position;
        float bounceHeight = 0.4f;
        float bounceDuration = 0.3f;
        
        for (int i = 0; i < 3; i++)
        {
            float elapsed = 0;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bounceDuration;
                float bounce = Mathf.Sin(t * Mathf.PI) * bounceHeight;
                npcPetugasKebersihan.position = startPos + Vector3.up * bounce;
                yield return null;
            }
        }
        
        npcPetugasKebersihan.position = startPos;
    }
    
    public bool IsPlaying => isPlaying;
}

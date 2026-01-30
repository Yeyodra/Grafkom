using UnityEngine;
using System.Collections;

public class QuestCutscene : MonoBehaviour
{
    [Header("NPC References")]
    public Transform strangerHelper; // Yang minta tolong (kuning)
    public Transform strangerSick;   // Yang pingsan (hijau)
    
    [Header("Animation Settings")]
    public float walkSpeed = 2f;
    public float bendDuration = 0.5f;
    public float healDuration = 1.5f;
    public float standUpDuration = 1f;
    public float thankYouDuration = 2f;
    
    [Header("Camera")]
    public Camera mainCamera;
    public float zoomAmount = 2f;
    public float zoomDuration = 0.5f;
    
    [Header("Effects")]
    public Color particleColor = new Color(1f, 0.8f, 0.2f); // Golden sparkle
    public int particleCount = 20;
    
    [Header("Audio")]
    public AudioClip healSound;
    public AudioClip successSound;
    
    private AudioSource audioSource;
    private Vector3 originalCameraPos;
    private float originalCameraFOV;
    private bool isPlaying = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    
    public void PlayCutscene(System.Action onComplete = null)
    {
        if (isPlaying) return;
        StartCoroutine(CutsceneSequence(onComplete));
    }
    
    IEnumerator CutsceneSequence(System.Action onComplete)
    {
        isPlaying = true;
        
        // Store original camera values
        if (mainCamera != null)
        {
            originalCameraPos = mainCamera.transform.position;
            originalCameraFOV = mainCamera.fieldOfView;
        }
        
        // Disable player movement
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.enabled = false;
        
        // 1. Camera zoom in
        yield return StartCoroutine(CameraZoomIn());
        
        // 2. Stranger helper walks to sick stranger
        if (strangerHelper != null && strangerSick != null)
        {
            yield return StartCoroutine(WalkToTarget(strangerHelper, strangerSick.position + Vector3.right * 0.5f));
        }
        
        // 3. Bend down (scale Y)
        yield return StartCoroutine(BendDown(strangerHelper));
        
        // 4. Healing effect + sound
        yield return StartCoroutine(HealingEffect());
        
        // 5. Sick stranger stands up
        yield return StartCoroutine(StandUp(strangerSick));
        
        // 6. Both face player and bounce (thank you)
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        yield return StartCoroutine(ThankYouAnimation(playerTransform));
        
        // 7. Play success sound
        if (successSound != null && audioSource != null)
            audioSource.PlayOneShot(successSound);
        
        // 8. Camera zoom out
        yield return StartCoroutine(CameraZoomOut());
        
        // Re-enable player
        if (player != null)
            player.enabled = true;
        
        isPlaying = false;
        
        // Complete quest
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnDeliverItem("delivery_medicine");
        }
        
        onComplete?.Invoke();
    }
    
    IEnumerator CameraZoomIn()
    {
        if (mainCamera == null) yield break;
        
        float elapsed = 0;
        float startFOV = mainCamera.fieldOfView;
        float targetFOV = startFOV - zoomAmount * 5f;
        
        // Calculate target position (look at scene center)
        Vector3 sceneCenter = Vector3.zero;
        if (strangerHelper != null && strangerSick != null)
        {
            sceneCenter = (strangerHelper.position + strangerSick.position) / 2f;
        }
        
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            t = Mathf.SmoothStep(0, 1, t);
            
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            
            yield return null;
        }
    }
    
    IEnumerator CameraZoomOut()
    {
        if (mainCamera == null) yield break;
        
        float elapsed = 0;
        float startFOV = mainCamera.fieldOfView;
        
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            t = Mathf.SmoothStep(0, 1, t);
            
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, originalCameraFOV, t);
            
            yield return null;
        }
        
        mainCamera.fieldOfView = originalCameraFOV;
    }
    
    IEnumerator WalkToTarget(Transform npc, Vector3 targetPos)
    {
        if (npc == null) yield break;
        
        Vector3 startPos = npc.position;
        targetPos.y = startPos.y; // Keep same Y
        
        // Face target
        Vector3 dir = (targetPos - startPos).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            npc.rotation = targetRot;
        }
        
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / walkSpeed;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Walk with slight bounce
            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI * 4) * 0.05f; // Bounce while walking
            npc.position = pos;
            
            yield return null;
        }
        
        npc.position = targetPos;
    }
    
    IEnumerator BendDown(Transform npc)
    {
        if (npc == null) yield break;
        
        // Face the sick stranger
        if (strangerSick != null)
        {
            Vector3 dir = (strangerSick.position - npc.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                npc.rotation = Quaternion.LookRotation(dir);
        }
        
        Vector3 startScale = npc.localScale;
        Vector3 bentScale = new Vector3(startScale.x, startScale.y * 0.7f, startScale.z);
        
        float elapsed = 0;
        while (elapsed < bendDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bendDuration;
            npc.localScale = Vector3.Lerp(startScale, bentScale, t);
            yield return null;
        }
        
        // Wait a moment
        yield return new WaitForSeconds(0.3f);
        
        // Stand back up
        elapsed = 0;
        while (elapsed < bendDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bendDuration;
            npc.localScale = Vector3.Lerp(bentScale, startScale, t);
            yield return null;
        }
        
        npc.localScale = startScale;
    }
    
    IEnumerator HealingEffect()
    {
        // Play heal sound
        if (healSound != null && audioSource != null)
            audioSource.PlayOneShot(healSound);
        
        // Create particle burst
        if (strangerSick != null)
        {
            Vector3 effectPos = strangerSick.position + Vector3.up * 0.5f;
            
            for (int i = 0; i < particleCount; i++)
            {
                CreateSparkle(effectPos);
            }
        }
        
        yield return new WaitForSeconds(healDuration);
    }
    
    void CreateSparkle(Vector3 center)
    {
        GameObject sparkle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sparkle.name = "Sparkle";
        sparkle.transform.position = center + Random.insideUnitSphere * 0.5f;
        sparkle.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);
        
        // Remove collider
        Collider col = sparkle.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        // Set material
        Renderer rend = sparkle.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
            mat = new Material(Shader.Find("Standard"));
        mat.SetColor("_BaseColor", particleColor);
        mat.color = particleColor;
        mat.SetFloat("_Smoothness", 1f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", particleColor * 2f);
        rend.material = mat;
        
        // Animate and destroy
        StartCoroutine(AnimateSparkle(sparkle));
    }
    
    IEnumerator AnimateSparkle(GameObject sparkle)
    {
        float duration = Random.Range(0.8f, 1.5f);
        float elapsed = 0;
        Vector3 startPos = sparkle.transform.position;
        Vector3 endPos = startPos + Vector3.up * Random.Range(0.5f, 1.5f) + Random.insideUnitSphere * 0.3f;
        Vector3 startScale = sparkle.transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            sparkle.transform.position = Vector3.Lerp(startPos, endPos, t);
            sparkle.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            yield return null;
        }
        
        Destroy(sparkle);
    }
    
    IEnumerator StandUp(Transform npc)
    {
        if (npc == null) yield break;
        
        // Currently lying down (rotated Z = 90)
        Quaternion startRot = npc.localRotation;
        Quaternion endRot = Quaternion.identity;
        
        Vector3 startPos = npc.position;
        Vector3 endPos = startPos + Vector3.up * 0.3f; // Lift up slightly
        
        float elapsed = 0;
        while (elapsed < standUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / standUpDuration;
            t = Mathf.SmoothStep(0, 1, t);
            
            npc.localRotation = Quaternion.Slerp(startRot, endRot, t);
            npc.position = Vector3.Lerp(startPos, endPos, t);
            
            yield return null;
        }
        
        npc.localRotation = endRot;
        npc.position = endPos;
        
        // Pop scale effect
        Vector3 normalScale = npc.localScale;
        npc.localScale = normalScale * 1.2f;
        
        elapsed = 0;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.2f;
            npc.localScale = Vector3.Lerp(normalScale * 1.2f, normalScale, t);
            yield return null;
        }
        
        npc.localScale = normalScale;
    }
    
    IEnumerator ThankYouAnimation(Transform player)
    {
        // Both NPCs face player
        if (player != null)
        {
            if (strangerHelper != null)
            {
                Vector3 dir = (player.position - strangerHelper.position).normalized;
                dir.y = 0;
                if (dir != Vector3.zero)
                    strangerHelper.rotation = Quaternion.LookRotation(dir);
            }
            
            if (strangerSick != null)
            {
                Vector3 dir = (player.position - strangerSick.position).normalized;
                dir.y = 0;
                if (dir != Vector3.zero)
                    strangerSick.rotation = Quaternion.LookRotation(dir);
            }
        }
        
        // Bounce animation (jumping joy)
        float elapsed = 0;
        int bounceCount = 3;
        float bounceHeight = 0.3f;
        float bounceDuration = thankYouDuration / bounceCount;
        
        Vector3 helperStartPos = strangerHelper != null ? strangerHelper.position : Vector3.zero;
        Vector3 sickStartPos = strangerSick != null ? strangerSick.position : Vector3.zero;
        
        for (int i = 0; i < bounceCount; i++)
        {
            elapsed = 0;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bounceDuration;
                float bounce = Mathf.Sin(t * Mathf.PI) * bounceHeight;
                
                if (strangerHelper != null)
                    strangerHelper.position = helperStartPos + Vector3.up * bounce;
                    
                if (strangerSick != null)
                    strangerSick.position = sickStartPos + Vector3.up * bounce;
                
                yield return null;
            }
        }
        
        // Reset positions
        if (strangerHelper != null)
            strangerHelper.position = helperStartPos;
        if (strangerSick != null)
            strangerSick.position = sickStartPos;
    }
    
    public bool IsPlaying => isPlaying;
}

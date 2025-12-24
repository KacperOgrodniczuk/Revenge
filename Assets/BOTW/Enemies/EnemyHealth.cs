using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Settings")]
    public Canvas worldSpaceCanvas;
    public Slider healthBarSlider;
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);

    [Header("Knockback Settings")]
    public float knockbackRecoveryTime = 0.5f;  
    private Rigidbody rb;
    private NavMeshAgent agent;
    private EnemyController EnCont;
    private bool isKnockedBack = false;
    private float knockbackTimer;

    [Header("Visual Feedback")]
    public Renderer[] renderers; 
    private Color[] originalColors;
    private bool isFlashing = false;

    private Camera mainCamera;


    void Start()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;
        EnCont = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }

        //Find all Renderers connected
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        // keeps track of original Mats
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                originalColors[i] = renderers[i].material.color;
        }
    }

    void Update()
    {
      
        if (worldSpaceCanvas != null && mainCamera != null)
        {
            worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(worldSpaceCanvas.transform.position - mainCamera.transform.position);
            worldSpaceCanvas.transform.position = transform.position + healthBarOffset;
        }

       
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                EnCont.enabled = true;
                rb.isKinematic = true;
                if (agent != null) agent.enabled = true;
                
            }
        }
    }

 //DMG Section
    public void TakeDamage(float amount, Vector3 hitDirection, float knockbackForce)
    {
        
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

   
        if (rb != null)
        {
           
            if (agent != null) 
            
            agent.enabled = false;
            rb.isKinematic = false;            
            EnCont.enabled = false;
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode.Impulse);
            isKnockedBack = true;
            knockbackTimer = knockbackRecoveryTime;
        }

        FlashRed();

        if (currentHealth <= 0f)
            Die();
    }


    void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        Destroy(gameObject);
    }



    //Visual Feedback Section
    void FlashRed()
    {
        if (!isFlashing)
            StartCoroutine(FlashRoutine());
    }

   IEnumerator FlashRoutine()
    {
        isFlashing = true;

       
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                renderers[i].material.color = Color.red;
        }

       
        yield return new WaitForSeconds(0.5f);

      
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                renderers[i].material.color = originalColors[i];
        }

        isFlashing = false;
    }
}

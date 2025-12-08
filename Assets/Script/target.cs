using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class target : MonoBehaviour
{
    [Header("Shrink Settings")]
    public float shrinkDuration = 2f; // Durée de l'animation de rétrécissement
    
    [Header("Collision Settings")]
    public bool useTrigger = false; // Utiliser un trigger au lieu d'une collision physique
    
    [Header("Respawn Settings")]
    public Vector3 respawnAreaMin = new Vector3(-10f, 0f, -10f); // Zone minimale de respawn
    public Vector3 respawnAreaMax = new Vector3(10f, 5f, 10f); // Zone maximale de respawn
    
    [Header("Health Settings")]
    public int maxHealth = 50; // Points de vie maximum de la cible
    public float damagePerCollision = 10f; // Dégâts infligés par collision (ou utiliser la force de collision)
    public bool useCollisionForce = true; // Utiliser la force de collision pour calculer les dégâts
    
    [Header("Health Bar Settings")]
    public float healthBarHeight = 1.5f; // Hauteur de la barre de vie au-dessus de la cible (offset depuis le haut du collider)
    public Vector2 healthBarSize = new Vector2(2f, 0.3f); // Taille de la barre de vie (largeur, hauteur) - plus grande par défaut
    public bool showHealthBar = true; // Afficher ou cacher la barre de vie
    public bool alwaysShowHealthBar = true; // Toujours afficher la barre même à PV max
    
    private bool isShrinking = false; // Pour éviter plusieurs animations simultanées
    private Vector3 originalScale; // Échelle originale de l'objet
    private Vector3 originalPosition; // Position originale de l'objet
    private int currentHealth; // Points de vie actuels de la cible
    
    // Composants de la barre de vie
    private Canvas healthBarCanvas;
    private Image healthBarBackground;
    private Image healthBarFill;
    private Camera mainCamera;

    void Start()
    {
        // Sauvegarder l'échelle et la position originales
        originalScale = transform.localScale;
        originalPosition = transform.position;
        
        // Initialiser les points de vie
        currentHealth = maxHealth;
        
        // S'assurer qu'il y a un Collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Ajouter un BoxCollider par défaut si aucun collider n'existe
            col = gameObject.AddComponent<BoxCollider>();
        }
        
        // Configurer le collider selon le mode choisi
        col.isTrigger = useTrigger;
        
        // Si on utilise des collisions physiques, ajouter un Rigidbody kinematic
        if (!useTrigger)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true; // Kinematic pour que la cible ne bouge pas avec la physique
            }
        }
        
        // Créer la barre de vie
        CreateHealthBar();
        
        // Trouver la caméra principale
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignorer les collisions avec les projectiles (ils gèrent leurs propres dégâts)
        if (collision.gameObject.GetComponent<Projectile>() != null)
        {
            return; // Le projectile gère déjà les dégâts via son script
        }
        
        // Infliger des dégâts si pas déjà en train de disparaître
        if (!isShrinking && !useTrigger)
        {
            // Calculer les dégâts
            int damage = 0;
            if (useCollisionForce)
            {
                // Utiliser la force de collision pour calculer les dégâts
                float collisionForce = collision.relativeVelocity.magnitude;
                damage = Mathf.RoundToInt(collisionForce * damagePerCollision);
            }
            else
            {
                // Utiliser une valeur fixe
                damage = Mathf.RoundToInt(damagePerCollision);
            }
            
            Debug.Log("Collision détectée avec: " + collision.gameObject.name + " - Dégâts: " + damage);
            TakeDamage(damage);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignorer les collisions avec les projectiles (ils gèrent leurs propres dégâts)
        if (other.gameObject.GetComponent<Projectile>() != null)
        {
            return; // Le projectile gère déjà les dégâts via son script
        }
        
        // Infliger des dégâts si pas déjà en train de disparaître
        if (!isShrinking && useTrigger)
        {
            // Pour les triggers, utiliser une valeur fixe de dégâts
            int damage = Mathf.RoundToInt(damagePerCollision);
            
            Debug.Log("Trigger détecté avec: " + other.gameObject.name + " - Dégâts: " + damage);
            TakeDamage(damage);
        }
    }

    IEnumerator ShrinkAndDisappear()
    {
        isShrinking = true;
        
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        
        // Animer le rétrécissement de l'échelle actuelle à 0
        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shrinkDuration;
            
            // Interpolation linéaire de l'échelle de 1 à 0
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            
            yield return null;
        }
        
        // S'assurer que l'échelle est exactement à 0
        transform.localScale = Vector3.zero;
        
        // Réapparaître à une nouvelle position aléatoire
        RespawnAtRandomPosition();
    }
    
    void RespawnAtRandomPosition()
    {
        // Générer une position aléatoire dans la zone définie
        float randomX = Random.Range(respawnAreaMin.x, respawnAreaMax.x);
        float randomY = Random.Range(respawnAreaMin.y, respawnAreaMax.y);
        float randomZ = Random.Range(respawnAreaMin.z, respawnAreaMax.z);
        
        Vector3 newPosition = new Vector3(randomX, randomY, randomZ);
        
        // Réinitialiser la position
        transform.position = newPosition;
        
        // Réinitialiser l'échelle à l'originale
        transform.localScale = originalScale;
        
        // Réinitialiser l'état pour permettre une nouvelle collision
        isShrinking = false;
        
        // Réinitialiser les points de vie
        currentHealth = maxHealth;
        
        // Mettre à jour la barre de vie
        UpdateHealthBar();
        
        Debug.Log("Cible réapparue à la position: " + newPosition);
    }
    
    public void TakeDamage(int damage)
    {
        // Infliger les dégâts
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // S'assurer que les PV ne descendent pas en dessous de 0
        
        // Mettre à jour la barre de vie
        UpdateHealthBar();
        
        Debug.Log("Cible a subi " + damage + " dégâts. PV restants: " + currentHealth + "/" + maxHealth);
        
        // Vérifier si la cible a subi 50 dégâts ou plus (PV <= 0)
        if (currentHealth <= 0)
        {
            Debug.Log("Cible détruite! Dégâts totaux subis: " + (maxHealth - currentHealth));
            // Démarrer l'animation de rétrécissement
            StartCoroutine(ShrinkAndDisappear());
        }
    }
    
    void Update()
    {
        // Mettre à jour la position de la barre de vie pour qu'elle reste au-dessus de la cible
        if (healthBarCanvas != null)
        {
            // Calculer la position au-dessus de la cible en fonction du collider
            Vector3 barPosition = transform.position;
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                // Positionner la barre au-dessus du collider
                barPosition.y = col.bounds.max.y + healthBarHeight;
            }
            else
            {
                // Si pas de collider, utiliser la position + hauteur
                barPosition.y = transform.position.y + healthBarHeight;
            }
            
            healthBarCanvas.transform.position = barPosition;
            
            // Faire tourner la barre de vie pour qu'elle regarde toujours la caméra
            if (mainCamera != null)
            {
                healthBarCanvas.transform.LookAt(healthBarCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
    
    void CreateHealthBar()
    {
        if (!showHealthBar) return;
        
        // Créer un GameObject pour le Canvas
        GameObject canvasObject = new GameObject("HealthBarCanvas");
        canvasObject.transform.SetParent(transform);
        canvasObject.transform.localPosition = Vector3.zero;
        canvasObject.transform.localPosition = new Vector3(0, healthBarHeight, 0);
        
        // Ajouter le Canvas
        healthBarCanvas = canvasObject.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        healthBarCanvas.worldCamera = mainCamera;
        
        // Configurer la taille du Canvas pour le World Space
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            canvasRect = canvasObject.AddComponent<RectTransform>();
        }
        canvasRect.sizeDelta = healthBarSize;
        
        // Ne pas utiliser CanvasScaler pour World Space, on contrôle la taille directement
        
        // Ajouter un GraphicRaycaster (optionnel mais recommandé)
        canvasObject.AddComponent<GraphicRaycaster>();
        
        // Créer le fond de la barre de vie
        GameObject backgroundObject = new GameObject("HealthBarBackground");
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        healthBarBackground = backgroundObject.AddComponent<Image>();
        healthBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Gris foncé semi-transparent
        
        RectTransform bgRect = healthBarBackground.rectTransform;
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = Vector2.zero;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgRect.localPosition = Vector3.zero;
        
        // Créer le remplissage de la barre de vie
        GameObject fillObject = new GameObject("HealthBarFill");
        fillObject.transform.SetParent(backgroundObject.transform, false);
        healthBarFill = fillObject.AddComponent<Image>();
        healthBarFill.color = Color.green; // Vert pour la vie
        
        RectTransform fillRect = healthBarFill.rectTransform;
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // Mettre à jour la barre de vie initiale
        UpdateHealthBar();
    }
    
    void UpdateHealthBar()
    {
        if (healthBarFill == null || !showHealthBar) return;
        
        // Toujours afficher la barre si demandé
        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(true);
        }
        
        // Calculer le pourcentage de vie
        float healthPercentage = (float)currentHealth / (float)maxHealth;
        healthPercentage = Mathf.Clamp01(healthPercentage);
        
        // Mettre à jour la largeur du remplissage
        healthBarFill.fillAmount = healthPercentage;
        
        // Changer la couleur selon les PV (vert -> jaune -> rouge)
        if (healthPercentage > 0.5f)
        {
            // Vert à jaune
            float t = (healthPercentage - 0.5f) * 2f;
            healthBarFill.color = Color.Lerp(Color.yellow, Color.green, t);
        }
        else
        {
            // Jaune à rouge
            float t = healthPercentage * 2f;
            healthBarFill.color = Color.Lerp(Color.red, Color.yellow, t);
        }
        
        // Afficher ou cacher selon les paramètres
        if (healthBarCanvas != null)
        {
            if (alwaysShowHealthBar)
            {
                healthBarCanvas.gameObject.SetActive(true);
            }
            else
            {
                // Cacher seulement si les PV sont au maximum
                healthBarCanvas.gameObject.SetActive(healthPercentage < 1f || currentHealth < maxHealth);
            }
        }
    }
    
    // Getter pour obtenir les PV actuels
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // Getter pour obtenir les PV maximum
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}

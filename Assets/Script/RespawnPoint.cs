using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RespawnPoint : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Tag du joueur à détecter (laisse vide pour réagir à tout objet avec un Rigidbody)")]
    public string playerTag = "";
    
    [Tooltip("Activer ce point de respawn automatiquement au démarrage")]
    public bool activateOnStart = false;
    
    [Tooltip("Utiliser la physique 2D (Collider2D) au lieu de 3D")]
    public bool use2DPhysics = false;
    
    private Collider ownCollider;
    private Collider2D ownCollider2D;
    private static RespawnPoint lastActivatedRespawnPoint = null; // Dernier point de respawn activé
    
    void Awake()
    {
        // Récupère et force le collider en mode Trigger
        if (use2DPhysics)
        {
            ownCollider2D = GetComponent<Collider2D>();
            if (ownCollider2D != null)
            {
                if (!ownCollider2D.isTrigger)
                {
                    ownCollider2D.isTrigger = true;
                    Debug.Log($"[RespawnPoint] Collider2D de {gameObject.name} mis en mode Trigger automatiquement.");
                }
            }
            else
            {
                Debug.LogWarning($"[RespawnPoint] Aucun Collider2D trouvé sur {gameObject.name}. Ajoutez un Collider2D pour que le RespawnPoint fonctionne.");
            }
        }
        else
        {
            ownCollider = GetComponent<Collider>();
            if (ownCollider != null)
            {
                // Cas particulier des MeshCollider concaves : Unity ne supporte pas isTrigger dessus
                MeshCollider meshCol = ownCollider as MeshCollider;
                if (meshCol != null && !meshCol.convex)
                {
                    Debug.LogWarning($"[RespawnPoint] Trigger sur MeshCollider concave non supporté ({gameObject.name}). " +
                                     $"Passe ce MeshCollider en Convex ou remplace-le par un Box/Sphere/Capsule.");
                    return;
                }

                if (!ownCollider.isTrigger)
                {
                    ownCollider.isTrigger = true;
                    Debug.Log($"[RespawnPoint] Collider de {gameObject.name} mis en mode Trigger automatiquement.");
                }
            }
            else
            {
                Debug.LogWarning($"[RespawnPoint] Aucun Collider trouvé sur {gameObject.name}. Ajoutez un Collider pour que le RespawnPoint fonctionne.");
            }
        }
    }
    
    void Start()
    {
        // Si activé au démarrage, enregistrer ce point comme dernier respawn
        if (activateOnStart)
        {
            SetAsLastRespawnPoint();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (use2DPhysics) return; // Ignorer si on utilise la physique 2D
        
        // Message pour TOUS les contacts (pour déboguer)
        Debug.Log($"🔵 [RESPAWN POINT] {gameObject.name} : CONTACT DÉTECTÉ avec {other.gameObject.name}", gameObject);
        
        // Vérifier si c'est le joueur qui entre
        bool isPlayer = IsPlayer(other);
        if (!isPlayer)
        {
            Debug.LogWarning($"⚠️ [RESPAWN POINT] {gameObject.name} : {other.gameObject.name} n'est PAS détecté comme joueur.\n" +
                           $"   Tag de l'objet: {other.gameObject.tag}\n" +
                           $"   Tag recherché: {(string.IsNullOrEmpty(playerTag) ? "Aucun (détection auto)" : playerTag)}", gameObject);
            return;
        }
        
        // Message de succès très visible
        Debug.LogError($"✅✅✅ [RESPAWN POINT] {gameObject.name} : JOUEUR DÉTECTÉ ! CHECKPOINT ACTIVÉ ! ✅✅✅\n" +
                      $"   Position: {transform.position}\n" +
                      $"   Ce point sera utilisé au prochain respawn.", gameObject);
        
        // Enregistrer ce point comme le dernier respawn activé
        SetAsLastRespawnPoint();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!use2DPhysics) return; // Ignorer si on n'utilise pas la physique 2D
        
        // Message pour TOUS les contacts (pour déboguer)
        Debug.Log($"🔵 [RESPAWN POINT 2D] {gameObject.name} : CONTACT DÉTECTÉ avec {other.gameObject.name}", gameObject);
        
        // Vérifier si c'est le joueur qui entre
        bool isPlayer = IsPlayer2D(other);
        if (!isPlayer)
        {
            Debug.LogWarning($"⚠️ [RESPAWN POINT 2D] {gameObject.name} : {other.gameObject.name} n'est PAS détecté comme joueur.\n" +
                           $"   Tag de l'objet: {other.gameObject.tag}\n" +
                           $"   Tag recherché: {(string.IsNullOrEmpty(playerTag) ? "Aucun (détection auto)" : playerTag)}", gameObject);
            return;
        }
        
        // Message de succès très visible
        Debug.LogError($"✅✅✅ [RESPAWN POINT 2D] {gameObject.name} : JOUEUR DÉTECTÉ ! CHECKPOINT ACTIVÉ ! ✅✅✅\n" +
                      $"   Position: {transform.position}\n" +
                      $"   Ce point sera utilisé au prochain respawn.", gameObject);
        
        // Enregistrer ce point comme le dernier respawn activé
        SetAsLastRespawnPoint();
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (use2DPhysics) return; // Ignorer si on utilise la physique 2D
        
        // Mettre à jour même si le joueur reste en contact
        bool isPlayer = IsPlayer(other);
        if (!isPlayer) return;
        
        // Message périodique pour confirmer que le joueur est toujours en contact
        // (mais seulement une fois par seconde pour éviter le spam)
        if (Time.frameCount % 60 == 0) // Toutes les 60 frames environ
        {
            Debug.Log($"🟢 [RESPAWN POINT] {gameObject.name} : Joueur toujours en contact avec {other.gameObject.name}", gameObject);
        }
        
        SetAsLastRespawnPoint();
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!use2DPhysics) return; // Ignorer si on n'utilise pas la physique 2D
        
        // Mettre à jour même si le joueur reste en contact
        bool isPlayer = IsPlayer2D(other);
        if (!isPlayer) return;
        
        // Message périodique pour confirmer que le joueur est toujours en contact
        // (mais seulement une fois par seconde pour éviter le spam)
        if (Time.frameCount % 60 == 0) // Toutes les 60 frames environ
        {
            Debug.Log($"🟢 [RESPAWN POINT 2D] {gameObject.name} : Joueur toujours en contact avec {other.gameObject.name}", gameObject);
        }
        
        SetAsLastRespawnPoint();
    }
    
    private bool IsPlayer(Collider other)
    {
        // Si un tag est défini, vérifier le tag
        if (!string.IsNullOrEmpty(playerTag))
        {
            try
            {
                return other.CompareTag(playerTag);
            }
            catch
            {
                // Si le tag n'existe pas, continuer avec les autres vérifications
            }
        }
        
        // Sinon, vérifier si c'est un objet avec un Rigidbody (probablement le joueur)
        // ou chercher le script NewMonoBehaviourScript
        if (other.attachedRigidbody != null)
        {
            // Vérifier si c'est le joueur en cherchant le script NewMonoBehaviourScript
            NewMonoBehaviourScript playerScript = other.GetComponent<NewMonoBehaviourScript>();
            if (playerScript == null)
            {
                playerScript = other.GetComponentInParent<NewMonoBehaviourScript>();
            }
            if (playerScript == null)
            {
                playerScript = other.attachedRigidbody.GetComponent<NewMonoBehaviourScript>();
            }
            
            return playerScript != null;
        }
        
        return false;
    }
    
    private bool IsPlayer2D(Collider2D other)
    {
        // Si un tag est défini, vérifier le tag
        if (!string.IsNullOrEmpty(playerTag))
        {
            try
            {
                return other.CompareTag(playerTag);
            }
            catch
            {
                // Si le tag n'existe pas, continuer avec les autres vérifications
            }
        }
        
        // Sinon, vérifier si c'est un objet avec un Rigidbody2D (probablement le joueur)
        // ou chercher le script NewMonoBehaviourScript
        if (other.attachedRigidbody != null)
        {
            // Vérifier si c'est le joueur en cherchant le script NewMonoBehaviourScript
            NewMonoBehaviourScript playerScript = other.GetComponent<NewMonoBehaviourScript>();
            if (playerScript == null)
            {
                playerScript = other.GetComponentInParent<NewMonoBehaviourScript>();
            }
            if (playerScript == null)
            {
                playerScript = other.attachedRigidbody.GetComponent<NewMonoBehaviourScript>();
            }
            
            return playerScript != null;
        }
        
        return false;
    }
    
    public void SetAsLastRespawnPoint()
    {
        lastActivatedRespawnPoint = this;
        
        // Message très visible pour confirmer l'activation
        Debug.LogError($"📍📍📍 CHECKPOINT ENREGISTRÉ ! 📍📍📍\n" +
                      $"   Point de respawn: {gameObject.name}\n" +
                      $"   Position: X={transform.position.x:F2}, Y={transform.position.y:F2}, Z={transform.position.z:F2}\n" +
                      $"   Ce sera le point de respawn au prochain restart.", gameObject);
        
        // Mettre à jour aussi le respawnPosition dans NewMonoBehaviourScript si le joueur existe
        NewMonoBehaviourScript player = FindObjectOfType<NewMonoBehaviourScript>();
        if (player != null)
        {
            // Utiliser la réflexion ou une méthode publique pour mettre à jour respawnPosition
            // Pour l'instant, on laisse le système utiliser GetLastRespawnPosition() au respawn
        }
    }
    
    // Méthode statique pour obtenir le dernier point de respawn activé
    public static RespawnPoint GetLastActivatedRespawnPoint()
    {
        return lastActivatedRespawnPoint;
    }
    
    // Méthode statique pour obtenir la position du dernier respawn
    public static Vector3 GetLastRespawnPosition()
    {
        if (lastActivatedRespawnPoint != null)
        {
            return lastActivatedRespawnPoint.transform.position;
        }
        return Vector3.zero;
    }
    
    // Méthode statique pour obtenir la rotation du dernier respawn
    public static Quaternion GetLastRespawnRotation()
    {
        if (lastActivatedRespawnPoint != null)
        {
            return lastActivatedRespawnPoint.transform.rotation;
        }
        return Quaternion.identity;
    }
    
    // Méthode statique pour vérifier si un respawn a été activé
    public static bool HasActivatedRespawnPoint()
    {
        return lastActivatedRespawnPoint != null;
    }
}


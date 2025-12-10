using UnityEngine;

[RequireComponent(typeof(Collider))]
public class lavaWall : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Référence directe au GameObject du joueur (recommandé). Si laissé vide, cherchera automatiquement.")]
    public GameObject playerObject;
    
    [Tooltip("Tag du joueur à détecter (utilisé seulement si playerObject n'est pas assigné)")]
    public string playerTag = "";
    
    [Tooltip("Références directes aux plateformes à détecter (si pas de tag).")]
    public GameObject[] platformObjects;
    
    [Tooltip("Tag des plateformes à détecter (ex: Platform). Laissez vide pour détecter toutes les plateformes.")]
    public string platformTag = "";
    
    [Tooltip("Canvas ou panneau à afficher pour le restart (comme le script death)")]
    public GameObject restartScreen;
    
    [Tooltip("Utiliser la physique 2D (Collider2D) au lieu de 3D")]
    public bool use2DPhysics = false;
    
    private Collider ownCollider;
    private Collider2D ownCollider2D;
    private NewMonoBehaviourScript playerScript;
    
    void Awake()
    {
        // Récupérer les colliders
        if (use2DPhysics)
        {
            ownCollider2D = GetComponent<Collider2D>();
            if (ownCollider2D != null && !ownCollider2D.isTrigger)
            {
                ownCollider2D.isTrigger = true;
            }
        }
        else
        {
            ownCollider = GetComponent<Collider>();
            if (ownCollider != null)
            {
                // Cas particulier des MeshCollider concaves
                MeshCollider meshCol = ownCollider as MeshCollider;
                if (meshCol != null && !meshCol.convex)
                {
                    Debug.LogWarning($"[lavaWall] Trigger sur MeshCollider concave non supporté ({gameObject.name}). " +
                                     $"Passe ce MeshCollider en Convex ou remplace-le par un Box/Sphere/Capsule.");
                    return;
                }

                if (!ownCollider.isTrigger)
                {
                    ownCollider.isTrigger = true;
                }
            }
        }
    }
    
    void Start()
    {
        // Chercher le joueur si non assigné
        if (playerObject == null)
        {
            NewMonoBehaviourScript foundPlayer = FindObjectOfType<NewMonoBehaviourScript>();
            if (foundPlayer != null)
            {
                playerObject = foundPlayer.gameObject;
                playerScript = foundPlayer;
                Debug.Log($"[lavaWall] {gameObject.name} : Joueur trouvé automatiquement : {playerObject.name}");
            }
        }
        else
        {
            playerScript = playerObject.GetComponent<NewMonoBehaviourScript>();
        }
        
        // S'assurer que l'écran de restart est caché au démarrage
        if (restartScreen != null)
        {
            restartScreen.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (use2DPhysics) return;
        
        HandleContact(other.gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!use2DPhysics) return;
        
        HandleContact(other.gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (use2DPhysics) return;
        
        HandleContact(collision.gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!use2DPhysics) return;
        
        HandleContact(collision.gameObject);
    }
    
    private void HandleContact(GameObject other)
    {
        // Vérifier si c'est le joueur
        if (IsPlayer(other))
        {
            KillPlayer();
            return;
        }
        
        // Vérifier si c'est une plateforme
        if (IsPlatform(other))
        {
            // Si une plateforme touche le mur de lave, tuer le joueur aussi
            KillPlayer();
            Debug.Log($"[lavaWall] {gameObject.name} : Plateforme détectée, le joueur meurt !");
        }
    }
    
    private bool IsPlayer(GameObject obj)
    {
        // Vérifier si c'est le GameObject du joueur assigné
        if (playerObject != null && (obj == playerObject || obj.transform.IsChildOf(playerObject.transform)))
        {
            return true;
        }
        
        // Vérifier par tag
        if (!string.IsNullOrEmpty(playerTag))
        {
            try
            {
                if (obj.CompareTag(playerTag))
                {
                    return true;
                }
            }
            catch
            {
                // Tag n'existe pas
            }
        }
        
        // Vérifier par script
        NewMonoBehaviourScript script = obj.GetComponent<NewMonoBehaviourScript>();
        if (script == null)
        {
            script = obj.GetComponentInParent<NewMonoBehaviourScript>();
        }
        if (script == null && obj.GetComponent<Rigidbody>() != null)
        {
            script = obj.GetComponent<Rigidbody>().GetComponent<NewMonoBehaviourScript>();
        }
        
        return script != null;
    }
    
    private bool IsPlatform(GameObject obj)
    {
        // Priorité 1 : références directes
        if (platformObjects != null && platformObjects.Length > 0)
        {
            foreach (var p in platformObjects)
            {
                if (p == null) continue;
                if (obj == p || obj.transform.IsChildOf(p.transform))
                {
                    return true;
                }
            }
        }

        // Vérifier par tag
        if (!string.IsNullOrEmpty(platformTag))
        {
            try
            {
                if (obj.CompareTag(platformTag))
                {
                    return true;
                }
            }
            catch
            {
                // Tag n'existe pas
            }
        }
        
        // Vérifier si c'est une plateforme en cherchant des scripts de plateforme
        // (ElevatorPlatform, PlatformController, MovingCube, etc.)
        if (obj.GetComponent<ElevatorPlatform>() != null ||
            obj.GetComponent<PlatformController>() != null ||
            obj.GetComponent<MovingCube>() != null)
        {
            return true;
        }
        
        // Vérifier aussi dans les parents
        if (obj.transform.parent != null)
        {
            GameObject parent = obj.transform.parent.gameObject;
            if (parent.GetComponent<ElevatorPlatform>() != null ||
                parent.GetComponent<PlatformController>() != null ||
                parent.GetComponent<MovingCube>() != null)
            {
                return true;
            }

            // Vérifier si le parent est dans la liste des plateformes directes
            if (platformObjects != null && platformObjects.Length > 0)
            {
                foreach (var p in platformObjects)
                {
                    if (p == null) continue;
                    if (parent == p || parent.transform.IsChildOf(p.transform))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    private void KillPlayer()
    {
        // Trouver le script du joueur si on ne l'a pas déjà
        if (playerScript == null)
        {
            if (playerObject != null)
            {
                playerScript = playerObject.GetComponent<NewMonoBehaviourScript>();
            }
            
            if (playerScript == null)
            {
                playerScript = FindObjectOfType<NewMonoBehaviourScript>();
            }
        }
        
        // Tuer le joueur en infligeant des dégâts mortels
        if (playerScript != null)
        {
            // Infliger des dégâts mortels (plus que les PV max)
            playerScript.TakeDamage(9999);
            Debug.Log($"[lavaWall] {gameObject.name} : Le joueur a été tué par le mur de lave !");
        }
        
        // Afficher l'écran de restart (comme le script death)
        if (restartScreen != null)
        {
            restartScreen.SetActive(true);
        }
    }
}

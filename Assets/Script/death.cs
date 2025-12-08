using UnityEngine;

[RequireComponent(typeof(Collider))]
public class death : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Tag du joueur à détecter (laisse vide pour réagir à tout)")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Canvas ou panneau à afficher pour le restart")]
    [SerializeField] private GameObject restartScreen;

    private Collider ownCollider;

    void Awake()
    {
        // Récupère et force le collider en mode Trigger pour fonctionner sur une surface.
        ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            // Cas particulier des MeshCollider concaves : Unity ne supporte pas isTrigger dessus.
            MeshCollider meshCol = ownCollider as MeshCollider;
            if (meshCol != null && !meshCol.convex)
            {
                Debug.LogWarning($"[death] Trigger sur MeshCollider concave non supporté ({gameObject.name}). " +
                                 $"Passe ce MeshCollider en Convex ou remplace-le par un Box/Sphere/Capsule pour la zone de mort.");
                return; // On ne force pas isTrigger pour éviter l'erreur Unity.
            }

            if (!ownCollider.isTrigger)
            {
                ownCollider.isTrigger = true;
            }
        }
    }

    void Start()
    {
        // S'assurer que l'écran de restart est caché au démarrage.
        if (restartScreen != null)
        {
            restartScreen.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Aucun écran de restart assigné sur " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si playerTag est vide, on accepte tout ce qui entre ; sinon on filtre.
        bool isTarget = string.IsNullOrEmpty(playerTag) || other.CompareTag(playerTag);
        if (!isTarget) return;

        if (restartScreen != null)
        {
            restartScreen.SetActive(true);
        }
    }
}

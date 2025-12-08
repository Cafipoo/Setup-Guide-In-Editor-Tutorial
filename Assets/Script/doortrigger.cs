using UnityEngine;

public class doortrigger : MonoBehaviour
{
    [Header("Zone de détection")]
    [Tooltip("Rayon de détection pour l'entrée (si vous utilisez un SphereCollider, cette valeur sera appliquée au démarrage)")]
    public float detectionRadius = 5f;
    
    [Tooltip("Rayon de détection pour la sortie (plus petit = retour à la position initiale plus rapide)")]
    public float exitDetectionRadius = 2f;

    private Vector3 initialRotation;
    private bool isRotated = false;
    private SphereCollider triggerCollider;
    private BoxCollider boxTriggerCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Sauvegarde la rotation initiale
        initialRotation = transform.eulerAngles;

        // Configure la zone de détection selon le type de collider
        triggerCollider = GetComponent<SphereCollider>();
        if (triggerCollider != null)
        {
            triggerCollider.radius = detectionRadius;
        }
        else
        {
            boxTriggerCollider = GetComponent<BoxCollider>();
            if (boxTriggerCollider != null)
            {
                // Augmente la taille du BoxCollider proportionnellement
                boxTriggerCollider.size = new Vector3(detectionRadius * 2, detectionRadius * 2, detectionRadius * 2);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Détecte quand un objet entre dans le trigger
    void OnTriggerEnter(Collider other)
    {
        if (!isRotated)
        {
            // Rotation Y de 90 degrés
            transform.Rotate(0, 90, 0);
            isRotated = true;
            
            // Réduit la zone de détection pour la sortie
            ReduceDetectionZone();
        }
    }

    // Détecte quand un objet sort du trigger
    void OnTriggerExit(Collider other)
    {
        if (isRotated)
        {
            // Restaure la rotation initiale
            transform.eulerAngles = initialRotation;
            isRotated = false;
            
            // Restaure la zone de détection normale
            RestoreDetectionZone();
        }
    }

    // Réduit la zone de détection pour faciliter la sortie
    private void ReduceDetectionZone()
    {
        if (triggerCollider != null)
        {
            triggerCollider.radius = exitDetectionRadius;
        }
        else if (boxTriggerCollider != null)
        {
            boxTriggerCollider.size = new Vector3(exitDetectionRadius * 2, exitDetectionRadius * 2, exitDetectionRadius * 2);
        }
    }

    // Restaure la zone de détection normale
    private void RestoreDetectionZone()
    {
        if (triggerCollider != null)
        {
            triggerCollider.radius = detectionRadius;
        }
        else if (boxTriggerCollider != null)
        {
            boxTriggerCollider.size = new Vector3(detectionRadius * 2, detectionRadius * 2, detectionRadius * 2);
        }
    }
}

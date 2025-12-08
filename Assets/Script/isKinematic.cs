using UnityEngine;

public class isKinematic : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Si activé, ce cube sera en mode Kinematic (non affecté par la physique)")]
    public bool enableKinematic = true;
    
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Récupérer le composant Rigidbody
        rb = GetComponent<Rigidbody>();
        
        // Si aucun Rigidbody n'existe, en créer un
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.LogWarning("Aucun Rigidbody trouvé sur " + gameObject.name + ". Un Rigidbody a été ajouté automatiquement.");
            // Si on vient de créer le Rigidbody, utiliser la valeur par défaut du script
            rb.isKinematic = enableKinematic;
        }
        else
        {
            // Si le Rigidbody existe déjà, lire sa valeur actuelle et synchroniser enableKinematic
            enableKinematic = rb.isKinematic;
        }
        
        // Afficher un message pour indiquer l'état
        if (enableKinematic)
        {
            Debug.Log(gameObject.name + " : isKinematic = TRUE (non affecté par la physique, gravité, forces) - Le cube ne tombera PAS");
        }
        else
        {
            Debug.Log(gameObject.name + " : isKinematic = FALSE (affecté par la physique normale) - Le cube tombera avec la gravité");
        }
        
        // Vérification visuelle : changer la couleur du cube selon son état
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (enableKinematic)
            {
                renderer.material.color = Color.green; // Vert = Kinematic (ne tombe pas)
            }
            else
            {
                renderer.material.color = Color.red; // Rouge = Non-Kinematic (tombe)
            }
        }
    }

    // Méthode pour changer l'état isKinematic à l'exécution
    public void ToggleKinematic()
    {
        if (rb != null)
        {
            rb.isKinematic = !rb.isKinematic;
            enableKinematic = rb.isKinematic;
            Debug.Log(gameObject.name + " : isKinematic changé à " + rb.isKinematic);
        }
    }
}

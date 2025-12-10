using UnityEngine;
using System.Collections;

[AddComponentMenu("Custom/Platform Controller")]
public class PlatformController : MonoBehaviour
{
    [Header("Paramètres de mouvement")]
    [Tooltip("Vitesse de déplacement")]
    public float moveSpeed = 5f;
    
    [Tooltip("Référence à la caméra (laissé vide pour trouver automatiquement)")]
    public Camera playerCamera;

    private Vector3 initialPosition;
    private bool isPlayerOnPlatform = false;
    private Coroutine returnCoroutine;
    private Transform playerTransform;

    void Start()
    {
        // Sauvegarde la position initiale
        initialPosition = transform.position;
        
        // Trouve la caméra si elle n'est pas assignée
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Si le joueur est sur la plateforme, contrôle le mouvement avec la caméra
        if (isPlayerOnPlatform && playerCamera != null)
        {
            ControlPlatformWithCamera();
        }
    }

    // Détecte quand le joueur atterrit sur la plateforme (Trigger)
    void OnTriggerEnter(Collider other)
    {
        StartPlatformMovement(other.transform);
    }

    // Détecte quand le joueur quitte la plateforme (Trigger)
    void OnTriggerExit(Collider other)
    {
        StopPlatformMovement();
    }

    // Détecte quand le joueur atterrit sur la plateforme (Collision)
    void OnCollisionEnter(Collision collision)
    {
        StartPlatformMovement(collision.transform);
    }

    // Détecte quand le joueur quitte la plateforme (Collision)
    void OnCollisionExit(Collision collision)
    {
        StopPlatformMovement();
    }

    // Démarre le mouvement de la plateforme
    private void StartPlatformMovement(Transform otherTransform)
    {
        if (!isPlayerOnPlatform)
        {
            isPlayerOnPlatform = true;
            playerTransform = otherTransform;
            Debug.Log("Joueur détecté sur la plateforme - Contrôle par caméra activé");
            
            // Arrête le retour à la position initiale si en cours
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }
        }
    }

    // Arrête le mouvement de la plateforme
    private void StopPlatformMovement()
    {
        if (isPlayerOnPlatform)
        {
            isPlayerOnPlatform = false;
            playerTransform = null;
            Debug.Log("Joueur quitte la plateforme - Retour à la position initiale");
            
            // Retourne à la position initiale
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
            returnCoroutine = StartCoroutine(ReturnToInitialPosition());
        }
    }

    // Contrôle la plateforme avec l'orientation de la caméra
    private void ControlPlatformWithCamera()
    {
        // Récupère la direction de la caméra (où elle regarde)
        Vector3 cameraForward = playerCamera.transform.forward;
        
        // Projette uniquement sur le plan horizontal (X, Z) - pas de mouvement vertical
        Vector3 horizontalDirection = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
        
        // Calcule la nouvelle position (mouvement horizontal uniquement, Y reste à la hauteur initiale)
        Vector3 newPosition = transform.position + horizontalDirection * moveSpeed * Time.deltaTime;
        newPosition.y = initialPosition.y; // Maintient la hauteur initiale
        
        // Calcule le déplacement de la plateforme
        Vector3 platformMovement = newPosition - transform.position;
        
        // Déplace la plateforme
        transform.position = newPosition;
        
        // Déplace le joueur avec la plateforme
        if (playerTransform != null)
        {
            playerTransform.position += platformMovement;
        }
    }

    // Coroutine pour retourner à la position initiale
    private IEnumerator ReturnToInitialPosition()
    {
        while (Vector3.Distance(transform.position, initialPosition) > 0.1f)
        {
            Vector3 previousPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, moveSpeed * Time.deltaTime);
            
            // Déplace le joueur avec la plateforme s'il est toujours dessus (au cas où)
            if (playerTransform != null)
            {
                Vector3 platformMovement = transform.position - previousPosition;
                playerTransform.position += platformMovement;
            }
            
            yield return null;
        }
        
        // Assure que la position finale est exacte
        transform.position = initialPosition;
        returnCoroutine = null;
    }

    // Réinitialisation manuelle (utilisé au death/restart du joueur)
    public void ResetPlatform()
    {
        // Stopper tout retour en cours
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // Réinitialiser l'état
        isPlayerOnPlatform = false;
        playerTransform = null;

        // Revenir à la position initiale immédiatement
        transform.position = initialPosition;
    }
}

using UnityEngine;

public class BallControl : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject ballPrefab; // Le prefab de la balle à tirer
    public float shootVelocity = 20f; // Vitesse de la balle
    public float shootCooldown = 0.5f; // Temps entre chaque tir
    
    [Header("Spawn Settings")]
    public Vector3 spawnOffset = Vector3.zero; // Offset de position pour le spawn
    
    private float lastShootTime = 0f;

    void Update()
    {
        // Détecter l'input pour tirer (clic gauche ou espace)
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Vérifier le cooldown
        if (Time.time - lastShootTime < shootCooldown)
            return;

        // Vérifier qu'un prefab est assigné
        if (ballPrefab == null)
        {
            Debug.LogWarning("BallControl: Aucun prefab de balle assigné!");
            return;
        }

        // Calculer la position de spawn (position actuelle + offset)
        Vector3 spawnPosition = transform.position + transform.TransformDirection(spawnOffset);

        // Instancier la balle
        GameObject newBall = Instantiate(ballPrefab, spawnPosition, transform.rotation);

        // Ajouter un Rigidbody si nécessaire
        Rigidbody ballRigidbody = newBall.GetComponent<Rigidbody>();
        if (ballRigidbody == null)
        {
            ballRigidbody = newBall.AddComponent<Rigidbody>();
        }

        // Appliquer la vélocité dans la direction avant (forward)
        Vector3 shootDirection = transform.forward;
        ballRigidbody.linearVelocity = shootDirection * shootVelocity;

        // Mettre à jour le temps du dernier tir
        lastShootTime = Time.time;
    }
}

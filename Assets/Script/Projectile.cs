using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 25; // Dégâts infligés par la balle
    
    [Header("Lifetime Settings")]
    public float lifetime = 10f; // Temps avant que la balle se détruise automatiquement
    
    private float spawnTime;
    
    void Start()
    {
        spawnTime = Time.time;
    }
    
    void Update()
    {
        // Détruire la balle après un certain temps
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Vérifier si on a touché une cible
        target targetComponent = collision.gameObject.GetComponent<target>();
        if (targetComponent != null)
        {
            // Infliger les dégâts à la cible
            targetComponent.TakeDamage(damage);
            Debug.Log("Balle a infligé " + damage + " dégâts à la cible: " + collision.gameObject.name);
            
            // Détruire la balle après l'impact
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Vérifier si on a touché une cible (pour les triggers)
        target targetComponent = other.gameObject.GetComponent<target>();
        if (targetComponent != null)
        {
            // Infliger les dégâts à la cible
            targetComponent.TakeDamage(damage);
            Debug.Log("Balle a infligé " + damage + " dégâts à la cible: " + other.gameObject.name);
            
            // Détruire la balle après l'impact
            Destroy(gameObject);
        }
    }
}


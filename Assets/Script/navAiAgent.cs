using UnityEngine;
using UnityEngine.AI;

public class moveTo : MonoBehaviour
{
    [Header("Patrouille")]
    public Transform[] waypoints; // Points de patrouille autour de la map
    public float waypointReachDistance = 1f; // Distance à laquelle on considère qu'on a atteint un waypoint
    
    [Header("Détection du Joueur")]
    public Transform player; // Référence au joueur (peut être laissé null pour auto-détection)
    public float detectionDistance = 10f; // Distance à laquelle l'IA détecte le joueur
    public float stopChaseDistance = 15f; // Distance à laquelle l'IA arrête de suivre le joueur
    
    [Header("Paramètres")]
    public float patrolWaitTime = 1f; // Temps d'attente à chaque waypoint
    
    [Header("Mort du Joueur")]
    public int killDamage = 999; // Dégâts infligés au joueur (assez pour le tuer)
    
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isChasingPlayer = false;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool hasKilledPlayer = false; // Pour éviter de tuer plusieurs fois
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Si le joueur n'est pas assigné, essayer de le trouver automatiquement
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                // Chercher le script NewMonoBehaviourScript
                NewMonoBehaviourScript playerScript = FindObjectOfType<NewMonoBehaviourScript>();
                if (playerScript != null)
                {
                    player = playerScript.transform;
                }
            }
            else
            {
                player = playerObject.transform;
            }
        }
        
        // Si on a des waypoints, commencer la patrouille
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.destination = waypoints[0].position;
        }
    }

    void Update()
    {
        if (agent == null) return;
        
        // Vérifier la distance au joueur
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Si le joueur est proche et qu'on ne le suit pas déjà, commencer à le suivre
            if (distanceToPlayer <= detectionDistance && !isChasingPlayer)
            {
                StartChasing();
            }
            // Si on suit le joueur mais qu'il s'est éloigné, arrêter de le suivre
            else if (distanceToPlayer > stopChaseDistance && isChasingPlayer)
            {
                StopChasing();
            }
            
            // Si on suit le joueur, mettre à jour la destination
            if (isChasingPlayer)
            {
                agent.destination = player.position;
            }
        }
        
        // Si on ne suit pas le joueur, faire la patrouille
        if (!isChasingPlayer && waypoints != null && waypoints.Length > 0)
        {
            Patrol();
        }
    }
    
    void Patrol()
    {
        // Si on attend, ne rien faire
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                // Passer au waypoint suivant
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                agent.destination = waypoints[currentWaypointIndex].position;
            }
            return;
        }
        
        // Vérifier si on a atteint le waypoint actuel
        if (waypoints[currentWaypointIndex] != null)
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
            
            if (distanceToWaypoint <= waypointReachDistance)
            {
                // On a atteint le waypoint, attendre un peu
                isWaiting = true;
                waitTimer = patrolWaitTime;
            }
        }
    }
    
    void StartChasing()
    {
        isChasingPlayer = true;
        isWaiting = false; // Arrêter d'attendre si on était en attente
        Debug.Log("IA: Je commence à suivre le joueur!");
    }
    
    void StopChasing()
    {
        isChasingPlayer = false;
        // Reprendre la patrouille au waypoint le plus proche
        if (waypoints != null && waypoints.Length > 0)
        {
            FindNearestWaypoint();
            agent.destination = waypoints[currentWaypointIndex].position;
        }
        Debug.Log("IA: J'arrête de suivre le joueur, retour à la patrouille.");
    }
    
    void FindNearestWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        float nearestDistance = float.MaxValue;
        int nearestIndex = 0;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                float distance = Vector3.Distance(transform.position, waypoints[i].position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }
        }
        
        currentWaypointIndex = nearestIndex;
    }
    
    // Méthode pour dessiner les gizmos dans l'éditeur (aide visuelle)
    void OnDrawGizmosSelected()
    {
        // Dessiner la zone de détection
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
        
        // Dessiner la zone d'arrêt de poursuite
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
        
        // Dessiner les waypoints et les connexions
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
                    
                    // Dessiner une ligne vers le waypoint suivant
                    int nextIndex = (i + 1) % waypoints.Length;
                    if (waypoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                    }
                }
            }
        }
    }
    
    // Détection de collision avec le joueur (pour les colliders normaux)
    void OnCollisionEnter(Collision collision)
    {
        CheckPlayerContact(collision.gameObject);
    }
    
    // Détection de trigger avec le joueur (pour les colliders en mode trigger)
    void OnTriggerEnter(Collider other)
    {
        CheckPlayerContact(other.gameObject);
    }
    
    // Vérifier si l'objet qui a touché le bot est le joueur
    void CheckPlayerContact(GameObject other)
    {
        // Éviter de tuer plusieurs fois
        if (hasKilledPlayer) return;
        
        // Vérifier si c'est le joueur
        if (player != null && other.transform == player)
        {
            KillPlayer();
        }
        // Vérifier aussi par le script du joueur
        else
        {
            NewMonoBehaviourScript playerScript = other.GetComponent<NewMonoBehaviourScript>();
            if (playerScript != null)
            {
                KillPlayer();
            }
        }
    }
    
    // Tuer le joueur et afficher l'écran de fin
    void KillPlayer()
    {
        if (hasKilledPlayer) return; // Double vérification
        
        hasKilledPlayer = true;
        
        // Trouver le script du joueur si on ne l'a pas déjà
        NewMonoBehaviourScript playerScript = null;
        if (player != null)
        {
            playerScript = player.GetComponent<NewMonoBehaviourScript>();
        }
        
        if (playerScript == null)
        {
            playerScript = FindObjectOfType<NewMonoBehaviourScript>();
        }
        
        // Tuer le joueur en lui infligeant des dégâts mortels
        if (playerScript != null)
        {
            playerScript.TakeDamage(killDamage);
            Debug.Log("Le bot a tué le joueur!");
        }
    }
    
    // Méthode publique pour réinitialiser complètement le bot
    public void ResetBot()
    {
        // Réinitialiser l'état de poursuite
        isChasingPlayer = false;
        isWaiting = false;
        waitTimer = 0f;
        hasKilledPlayer = false;
        
        // Réinitialiser le waypoint au premier
        currentWaypointIndex = 0;
        
        // Reprendre la patrouille au premier waypoint
        if (agent != null && waypoints != null && waypoints.Length > 0)
        {
            agent.destination = waypoints[0].position;
        }
        
        Debug.Log("Bot réinitialisé!");
    }
}

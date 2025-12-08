using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // La balle à suivre
    public bool followPosition = true;
    public bool followRotation = true;

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 5f, -10f); // Offset par rapport à la balle
    public float followSpeed = 10f; // Vitesse de suivi (smooth)

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f; // Vitesse de rotation
    public bool lookAtTarget = true; // Regarder directement la balle
    public bool followMovementDirection = false; // Suivre la direction de mouvement

    [Header("Advanced Settings")]
    public bool useFixedUpdate = false; // Utiliser FixedUpdate pour la physique

    private Rigidbody targetRigidbody;
    private Vector3 lastTargetPosition;
    private Vector3 movementDirection;

    void Start()
    {
        // Si aucune cible n'est assignée, chercher automatiquement
        if (target == null)
        {
            GameObject ball = GameObject.FindGameObjectWithTag("Player");
            if (ball != null)
            {
                target = ball.transform;
            }
            else
            {
                // Chercher le script NewMonoBehaviourScript
                NewMonoBehaviourScript ballScript = FindObjectOfType<NewMonoBehaviourScript>();
                if (ballScript != null)
                {
                    target = ballScript.transform;
                }
            }
        }

        // Récupérer le Rigidbody de la cible pour suivre la direction de mouvement
        if (target != null)
        {
            targetRigidbody = target.GetComponent<Rigidbody>();
            lastTargetPosition = target.position;
        }
    }

    void Update()
    {
        if (!useFixedUpdate && target != null)
        {
            UpdateCamera();
        }
    }

    void FixedUpdate()
    {
        if (useFixedUpdate && target != null)
        {
            UpdateCamera();
        }
    }

    void LateUpdate()
    {
        // LateUpdate pour s'assurer que la caméra suit après tous les mouvements
        if (target != null)
        {
            UpdateCamera();
        }
    }

    void UpdateCamera()
    {
        if (target == null) return;

        // Calculer la direction de mouvement si nécessaire
        if (followMovementDirection && targetRigidbody != null)
        {
            Vector3 velocity = targetRigidbody.linearVelocity;
            if (velocity.magnitude > 0.1f)
            {
                movementDirection = velocity.normalized;
            }
        }
        else
        {
            // Calculer la direction basée sur le changement de position
            Vector3 currentPosition = target.position;
            Vector3 positionDelta = currentPosition - lastTargetPosition;
            if (positionDelta.magnitude > 0.01f)
            {
                movementDirection = positionDelta.normalized;
            }
            lastTargetPosition = currentPosition;
        }

        // Suivre la position
        if (followPosition)
        {
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }

        // Suivre la rotation
        if (followRotation)
        {
            if (lookAtTarget)
            {
                // Regarder directement la balle
                Vector3 lookDirection = target.position - transform.position;
                if (lookDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else if (followMovementDirection && movementDirection.magnitude > 0.1f)
            {
                // Suivre la direction de mouvement de la balle
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}


using UnityEngine;

public class playerScriptAnim : MonoBehaviour
{
    private Animator animator;
    private string defeatedTrigger = "Defeated"; // Nom du trigger dans l'Animator
    private string jumpTrigger = "jumpTrigger"; // Nom du trigger pour le saut
    private string hitTrigger = "hitTrigger"; // Nom du trigger pour l'attaque
    private string walkTrigger = "walkTrigger"; // Nom du trigger pour la marche

    void Start()
    {
        // Récupérer le composant Animator
        animator = GetComponent<Animator>();
        
        // Si l'Animator n'est pas sur ce GameObject, chercher dans les enfants
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        // Avertir si aucun Animator n'est trouvé
        if (animator == null)
        {
            Debug.LogWarning("Aucun Animator trouvé sur " + gameObject.name + ". Assurez-vous qu'un Animator est attaché au GameObject ou à ses enfants.");
        }
    }

    void Update()
    {
        // Détecter l'appui sur la touche P
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayDefeatedAnimation();
        }
        
        // Détecter l'appui sur la touche Espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayJumpAnimation();
        }
        
        // Détecter le clic gauche de la souris
        if (Input.GetMouseButtonDown(0))
        {
            PlayHitAnimation();
        }
    }

    void PlayDefeatedAnimation()
    {
        if (animator != null)
        {
            // Essayer d'abord avec un trigger (méthode la plus courante)
            if (HasParameter(animator, defeatedTrigger))
            {
                animator.SetTrigger(defeatedTrigger);
            }
            // Sinon, essayer de jouer l'animation directement par son nom
            else
            {
                animator.Play("Defeated");
            }
        }
        else
        {
            Debug.LogWarning("Impossible de jouer l'animation Defeated : aucun Animator trouvé.");
        }
    }

    // Méthode publique pour jouer l'animation de saut (force l'arrêt des autres animations)
    public void PlayJumpAnimation()
    {
        if (animator != null)
        {
            // Réinitialiser tous les autres triggers pour forcer l'arrêt des autres animations
            ResetAllTriggers();
            
            // Essayer d'abord avec un trigger (méthode la plus courante)
            if (HasParameter(animator, jumpTrigger))
            {
                animator.SetTrigger(jumpTrigger);
            }
            // Sinon, essayer de jouer l'animation directement par son nom
            else
            {
                animator.Play("Jump");
            }
        }
        else
        {
            Debug.LogWarning("Impossible de jouer l'animation Jump : aucun Animator trouvé.");
        }
    }

    // Méthode publique pour jouer l'animation d'attaque (force l'arrêt des autres animations)
    public void PlayHitAnimation()
    {
        if (animator != null)
        {
            // Réinitialiser tous les autres triggers pour forcer l'arrêt des autres animations
            ResetAllTriggers();
            
            // Essayer d'abord avec un trigger (méthode la plus courante)
            if (HasParameter(animator, hitTrigger))
            {
                animator.SetTrigger(hitTrigger);
            }
            // Sinon, essayer de jouer l'animation directement par son nom
            else
            {
                animator.Play("Hit");
            }
        }
        else
        {
            Debug.LogWarning("Impossible de jouer l'animation Hit : aucun Animator trouvé.");
        }
    }

    // Méthode publique pour jouer l'animation de marche
    public void PlayWalkAnimation()
    {
        if (animator != null)
        {
            // Essayer d'abord avec un trigger (méthode la plus courante)
            if (HasParameter(animator, walkTrigger))
            {
                animator.SetTrigger(walkTrigger);
            }
            // Sinon, essayer de jouer l'animation directement par son nom
            else
            {
                animator.Play("Walking");
            }
        }
        else
        {
            Debug.LogWarning("Impossible de jouer l'animation Walking : aucun Animator trouvé.");
        }
    }

    // Réinitialiser tous les triggers pour forcer l'arrêt des animations
    void ResetAllTriggers()
    {
        if (animator != null && animator.parameters != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(param.name);
                }
            }
        }
    }

    // Vérifier si un paramètre existe dans l'Animator
    bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class restart : MonoBehaviour
{
    [Header("Configuration du Bouton")]
    [Tooltip("Assignez directement le bouton de restart ici (recommandé). Si laissé vide, le script cherchera automatiquement.")]
    public Button restartButton; // Bouton à assigner dans l'inspecteur Unity
    
    [Header("Debug")]
    public bool enableDebugClickDetection = true; // Activer la détection de clic alternative
    
    void Start()
    {
        // Vérifier et créer l'EventSystem si nécessaire
        CheckEventSystem();
        
        // Si un bouton est assigné dans l'inspecteur, l'utiliser en priorité
        if (restartButton != null)
        {
            ConfigureButton(restartButton);
            Debug.Log("✅ Bouton assigné dans l'inspecteur configuré: " + restartButton.name);
        }
        else
        {
            // Sinon, essayer de trouver et configurer automatiquement le bouton de restart
            SetupRestartButton();
        }
    }
    
    void Update()
    {
        // Détection alternative de clic si le bouton ne fonctionne pas normalement
        if (enableDebugClickDetection && Input.GetMouseButtonDown(0))
        {
            // Prioriser le bouton assigné dans l'inspecteur
            Button targetButton = restartButton;
            
            // Si pas de bouton assigné, chercher tous les boutons
            if (targetButton == null)
            {
                Button[] buttons = FindObjectsOfType<Button>(true);
                foreach (Button btn in buttons)
                {
                    string buttonName = btn.name.ToLower();
                    if (buttonName.Contains("restart") || buttonName.Contains("relancer") || buttonName.Contains("rejouer"))
                    {
                        targetButton = btn;
                        break;
                    }
                }
            }
            
            // Vérifier si on clique sur le bouton cible
            if (targetButton != null && targetButton.gameObject.activeInHierarchy && targetButton.interactable)
            {
                RectTransform rectTransform = targetButton.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector2 mousePos = Input.mousePosition;
                    if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos))
                    {
                        Debug.Log("🖱️ CLIC DÉTECTÉ SUR LE BOUTON (méthode alternative): " + targetButton.name);
                        RestartGame();
                        return;
                    }
                }
            }
        }
    }
    
    // Vérifier que l'EventSystem existe
    void CheckEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("⚠️ Aucun EventSystem trouvé! Création d'un EventSystem...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("✅ EventSystem créé automatiquement");
        }
        else
        {
            Debug.Log("✅ EventSystem trouvé: " + eventSystem.gameObject.name);
        }
    }
    
    // Configurer un bouton spécifique
    void ConfigureButton(Button btn)
    {
        if (btn == null)
        {
            Debug.LogWarning("⚠️ Tentative de configurer un bouton null!");
            return;
        }
        
        // S'assurer que le bouton est interactable
        if (!btn.interactable)
        {
            btn.interactable = true;
            Debug.Log("   Bouton rendu interactable");
        }
        
        // Retirer tous les listeners existants et ajouter le nôtre
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(RestartGame);
        Debug.Log("✅ Bouton '" + btn.name + "' configuré avec RestartGame()");
    }
    
    // Configurer automatiquement le bouton de restart
    void SetupRestartButton()
    {
        Debug.Log("=== RECHERCHE DES BOUTONS ===");
        
        // Chercher tous les boutons dans la scène
        Button[] buttons = FindObjectsOfType<Button>(true); // true = inclure les objets désactivés
        Debug.Log("Nombre total de boutons trouvés: " + buttons.Length);
        
        if (buttons.Length == 0)
        {
            Debug.LogWarning("⚠️ Aucun bouton trouvé dans la scène!");
            Debug.LogWarning("💡 ASTUCE: Assignez le bouton dans le champ 'Restart Button' de l'inspecteur Unity!");
            return;
        }
        
        bool foundRestartButton = false;
        
        foreach (Button btn in buttons)
        {
            Debug.Log("Bouton trouvé: " + btn.name + " (Interactable: " + btn.interactable + ", Actif: " + btn.gameObject.activeInHierarchy + ")");
            
            // Si le bouton contient "restart" ou "Restart" dans son nom
            string buttonName = btn.name.ToLower();
            if (buttonName.Contains("restart") || buttonName.Contains("relancer") || buttonName.Contains("rejouer"))
            {
                Debug.Log("🔍 Bouton restart détecté: " + btn.name);
                ConfigureButton(btn);
                foundRestartButton = true;
            }
        }
        
        if (!foundRestartButton)
        {
            Debug.LogWarning("⚠️ Aucun bouton avec 'restart', 'relancer' ou 'rejouer' dans le nom trouvé!");
            Debug.LogWarning("💡 ASTUCE: Assignez le bouton dans le champ 'Restart Button' de l'inspecteur Unity!");
        }
        
        Debug.Log("=== FIN DE LA RECHERCHE ===");
    }
    
    // Méthode publique appelée par le bouton pour relancer la partie
    public void RestartGame()
    {
        // LOG DE TEST - Vérifier si le clic est bien capté
        Debug.Log("✅✅✅ CLIC SUR LE BOUTON DÉTECTÉ ! ✅✅✅");
        Debug.Log("=== BOUTON RESTART CLIQUE - DÉBUT DE LA RÉINITIALISATION ===");
        
        // Réinitialiser tous les joueurs
        NewMonoBehaviourScript[] players = FindObjectsOfType<NewMonoBehaviourScript>();
        Debug.Log("Nombre de joueurs trouvés: " + players.Length);
        foreach (NewMonoBehaviourScript player in players)
        {
            if (player != null)
            {
                player.ResetPlayer();
                Debug.Log("Joueur réinitialisé: " + player.gameObject.name);
            }
        }
        
        // Réinitialiser tous les bots
        moveTo[] bots = FindObjectsOfType<moveTo>();
        Debug.Log("Nombre de bots trouvés: " + bots.Length);
        foreach (moveTo bot in bots)
        {
            if (bot != null)
            {
                bot.ResetBot();
                Debug.Log("Bot réinitialisé: " + bot.gameObject.name);
            }
        }
        
        // Détruire toutes les balles restantes
        Projectile[] projectiles = FindObjectsOfType<Projectile>();
        Debug.Log("Nombre de projectiles trouvés: " + projectiles.Length);
        foreach (Projectile projectile in projectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }
        
        Debug.Log("=== RÉINITIALISATION TERMINÉE ===");
    }
    
    // Méthode de test pour vérifier que le bouton fonctionne
    public void TestButtonClick()
    {
        Debug.Log("✅✅✅ TEST: Le bouton fonctionne! La méthode TestButtonClick a été appelée. ✅✅✅");
    }
    
    // Méthode pour forcer la configuration de TOUS les boutons (à appeler manuellement si nécessaire)
    [ContextMenu("Forcer Configuration Boutons")]
    public void ForceSetupAllButtons()
    {
        Debug.Log("=== FORÇAGE DE LA CONFIGURATION DE TOUS LES BOUTONS ===");
        CheckEventSystem();
        SetupRestartButton();
        
        // Configurer TOUS les boutons, pas seulement ceux avec "restart" dans le nom
        Button[] allButtons = FindObjectsOfType<Button>(true);
        Debug.Log("Configuration de TOUS les boutons (" + allButtons.Length + " trouvés)...");
        
        foreach (Button btn in allButtons)
        {
            if (!btn.interactable)
            {
                btn.interactable = true;
                Debug.Log("   Bouton '" + btn.name + "' rendu interactable");
            }
            
            // Ajouter le listener à tous les boutons
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(RestartGame);
            Debug.Log("   Bouton '" + btn.name + "' configuré avec RestartGame()");
        }
        
        Debug.Log("=== CONFIGURATION TERMINÉE ===");
    }
    
    // Alternative: recharger la scène (si la réinitialisation manuelle ne fonctionne pas)
    public void RestartGameByReloadingScene()
    {
        Debug.Log("Rechargement de la scène...");
        
        // Essayer par index d'abord
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex >= 0)
        {
            SceneManager.LoadScene(currentSceneIndex);
            Debug.Log("Scène rechargée par index: " + currentSceneIndex);
        }
        else
        {
            // Sinon par nom
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(currentSceneName))
            {
                SceneManager.LoadScene(currentSceneName);
                Debug.Log("Scène rechargée par nom: " + currentSceneName);
            }
            else
            {
                Debug.LogError("Impossible de recharger la scène! Index: " + currentSceneIndex + ", Nom: " + currentSceneName);
            }
        }
    }
}

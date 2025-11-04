using System;
using UnityEngine;

/// <summary>
/// Change la couleur de la vache selon l'heure:
/// - Minutes paires (0, 2, 4, 6...) = Couleur normale (blanche)
/// - Minutes impaires (1, 3, 5, 7...) = Couleur bleue
/// </summary>
public class DynamicCowColor : MonoBehaviour
{
    [Header("Cow Renderer")]
    [Tooltip("Le Renderer de la vache (sera détecté automatiquement si vide)")]
    public Renderer cowRenderer;

    [Header("Colors")]
    [Tooltip("Couleur normale de la vache (minutes paires)")]
    public Color normalColor = Color.white;

    [Tooltip("Couleur pour les minutes impaires")]
    public Color oddMinuteColor = Color.blue;

    [Header("Settings")]
    [Tooltip("Vitesse de transition entre les couleurs (0 = instantané)")]
    [Range(0f, 5f)]
    public float transitionSpeed = 2f;

    [Tooltip("Vérifier l'heure toutes les X secondes")]
    public float checkInterval = 1f;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private Material cowMaterial;
    private Color targetColor;
    private Color currentColor;
    private int lastMinute = -1;
    private float nextCheckTime;

    private void Start()
    {
        // Trouver le renderer automatiquement si non assigné
        if (cowRenderer == null)
        {
            // Chercher d'abord sur le GameObject actuel
            cowRenderer = GetComponent<Renderer>();

            // Si pas trouvé, chercher dans les enfants
            if (cowRenderer == null)
            {
                cowRenderer = GetComponentInChildren<Renderer>();
            }
        }

        if (cowRenderer == null)
        {
            Debug.LogError($"[DynamicCowColor] Aucun Renderer trouvé sur '{gameObject.name}' ni dans ses enfants! Assurez-vous que la vache a un MeshRenderer ou SkinnedMeshRenderer.");
            enabled = false;
            return;
        }

        Debug.Log($"[DynamicCowColor] Renderer trouvé sur '{cowRenderer.gameObject.name}'");

        // Créer une instance matériau pour pas modifier le matériau shared
        cowMaterial = cowRenderer.material;
        currentColor = cowMaterial.color;

        // Vérifier l'heure 
        CheckAndUpdateColor();
    }

    private void Update()
    {
        // Vérifier l'heure
        if (Time.time >= nextCheckTime)
        {
            CheckAndUpdateColor();
            nextCheckTime = Time.time + checkInterval;
        }


        if (transitionSpeed > 0)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
            cowMaterial.color = currentColor;
        }
    }

    /// <summary>
    /// Vérifie l'heure actuelle et met à jour la couleur
    /// </summary>
    private void CheckAndUpdateColor()
    {
        DateTime now = DateTime.Now;
        int currentMinute = now.Minute;

        if (currentMinute != lastMinute)
        {
            lastMinute = currentMinute;

            // Déterminer la couleur selon si la minute est paire ou impaire
            bool isEvenMinute = (currentMinute % 2) == 0;
            targetColor = isEvenMinute ? normalColor : oddMinuteColor;


            if (transitionSpeed == 0)
            {
                currentColor = targetColor;
                cowMaterial.color = currentColor;
            }

            if (showDebugLogs)
            {
                string minuteType = isEvenMinute ? "PAIRE" : "IMPAIRE";
                string colorName = isEvenMinute ? "NORMALE" : "BLEUE";
                Debug.Log($"[DynamicCowColor] {now:HH:mm:ss} - Minute {currentMinute} ({minuteType}) → Couleur {colorName}");
            }
        }
    }

    /// <summary>
    /// Force la mise à jour immédiate (utile pour tester)
    /// </summary>
    [ContextMenu("Force Update Color")]
    public void ForceUpdate()
    {
        lastMinute = -1; // Reset pour forcer la mise à jour
        CheckAndUpdateColor();
    }



    private void OnDestroy()
    {
        // Nettoyer le matériau instancié
        if (cowMaterial != null)
        {
            Destroy(cowMaterial);
        }
    }

    private void OnValidate()
    {
        // Mettre à jour la couleur en temps réel dans l'éditeur
        if (Application.isPlaying && cowMaterial != null)
        {
            ForceUpdate();
        }
    }
}

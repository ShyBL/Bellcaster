using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance { get; private set; }

    [System.Serializable]
    public struct AreaSpawnPoint
    {
        public string areaName;
        public Transform spawnTransform;
        public Vector2 cameraPosition;
        public GroundBounds areaGround;
        public Transform interactablesContainer;
    }

    [Header("Character Reference")]
    [SerializeField] private NinaController _nina;
    [SerializeField] private Transform _cameraTransform;
    
    [Header("Area Points in Scene")]
    [SerializeField] private List<AreaSpawnPoint> _spawnPoints = new List<AreaSpawnPoint>();

    [Header("Fade UI & Animation")]
    [SerializeField] private GameObject _fadeOverlayObject;
    [SerializeField] private Animator _fadeAnimator;
    [SerializeField, Tooltip("Animator trigger parameter for fading back in")]
    private string _fadeInTrigger = "FadeIn";

    [Header("Animation Clips (for auto-duration)")]
    [SerializeField] private AnimationClip _fadeOutClip;
    [SerializeField] private AnimationClip _fadeInClip;

    private Dictionary<string, AreaSpawnPoint> _spawnPointLookup;
    public GroundBounds CurrentGround { get; private set; }
    public string CurrentAreaName { get; private set; }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _spawnPointLookup = new Dictionary<string, AreaSpawnPoint>();
        foreach (var point in _spawnPoints)
        {
            if (!string.IsNullOrEmpty(point.areaName) && point.spawnTransform != null)
            {
                _spawnPointLookup[point.areaName] = point;
            }
        }
    }

    public void NavigateTo(string areaName)
    {
        if (!_spawnPointLookup.TryGetValue(areaName, out AreaSpawnPoint targetPoint))
        {
            Debug.LogError($"[NavigationManager] Area '{areaName}' not found in Inspector!");
            return;
        }

        StartCoroutine(TransitionRoutine(targetPoint));
    }

    private IEnumerator TransitionRoutine(AreaSpawnPoint targetPoint)
    {
        // Enable Fade Out object
        if (_fadeOverlayObject != null)
        {
            _fadeOverlayObject.SetActive(true);
        }

        float fadeOutDuration = _fadeOutClip != null ? _fadeOutClip.length : 0.5f;
        yield return new WaitForSeconds(fadeOutDuration);

        // Move Nina to target position
        if (_nina != null && targetPoint.spawnTransform != null)
        {
            _nina.transform.position = targetPoint.spawnTransform.position;
        }

        // Move Camera to target area position --- ADDED ---
        if (_cameraTransform != null)
        {
            _cameraTransform.position = new Vector3(
                targetPoint.cameraPosition.x,
                targetPoint.cameraPosition.y,
                _cameraTransform.position.z
            );
        }
        
        // Update the active ground bounds
        CurrentGround = targetPoint.areaGround;
        CurrentAreaName = targetPoint.areaName;
        
        // Refresh scene interactables now that we are in a new area
        PlayerInputHandler inputHandler = FindFirstObjectByType<PlayerInputHandler>();
        if (inputHandler != null)
        {
            // Pass the active area's interactable parent container
            inputHandler.RefreshInteractables(targetPoint.interactablesContainer);
        }
        
        // AUTO-SAVE: Save game state when entering a new area
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        
        // Trigger Fade In animation
        if (_fadeAnimator != null)
        {
            _fadeAnimator.SetTrigger(_fadeInTrigger);
        }

        float fadeInDuration = _fadeInClip != null ? _fadeInClip.length : 0.5f;
        yield return new WaitForSeconds(fadeInDuration);
    }

    /// <summary>
    /// Called by SaveManager on startup to instantly set up the initial area without a fade.
    /// </summary>
    public void InitializeAreaOnBoot(string areaName)
    {
        if (!_spawnPointLookup.TryGetValue(areaName, out AreaSpawnPoint targetPoint))
        {
            Debug.LogError($"[NavigationManager] Boot area '{areaName}' not found in spawn points list!");
            return;
        }

        CurrentAreaName = areaName;

        // 1. Teleport Nina to position
        if (_nina != null && targetPoint.spawnTransform != null)
        {
            _nina.transform.position = targetPoint.spawnTransform.position;
        }

        // 2. Set Camera position
        if (_cameraTransform != null)
        {
            _cameraTransform.position = new Vector3(
                targetPoint.cameraPosition.x,
                targetPoint.cameraPosition.y,
                _cameraTransform.position.z
            );
        }

        // 3. Assign active scene GroundBounds
        CurrentGround = targetPoint.areaGround;

        // 4. Refresh interactables for this area
        PlayerInputHandler inputHandler = FindFirstObjectByType<PlayerInputHandler>();
        if (inputHandler != null)
        {
            // Pass the active area's interactable parent container
            inputHandler.RefreshInteractables(targetPoint.interactablesContainer);
        }
    }
}
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Default Fallback Settings")]
    [SerializeField, Tooltip("Fallback area if no save file exists.")]
    private string _defaultStartingArea = "LivingRoom";

    // Save Keys
    private const string KEY_CURRENT_AREA = "player_current_area";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Automatically load and set up the game on boot
        LoadAndInitialize();
    }

    /// <summary>
    /// Reads saved data and initializes scene managers.
    /// </summary>
    public void LoadAndInitialize()
    {
        // 1. Fetch saved area from static SaveSystem
        string targetArea = SaveSystem.RequestLoad(KEY_CURRENT_AREA);

        // Fallback if brand new save or file doesn't exist
        if (string.IsNullOrEmpty(targetArea))
        {
            targetArea = _defaultStartingArea;
        }

        // 2. Tell NavigationManager to set up the starting area
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.InitializeAreaOnBoot(targetArea);
        }
        else
        {
            Debug.LogError("[SaveManager] NavigationManager not found in scene!");
        }
    }

    /// <summary>
    /// Call this anytime to save the current game state.
    /// </summary>
    public void SaveGame()
    {
        if (NavigationManager.Instance == null) return;

        // 1. Save current location name
        string currentArea = NavigationManager.Instance.CurrentAreaName;
        if (!string.IsNullOrEmpty(currentArea))
        {
            SaveSystem.Set(KEY_CURRENT_AREA, currentArea);
        }

        // 2. Flush memory buffer to disk (single write operation)
        SaveSystem.Flush();
        Debug.Log($"[SaveManager] Game saved! Current Area: {currentArea}");
    }
    
    #region Context Menu / Debug Tools

    /// <summary>
    /// Deletes the local save file on disk and resets in-memory save data.
    /// Right-click SaveManager in the Inspector -> Reset & Delete Save Data.
    /// </summary>
    [ContextMenu("Debug/Reset & Delete Save Data")]
    public void ResetSaveData()
    {
        SaveSystem.Clear();
        Debug.Log("<color=yellow>[SaveManager] Save data deleted and cleared!</color>");
    }

    /// <summary>
    /// Manually forces a save event from the Inspector while playing.
    /// </summary>
    [ContextMenu("Debug/Force Save Game")]
    private void Debug_ForceSave()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[SaveManager] You can only save while in Play Mode!");
            return;
        }

        SaveGame();
    }

    /// <summary>
    /// Deletes the save data AND re-initializes the starting scene state in one click.
    /// </summary>
    [ContextMenu("Debug/Reset Save & Reload Boot State")]
    private void Debug_ResetAndReload()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[SaveManager] You can only reload state while in Play Mode!");
            return;
        }

        ResetSaveData();
        LoadAndInitialize();
        Debug.Log("<color=green>[SaveManager] Save reset and initial state reloaded!</color>");
    }

    #endregion
}
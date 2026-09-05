using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opened/closed via the HUD Pack button. Populates the shared slot prefab grid from
/// InventoryManager.PackItems and refreshes live via OnPackChanged. Also hosts the
/// Journal icon button that hands off to JournalPanelController.
/// </summary>
public class PackPanelController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Grid")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _gridParent;

    [Header("Journal Handoff")]
    [SerializeField] private JournalPanelController _journalPanel;

    private readonly List<InventorySlotUI> _spawnedSlots = new List<InventorySlotUI>();

    void Awake()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }

    void Start()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnPackChanged += HandlePackChanged;
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnPackChanged -= HandlePackChanged;
    }

    private void HandlePackChanged()
    {
        // Only pay the rebuild cost if the panel is actually visible.
        if (_panelRoot != null && _panelRoot.activeSelf)
            Refresh();
    }

    /// <summary>Wire this to the HUD Pack button's OnClick.</summary>
    public void TogglePanel()
    {
        if (_panelRoot != null && _panelRoot.activeSelf) ClosePanel();
        else OpenPanel();
    }

    public void OpenPanel()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(true);

        UIModalState.NotifyOpened();
        Refresh();
    }

    public void ClosePanel()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);

        UIModalState.NotifyClosed();
    }

    /// <summary>Wire this to the Journal icon button's OnClick.</summary>
    public void OpenJournal()
    {
        ClosePanel();

        if (_journalPanel != null)
            _journalPanel.OpenPanel();
        else
            Debug.LogError("[PackPanelController] JournalPanelController not assigned.", this);
    }

    private void Refresh()
    {
        foreach (var slot in _spawnedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        _spawnedSlots.Clear();

        if (InventoryManager.Instance == null || _slotPrefab == null || _gridParent == null) return;

        foreach (InteractableData item in InventoryManager.Instance.PackItems)
        {
            GameObject slotObj = Instantiate(_slotPrefab, _gridParent);
            InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();
            if (slot == null)
            {
                Debug.LogError("[PackPanelController] Slot prefab is missing InventorySlotUI.", this);
                continue;
            }

            slot.Setup(item, locked: false, draggable: true);
            _spawnedSlots.Add(slot);
        }
    }
}

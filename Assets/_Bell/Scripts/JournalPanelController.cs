using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fullscreen souvenir book. Iterates SouvenirRegistry.AllSouvenirs in order, rendering
/// each entry locked (silhouette + "???") or unlocked based on InventoryManager.HasSouvenir.
/// Refreshes live via OnSouvenirsChanged while open. Clicking an unlocked slot shows its
/// flavour text via ExamineTextDisplay — same behaviour as examining an object in the world.
/// </summary>
public class JournalPanelController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField, Tooltip("Set blocksRaycasts true while open so world clicks don't pass through.")]
    private CanvasGroup _canvasGroup;

    [Header("Grid")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _gridParent;

    [Header("Data")]
    [SerializeField] private SouvenirRegistry _registry;

    private readonly List<InventorySlotUI> _spawnedSlots = new List<InventorySlotUI>();

    void Awake()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }

    void Start()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnSouvenirsChanged += HandleSouvenirsChanged;
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnSouvenirsChanged -= HandleSouvenirsChanged;
    }

    private void HandleSouvenirsChanged()
    {
        if (_panelRoot != null && _panelRoot.activeSelf)
            Refresh();
    }

    public void OpenPanel()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(true);

        if (_canvasGroup != null)
            _canvasGroup.blocksRaycasts = true;

        UIModalState.NotifyOpened();
        Refresh();
    }

    public void ClosePanel()
    {
        if (_canvasGroup != null)
            _canvasGroup.blocksRaycasts = false;

        if (_panelRoot != null)
            _panelRoot.SetActive(false);

        UIModalState.NotifyClosed();
    }

    private void Refresh()
    {
        foreach (var slot in _spawnedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        _spawnedSlots.Clear();

        if (_registry == null || InventoryManager.Instance == null || _slotPrefab == null || _gridParent == null)
        {
            Debug.LogError("[JournalPanelController] Missing registry or references.", this);
            return;
        }

        foreach (InteractableData souvenir in _registry.AllSouvenirs)
        {
            bool locked = !InventoryManager.Instance.HasSouvenir(souvenir);

            GameObject slotObj = Instantiate(_slotPrefab, _gridParent);
            InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();
            if (slot == null)
            {
                Debug.LogError("[JournalPanelController] Slot prefab is missing InventorySlotUI.", this);
                continue;
            }

            slot.Setup(souvenir, locked, draggable: false);
            slot.OnSlotClicked += OnSouvenirClicked;
            _spawnedSlots.Add(slot);
        }
    }

    private void OnSouvenirClicked(InteractableData souvenir)
    {
        if (ExamineTextDisplay.Instance != null)
            ExamineTextDisplay.Instance.ShowText(souvenir.examineText);
    }
}

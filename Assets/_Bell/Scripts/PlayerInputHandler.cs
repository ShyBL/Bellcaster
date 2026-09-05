using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all player input using the New Input System (PlayerActions action map).
///
/// Supported modes (auto-detected from last device used):
///   KBM-1  Mouse cursor → Left Click to select → character walks → menu appears
///   KBM-2  Arrow keys cycle interactables → Left Click or Enter to confirm
///   PAD-3  Left stick / D-Pad cycles interactables → Right Trigger to confirm
///
/// When the character arrives at the selected interactable, <see cref="Interactable.OnClick"/>
/// is called.
///
/// Also owns two things added alongside the Toolbelt/Quips features:
///   • World-space item dragging — pulling an equipped Toolbelt item (e.g. the
///     Hammer) off Nina and dropping it on a target Interactable. Mirrors
///     InventorySlotUI's UI-side drag, but the press/drag/release detection here
///     is done by polling Mouse.current directly rather than through uGUI's drag
///     interfaces, since a ToolbeltItemView isn't a Canvas Graphic. Both paths
///     converge on the same BeginWalkToUseItem/TryUseItem flow once a target is
///     resolved.
///   • Nina's Quips triggers — repeat-click, idle, and self-click all funnel
///     through here since every player action already passes through this class.
///     The fourth trigger (wrong item) lives on Interactable.TryUseItem instead,
///     since that's where the failure is actually detected.
///
/// Setup requirements
/// ──────────────────
/// • Input Actions asset with an action map called "Player" containing:
///     Click          (Button)   — left mouse button / right trigger
///     CycleNext      (Button)   — right arrow / right d-pad / right stick push
///     CyclePrev      (Button)   — left arrow / left d-pad / left stick push
///     Confirm        (Button)   — Enter key (alias of Click for KBM)
///     MenuExamine    (Button)   — X / square
///     MenuInteract   (Button)   — Y / triangle
///     MenuPickUp     (Button)   — B / circle
///     Cancel         (Button)   — Escape / circle / B
///
/// • <see cref="NinaController"/> reference
/// • <see cref="Camera"/> reference (defaults to Camera.main)
/// • A Collider2D on Nina herself (set as trigger), assigned to _ninaCollider,
///   for self-click quip detection.
/// • A SpriteRenderer assigned to _worldDragGhost, disabled by default, used
///   to visually follow the cursor while dragging a Toolbelt item.
/// </summary>
[DisallowMultipleComponent]
public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance { get; private set; }
    
    // Inspector 
    [Header("References")]
    [SerializeField] private NinaController _nina;
    [SerializeField] private Camera _camera;

    [Header("Scene Interactables")]
    [SerializeField, Tooltip("Leave empty — auto-populated at runtime")]
    private List<Interactable> _sceneInteractables = new List<Interactable>();

    [Header("Self-Click (Quips)")]
    [SerializeField, Tooltip("A Collider2D on Nina herself. A click landing here fires a self-click quip instead of a walk order.")]
    private Collider2D _ninaCollider;

    [Header("Quip Timing")]
    [SerializeField, Tooltip("Consecutive clicks on the same object before a repeat-click quip fires. Resets after firing.")]
    private int _repeatClickThreshold = 2;
    [SerializeField, Tooltip("Seconds of no input before an idle quip fires. Paused while Nina is moving or a Pack/Journal panel is open.")]
    private float _idleQuipDelay = 30f;

    [Header("World Item Drag (Toolbelt)")]
    [SerializeField, Tooltip("A SpriteRenderer that follows the cursor while dragging an equipped Toolbelt item off Nina. Kept disabled until a drag starts.")]
    private SpriteRenderer _worldDragGhost;
    [SerializeField, Tooltip("Screen-pixel distance the cursor must move before a press on a Toolbelt item counts as a drag rather than a click.")]
    private float _dragThresholdPixels = 12f;

    // Input Actions 
    private InputSystem_Actions _actions;
    private InputAction _actClick;
    private InputAction _actCycleNext;
    private InputAction _actCyclePrev;
    private InputAction _actConfirm;
    private InputAction _actCancel;

    // Runtime state 
    private int _cycleIndex = -1;
    private Interactable _hoveredView;
    private Interactable _cycledView;
    private Interactable _pendingView;
    private InteractableData _pendingItem; 
    
    //private bool _menuOpen = false;
    private Queue<Vector2> _currentPath = new Queue<Vector2>();
    private System.Action _onPathComplete;

    // Quips state
    private Interactable _lastClickedView;
    private int _repeatClickCount;
    private float _idleTimer;

    // World item drag state
    private ToolbeltItemView _draggedToolbeltItem;
    private Interactable _worldDragHoverTarget;
    private Vector2 _dragPressScreenPos;
    private bool _dragThresholdCrossed;
    
    #region Unity lifecycle

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (_nina == null)
            Debug.LogError("[PlayerInputHandler] NinaController not assigned.", this);

        if (_camera == null)
            _camera = Camera.main;

        if (_worldDragGhost != null)
            _worldDragGhost.gameObject.SetActive(false);

        _actions = new InputSystem_Actions();

        _actClick        = _actions.Player.Click;
        _actCycleNext    = _actions.Player.CycleNext;
        _actCyclePrev    = _actions.Player.CyclePrev;
        _actConfirm      = _actions.Player.Confirm;
        _actCancel       = _actions.Player.Cancel;
    }

    void OnEnable()
    {
        _actions.Enable();

        _actClick.performed        += OnClick;
        _actConfirm.performed      += OnConfirm;
        _actCycleNext.performed    += OnCycleNext;
        _actCyclePrev.performed    += OnCyclePrev;
        _actCancel.performed       += OnCancel;

        RefreshInteractables();
    }

    void OnDisable()
    {
        _actClick.performed        -= OnClick;
        _actConfirm.performed      -= OnConfirm;
        _actCycleNext.performed    -= OnCycleNext;
        _actCyclePrev.performed    -= OnCyclePrev;
        _actCancel.performed       -= OnCancel;

        _actions.Disable();
    }

    void Update()
    {
        UpdateHoveredInteractable();
        UpdateWorldItemDrag();
        UpdateIdleQuip();
    }

    #endregion
    
    private void UpdateHoveredInteractable()
    {
        // World item drag owns hover highlighting while it's in progress (see
        // UpdateWorldDragHover) — don't let this fight it over the same target.
        if (_draggedToolbeltItem != null) return;

        // Hover only applies when using the mouse (not keyboard/gamepad cycling)
        if (_cycledView != null) return;
        if (_camera == null || Mouse.current == null) return;

        Vector2 mouseWorld = MouseToWorld();
        var newHover = GetInteractableAtWorld(mouseWorld);

        if (newHover != _hoveredView)
        {
            if (_hoveredView != null)
            {
                _hoveredView.SetHighlight(false);
                _hoveredView.SetLabel(false);
            }
            _hoveredView = newHover;
            if (_hoveredView != null)
            {
                _hoveredView.SetHighlight(true);
                _hoveredView.SetLabel(true);
            }
        }
    }

    #region Input callbacks

    /// <summary>Left mouse button (KBM) or Right Trigger (gamepad).</summary>
    private void OnClick(InputAction.CallbackContext ctx)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        ResetIdleTimer();

        // Cycle selection active — confirm it
        if (_cycledView != null)
        {
            NavigateTo(_cycledView);
            return;
        }

        // Mouse: use world position under cursor
        if (_camera == null || Mouse.current == null) return;

        Vector2 worldPoint = MouseToWorld();

        // Toolbelt items are handled entirely by the world-space drag system in
        // Update() (press-down is detected there too) — don't also treat this
        // press as a normal world click / walk order.
        if (GetToolbeltItemAtWorld(worldPoint) != null) return;

        // Clicking Nina herself — a quip, not a walk order.
        if (_ninaCollider != null && _ninaCollider.OverlapPoint(worldPoint))
        {
            NinaSpeechBubble.Instance?.ShowCategory(NinaQuipCategory.SelfClick);
            return;
        }

        Interactable view = GetInteractableAtWorld(worldPoint);
        if (view != null)
        {
            NavigateTo(view);
            return;
        }

        // Ground walk — only if the click lands inside the walkable polygon.
        if (NavigationManager.Instance.CurrentGround == null) return;
        Vector2 walkTarget = NavigationManager.Instance.CurrentGround.IsOnGround(worldPoint)
            ? worldPoint
            : NavigationManager.Instance.CurrentGround.ClosestPointOnBoundary(worldPoint);

        List<Vector2> path = NavigationManager.Instance.CurrentGround.FindPath(_nina.transform.position, walkTarget);
        MoveAlongPath(path);
    }

    /// <summary>Enter key — same as click for KBM cycling (mode 2).</summary>
    private void OnConfirm(InputAction.CallbackContext ctx) => OnClick(ctx);

    private void OnCycleNext(InputAction.CallbackContext ctx) => CycleInteractable(+1);
    private void OnCyclePrev(InputAction.CallbackContext ctx) => CycleInteractable(-1);

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        ResetIdleTimer();

        if (_nina != null)
        {
            _currentPath.Clear();
            _nina.CancelMovement();
        }
        ClearSelection();
    }

    #endregion

    #region Cycling

    private void CycleInteractable(int direction)
    {
        ResetIdleTimer();

        if (_sceneInteractables.Count == 0) return;

        if (_hoveredView != null)
        {
            _hoveredView.SetHighlight(false);
            _hoveredView.SetLabel(false);
            _hoveredView = null;
        }

        _cycleIndex = (_cycleIndex + direction + _sceneInteractables.Count)
                      % _sceneInteractables.Count;

        if (_cycledView != null)
        {
            _cycledView.SetHighlight(false);
            _cycledView.SetLabel(false);
        }

        _cycledView = _sceneInteractables[_cycleIndex];
        _cycledView.SetHighlight(true);
        _cycledView.SetLabel(true);
    }

    private void ClearSelection()
    {
        if (_cycledView != null)
        {
            _cycledView.SetHighlight(false);
            _cycledView.SetLabel(false);
        }
        _cycledView  = null;
        _cycleIndex  = -1;
        _pendingView = null;
        _pendingItem = null;
    }

    #endregion
    
    #region Navigation
    
    private void NavigateTo(Interactable view)
    {
        TrackRepeatClick(view);

        _pendingItem = null;
        _pendingView = view;

        if (_cycledView != null && _cycledView != view)
        {
            _cycledView.SetHighlight(false);
            _cycledView.SetLabel(false);
            _cycledView = null;
        }
        
        if (NavigationManager.Instance.CurrentGround == null) return;

        List<Vector2> path = NavigationManager.Instance.CurrentGround.FindPath(_nina.transform.position, view.InteractionPosition);
        MoveAlongPath(path, OnNinaArrived);
        
        // if (GroundBounds.Instance == null) return;
        //
        // List<Vector2> path = GroundBounds.Instance.FindPath(_nina.transform.position, view.InteractionPosition);
        // MoveAlongPath(path, OnNinaArrived);
    }

    private void OnNinaArrived()
    {
        if (_pendingView == null) return;

        Interactable view = _pendingView;
        _pendingView = null;

        view.SetHighlight(false);
        view.SetLabel(false);
        if (_hoveredView == view) _hoveredView = null;

        view.OnClick();
    }
    
    // private void OnNinaArrived()
    // {
    //     if (_pendingView == null) return;
    //
    //     _menuOpen = true;
    //     
    //     _pendingView.OnClick();
    //     _pendingView.SetHighlight(false);
    //     _pendingView = null;
    // }

    // Poll for menu close (InteractionMenu has no close event)
    // private void LateUpdate()
    // {
    //     if (!_menuOpen) return;
    //     if (InteractionMenu.Instance == null) return;
    //
    //     if (!InteractionMenu.Instance.menuContainer.activeSelf)
    //     {
    //         _menuOpen = false;
    //         ClearSelection();
    //     }
    // }
    
    private void MoveAlongPath(List<Vector2> path, System.Action onComplete = null)
    {
        _currentPath.Clear();
        _onPathComplete = onComplete;

        // Skip the first waypoint (index 0) because it is Nina's current position
        for (int i = 1; i < path.Count; i++)
        {
            _currentPath.Enqueue(path[i]);
        }

        MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        if (_currentPath.Count == 0)
        {
            _onPathComplete?.Invoke();
            _onPathComplete = null;
            return;
        }

        Vector2 nextPoint = _currentPath.Dequeue();
        _nina.MoveTo(nextPoint, MoveToNextWaypoint);
    }

    #endregion

    #region Quips

    /// <summary>
    /// Tracks consecutive clicks on the same Interactable. Fires once the streak
    /// hits the threshold, then resets — so it takes a full new streak to fire
    /// again rather than quipping on every click past the first trigger.
    /// </summary>
    private void TrackRepeatClick(Interactable view)
    {
        if (view == _lastClickedView)
        {
            _repeatClickCount++;
        }
        else
        {
            _lastClickedView = view;
            _repeatClickCount = 1;
        }

        if (_repeatClickCount >= _repeatClickThreshold)
        {
            _repeatClickCount = 0;
            NinaSpeechBubble.Instance?.ShowCategory(NinaQuipCategory.RepeatClick);
        }
    }

    /// <summary>
    /// Paused while Nina is walking or a Pack/Journal panel is open (via
    /// UIModalState) so she doesn't quip mid-movement or mid-browse.
    /// </summary>
    private void UpdateIdleQuip()
    {
        if (UIModalState.IsAnyModalOpen || (_nina != null && _nina.IsMoving))
        {
            _idleTimer = 0f;
            return;
        }

        _idleTimer += Time.deltaTime;

        if (_idleTimer >= _idleQuipDelay)
        {
            _idleTimer = 0f;
            NinaSpeechBubble.Instance?.ShowCategory(NinaQuipCategory.Idle);
        }
    }

    private void ResetIdleTimer() => _idleTimer = 0f;

    #endregion

    #region World Item Drag (Toolbelt)

    /// <summary>
    /// Polls the mouse directly rather than using uGUI's drag interfaces, since
    /// a ToolbeltItemView lives in world space, not on a Canvas. Detects
    /// press-down on an equipped item, tracks whether the cursor has moved far
    /// enough to count as a drag (vs. a plain click), and on release either
    /// shows examine text (click) or attempts to use the item on whatever's
    /// under the cursor (drag).
    /// </summary>
    private void UpdateWorldItemDrag()
    {
        if (_camera == null || Mouse.current == null) return;

        if (_draggedToolbeltItem == null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
            {
                Vector2 worldPoint = MouseToWorld();
                ToolbeltItemView hit = GetToolbeltItemAtWorld(worldPoint);
                if (hit != null) BeginWorldItemDrag(hit);
            }
            return;
        }

        Vector2 screenPos = Mouse.current.position.ReadValue();

        if (!_dragThresholdCrossed && Vector2.Distance(screenPos, _dragPressScreenPos) >= _dragThresholdPixels)
        {
            _dragThresholdCrossed = true;
            ShowWorldDragGhost();
        }

        if (_dragThresholdCrossed && _worldDragGhost != null)
        {
            Vector2 worldPos = MouseToWorld();
            _worldDragGhost.transform.position = worldPos;
            UpdateWorldDragHover(worldPos);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            EndWorldItemDrag();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private ToolbeltItemView GetToolbeltItemAtWorld(Vector2 worldPos)
    {
        return InventoryManager.Instance != null
            ? InventoryManager.Instance.GetItemViewAtWorld(worldPos)
            : null;
    }

    private void BeginWorldItemDrag(ToolbeltItemView item)
    {
        ResetIdleTimer();

        _draggedToolbeltItem = item;
        _dragPressScreenPos = Mouse.current.position.ReadValue();
        _dragThresholdCrossed = false;
    }

    private void ShowWorldDragGhost()
    {
        if (_worldDragGhost == null || _draggedToolbeltItem == null) return;

        SpriteRenderer sourceRenderer = _draggedToolbeltItem.GetComponent<SpriteRenderer>();
        _worldDragGhost.sprite = sourceRenderer != null ? sourceRenderer.sprite : null;
        _worldDragGhost.gameObject.SetActive(true);
    }

    private void UpdateWorldDragHover(Vector2 worldPos)
    {
        Interactable target = GetInteractableAtWorld(worldPos);
        if (target == _worldDragHoverTarget) return;

        if (_worldDragHoverTarget != null)
            _worldDragHoverTarget.SetHighlight(false);

        _worldDragHoverTarget = target;

        if (_worldDragHoverTarget != null)
            _worldDragHoverTarget.SetHighlight(true);
    }

    private void EndWorldItemDrag()
    {
        ToolbeltItemView item = _draggedToolbeltItem;
        bool wasDrag = _dragThresholdCrossed;

        if (_worldDragGhost != null)
            _worldDragGhost.gameObject.SetActive(false);

        if (_worldDragHoverTarget != null)
        {
            _worldDragHoverTarget.SetHighlight(false);
            _worldDragHoverTarget = null;
        }

        _draggedToolbeltItem = null;
        _dragThresholdCrossed = false;

        if (item == null) return;

        if (!wasDrag)
        {
            item.OnClicked();
            return;
        }

        Vector2 releasePoint = MouseToWorld();
        Interactable target = GetInteractableAtWorld(releasePoint);
        if (target != null)
            BeginWalkToUseItem(item.Data, target);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Called by InventorySlotUI when a Pack item is dropped over the world. Walks Nina to
    /// the Interactable under the drop point and, on arrival, offers the item to it via
    /// Interactable.TryUseItem. No-ops if nothing valid is under the drop point.
    /// </summary>
    public void TryUseItemAtScreenPoint(InteractableData item, Vector2 screenPos)
    {
        Interactable view = GetInteractableAtScreenPoint(screenPos);
        if (view == null || item == null) return;

        BeginWalkToUseItem(item, view);
    }

    /// <summary>
    /// Shared by both drag sources (Pack UI and Toolbelt world items) once each has
    /// resolved its own target — walks Nina to it and offers the item on arrival.
    /// </summary>
    private void BeginWalkToUseItem(InteractableData item, Interactable view)
    {
        _pendingItem = item;
        _pendingView = view;

        if (_cycledView != null)
        {
            _cycledView.SetHighlight(false);
            _cycledView.SetLabel(false);
            _cycledView = null;
        }

        if (NavigationManager.Instance.CurrentGround == null) return;

        List<Vector2> path = NavigationManager.Instance.CurrentGround.FindPath(_nina.transform.position, view.InteractionPosition);
        MoveAlongPath(path, OnNinaArrivedWithItem);
    }

    private void OnNinaArrivedWithItem()
    {
        if (_pendingView == null) { _pendingItem = null; return; }

        Interactable view = _pendingView;
        InteractableData item = _pendingItem;
        _pendingView = null;
        _pendingItem = null;

        view.SetHighlight(false);
        view.SetLabel(false);
        if (_hoveredView == view) _hoveredView = null;

        view.TryUseItem(item);
    }
    
    /// <summary>Converts a screen point to the Interactable under it, or null. Used by
    /// InventorySlotUI's drag-drop to hit-test the world from a UI event.</summary>
    public Interactable GetInteractableAtScreenPoint(Vector2 screenPos)
    {
        if (_camera == null) return null;

        Vector3 screen = screenPos;
        screen.z = Mathf.Abs(_camera.transform.position.z);
        Vector2 worldPos = _camera.ScreenToWorldPoint(screen);
        return GetInteractableAtWorld(worldPos);
    }
    
    /// <summary>Converts the current mouse screen position to world space.</summary>
    private Vector2 MouseToWorld()
    {
        Vector3 screen = Mouse.current.position.ReadValue();
        screen.z = Mathf.Abs(_camera.transform.position.z);
        return _camera.ScreenToWorldPoint(screen);
    }

    /// <summary>
    /// Returns the <see cref="Interactable"/> whose PolygonCollider2D
    /// contains <paramref name="worldPos"/>, or null if none.
    /// </summary>
    private Interactable GetInteractableAtWorld(Vector2 worldPos)
    {
        foreach (Interactable view in _sceneInteractables)
        {
            if (view == null) continue;
           
            PolygonCollider2D poly = view._polygonCollider;
            if (poly.OverlapPoint(worldPos))
                return view;
        }
        return null;
    }

    /// <summary>Gathers interactables specifically inside the current area's container.</summary>
    public void RefreshInteractables(Transform container = null)
    {
        _sceneInteractables.Clear();

        if (container != null)
        {
            // Only fetch interactables inside this specific area GameObject!
            Interactable[] found = container.GetComponentsInChildren<Interactable>(false);
            _sceneInteractables.AddRange(found);
        }
        else
        {
            // Fallback for scene-wide search if no container was passed
            Interactable[] found = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
            _sceneInteractables.AddRange(found);
        }
    }

    #endregion
}

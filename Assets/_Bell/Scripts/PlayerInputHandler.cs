using System.Collections.Generic;
using UnityEngine;
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
/// is called. While the menu is open, face buttons (Gamepad) or the interaction
/// buttons in <see cref="InteractionMenu"/> handle the rest — this script defers
/// to the existing <see cref="InteractionMenu"/> for that.
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
/// </summary>
[DisallowMultipleComponent]
public class PlayerInputHandler : MonoBehaviour
{
    // Inspector 
    [Header("References")]
    [SerializeField] private NinaController _nina;
    [SerializeField] private Camera _camera;

    [Header("Scene Interactables")]
    [SerializeField, Tooltip("Leave empty — auto-populated at runtime")]
    private List<Interactable> _sceneInteractables = new List<Interactable>();

    // Input Actions 
    private InputSystem_Actions _actions;
    private InputAction _actClick;
    private InputAction _actCycleNext;
    private InputAction _actCyclePrev;
    private InputAction _actConfirm;
    private InputAction _actMenuExamine;
    private InputAction _actMenuInteract;
    private InputAction _actMenuPickUp;
    private InputAction _actCancel;

    // Runtime state 
    private int _cycleIndex = -1;
    private Interactable _hoveredView;
    private Interactable _cycledView;
    private Interactable _pendingView;
    private bool _menuOpen = false;
    private Queue<Vector2> _currentPath = new Queue<Vector2>();
    private System.Action _onPathComplete;
    
    #region Unity lifecycle

    void Awake()
    {
        if (_nina == null)
            Debug.LogError("[PlayerInputHandler] NinaController not assigned.", this);

        if (_camera == null)
            _camera = Camera.main;

        _actions = new InputSystem_Actions();

        _actClick        = _actions.Player.Click;
        _actCycleNext    = _actions.Player.CycleNext;
        _actCyclePrev    = _actions.Player.CyclePrev;
        _actConfirm      = _actions.Player.Confirm;
        _actMenuExamine  = _actions.Player.MenuExamine;
        _actMenuInteract = _actions.Player.MenuInteract;
        _actMenuPickUp   = _actions.Player.MenuPickUp;
        _actCancel       = _actions.Player.Cancel;
    }

    void OnEnable()
    {
        _actions.Enable();

        _actClick.performed        += OnClick;
        _actConfirm.performed      += OnConfirm;
        _actCycleNext.performed    += OnCycleNext;
        _actCyclePrev.performed    += OnCyclePrev;
        _actMenuExamine.performed  += OnMenuExamine;
        _actMenuInteract.performed += OnMenuInteract;
        _actMenuPickUp.performed   += OnMenuPickUp;
        _actCancel.performed       += OnCancel;

        RefreshInteractables();
    }

    void OnDisable()
    {
        _actClick.performed        -= OnClick;
        _actConfirm.performed      -= OnConfirm;
        _actCycleNext.performed    -= OnCycleNext;
        _actCyclePrev.performed    -= OnCyclePrev;
        _actMenuExamine.performed  -= OnMenuExamine;
        _actMenuInteract.performed -= OnMenuInteract;
        _actMenuPickUp.performed   -= OnMenuPickUp;
        _actCancel.performed       -= OnCancel;

        _actions.Disable();
    }

    void Update()
    {
        if (_menuOpen) return;

        UpdateHoveredInteractable();
    }

    #endregion
    
    private void UpdateHoveredInteractable()
    {
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
        if (_menuOpen) return;

        // Cycle selection active — confirm it
        if (_cycledView != null)
        {
            NavigateTo(_cycledView);
            return;
        }

        // Mouse: use world position under cursor
        if (_camera == null || Mouse.current == null) return;

        Vector2 worldPoint = MouseToWorld();

        Interactable view = GetInteractableAtWorld(worldPoint);
        if (view != null)
        {
            NavigateTo(view);
            return;
        }

        // Ground walk — only if the click lands inside the walkable polygon.
        if (GroundBounds.Instance == null) return;
        // if (!GroundBounds.Instance.IsOnGround(worldPoint)) return;
        Vector2 walkTarget = GroundBounds.Instance.IsOnGround(worldPoint)
            ? worldPoint
            : GroundBounds.Instance.ClosestPointOnBoundary(worldPoint);

        List<Vector2> path = GroundBounds.Instance.FindPath(_nina.transform.position, walkTarget);
        MoveAlongPath(path);
    }

    /// <summary>Enter key — same as click for KBM cycling (mode 2).</summary>
    private void OnConfirm(InputAction.CallbackContext ctx) => OnClick(ctx);

    private void OnCycleNext(InputAction.CallbackContext ctx) => CycleInteractable(+1);
    private void OnCyclePrev(InputAction.CallbackContext ctx) => CycleInteractable(-1);

    private void OnMenuExamine(InputAction.CallbackContext ctx)
    {
        if (!_menuOpen) return;
        InteractionMenu.Instance.examineButton.onClick.Invoke();
    }

    private void OnMenuInteract(InputAction.CallbackContext ctx)
    {
        if (!_menuOpen) return;
        InteractionMenu.Instance.interactButton.onClick.Invoke();
    }

    private void OnMenuPickUp(InputAction.CallbackContext ctx)
    {
        if (!_menuOpen) return;
        InteractionMenu.Instance.pickUpButton.onClick.Invoke();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (_menuOpen)
        {
            InteractionMenu.Instance.CloseMenu();
            _menuOpen = false;
            return;
        }

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
    }

    #endregion
    
    #region Navigation

    private void NavigateTo(Interactable view)
    {
        _pendingView = view;

        if (_cycledView != null && _cycledView != view)
        {
            _cycledView.SetHighlight(false);
            _cycledView.SetLabel(false);
            _cycledView = null;
        }
        
        if (GroundBounds.Instance == null) return;

        List<Vector2> path = GroundBounds.Instance.FindPath(_nina.transform.position, view.InteractionPosition);
        MoveAlongPath(path, OnNinaArrived);
    }

    private void OnNinaArrived()
    {
        if (_pendingView == null) return;

        _menuOpen = true;
        
        _pendingView.OnClick();
        _pendingView.SetHighlight(false);
        _pendingView = null;
    }

    // Poll for menu close (InteractionMenu has no close event)
    private void LateUpdate()
    {
        if (!_menuOpen) return;
        if (InteractionMenu.Instance == null) return;

        if (!InteractionMenu.Instance.menuContainer.activeSelf)
        {
            _menuOpen = false;
            ClearSelection();
        }
    }
    
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

    #region Helpers

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

    /// <summary>Gathers all <see cref="Interactable"/> objects currently in the scene.</summary>
    public void RefreshInteractables()
    {
        _sceneInteractables.Clear();
        Interactable[] found = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
        _sceneInteractables.AddRange(found);
    }

    #endregion
}
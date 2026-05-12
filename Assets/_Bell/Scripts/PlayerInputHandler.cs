using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Handles all player input using the New Input System (PlayerActions action map).
///
/// Supported modes (auto-detected from last device used):
///   KBM-1  Mouse cursor → Left Click to select → character walks → menu appears
///   KBM-2  Arrow keys cycle interactables → Left Click or Enter to confirm
///   PAD-3  Right stick moves cursor → Right Trigger to confirm
///   PAD-4  Left stick cycles interactables → Right Trigger to confirm
///
/// When the character arrives at the selected interactable, <see cref="InteractableView.OpenMenu"/>
/// is called. While the menu is open, face buttons (Gamepad) or the interaction
/// buttons in <see cref="InteractionMenu"/> handle the rest — this script defers
/// to the existing <see cref="InteractionMenu"/> for that.
///
/// Setup requirements
/// ──────────────────
/// • Input Actions asset with an action map called "Player" containing:
///     Click          (Button)   — left mouse button / right trigger
///     CursorDelta    (Vector2)  — mouse delta / right stick
///     CycleNext      (Button)   — right arrow / right d-pad / right stick push
///     CyclePrev      (Button)   — left arrow / left d-pad / left stick push
///     Confirm        (Button)   — Enter key (alias of Click for KBM)
///     MenuExamine    (Button)   — X / square
///     MenuInteract   (Button)   — Y / triangle
///     MenuPickUp     (Button)   — B / circle
///     Cancel         (Button)   — Escape / circle / B
///
/// • A child GameObject named "Cursor" on this same GameObject (or assign via Inspector)
/// • <see cref="NinaController"/> reference
/// • <see cref="Camera"/> reference (defaults to Camera.main)
/// </summary>
[DisallowMultipleComponent]
public class PlayerInputHandler : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private NinaController _nina;
    [SerializeField] private Transform      _cursorTransform;
    [SerializeField] private Camera         _camera;

    [Header("Cursor Settings")]
    [SerializeField, Tooltip("How fast the analog stick moves the cursor (world units/sec)")]
    private float _stickCursorSpeed = 8f;

    [Header("Scene Interactables")]
    [SerializeField, Tooltip("Leave empty — auto-populated at runtime")]
    private List<InteractableView> _sceneInteractables = new List<InteractableView>();

    // ── Input Actions ────────────────────────────────────────────────────────
    private InputSystem_Actions _actions;                 // generated class from Input Actions asset

    // Action references cached to avoid string lookups every frame
    private InputAction _actClick;
    private InputAction _actCursorDelta;
    private InputAction _actCycleNext;
    private InputAction _actCyclePrev;
    private InputAction _actConfirm;
    private InputAction _actMenuExamine;
    private InputAction _actMenuInteract;
    private InputAction _actMenuPickUp;
    private InputAction _actCancel;

    // ── Runtime state ────────────────────────────────────────────────────────
    private int              _cycleIndex      = -1;   // -1 = no selection
    private InteractableView _hoveredView;             // under cursor (mouse/stick)
    private InteractableView _cycledView;              // selected via cycle keys
    private InteractableView _pendingView;             // walking toward this
    private bool             _menuOpen        = false;
    private bool             _usingStickCursor = false; // right-stick cursor active

    // ────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        // Validate references
        if (_nina == null)
            Debug.LogError("[PlayerInputHandler] NinaController not assigned.", this);

        if (_camera == null)
            _camera = Camera.main;

        if (_cursorTransform == null)
            Debug.LogWarning("[PlayerInputHandler] No cursor transform assigned. Cursor won't move.", this);

        // Build action map
        _actions = new InputSystem_Actions();

        _actClick        = _actions.Player.Click;
        _actCursorDelta  = _actions.Player.CursorDelta;
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

        _actClick.performed       += OnClick;
        _actConfirm.performed     += OnConfirm;
        _actCycleNext.performed   += OnCycleNext;
        _actCyclePrev.performed   += OnCyclePrev;
        _actMenuExamine.performed += OnMenuExamine;
        _actMenuInteract.performed+= OnMenuInteract;
        _actMenuPickUp.performed  += OnMenuPickUp;
        _actCancel.performed      += OnCancel;

        // Populate interactables list from scene
        RefreshInteractables();
    }

    void OnDisable()
    {
        _actClick.performed       -= OnClick;
        _actConfirm.performed     -= OnConfirm;
        _actCycleNext.performed   -= OnCycleNext;
        _actCyclePrev.performed   -= OnCyclePrev;
        _actMenuExamine.performed -= OnMenuExamine;
        _actMenuInteract.performed-= OnMenuInteract;
        _actMenuPickUp.performed  -= OnMenuPickUp;
        _actCancel.performed      -= OnCancel;

        _actions.Disable();
    }

    // ────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (_menuOpen) return;

        HandleCursorMovement();
        UpdateHoveredInteractable();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Cursor & hover
    // ────────────────────────────────────────────────────────────────────────

    private void HandleCursorMovement()
    {
        if (_cursorTransform == null || _camera == null) return;

        Vector2 delta = _actCursorDelta.ReadValue<Vector2>();

        // Detect if input is coming from mouse or gamepad stick
        bool fromMouse = Mouse.current != null &&
                         _actCursorDelta.activeControl != null &&
                         _actCursorDelta.activeControl.device is Mouse;

        if (fromMouse)
        {
            // Mirror actual mouse position in world space
            Vector3 screenPos = Mouse.current.position.ReadValue();
            screenPos.z = Mathf.Abs(_camera.transform.position.z);
            _cursorTransform.position = _camera.ScreenToWorldPoint(screenPos);
            _usingStickCursor = false;
        }
        else if (delta.sqrMagnitude > 0.01f)
        {
            // Analog stick: translate cursor in world space
            _cursorTransform.position += (Vector3)(delta * (_stickCursorSpeed * Time.deltaTime));
            _usingStickCursor = true;
        }
    }

    private void UpdateHoveredInteractable()
    {
        // Only run cursor hover when not cycling via keys/left-stick
        if (_cycledView != null) return;

        if (_cursorTransform == null) return;

        InteractableView newHover = GetInteractableAtWorld(_cursorTransform.position);

        if (newHover != _hoveredView)
        {
            if (_hoveredView != null) _hoveredView.SetHighlight(false);
            _hoveredView = newHover;
            if (_hoveredView != null) _hoveredView.SetHighlight(true);
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Input callbacks
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>Left mouse button (KBM modes 1 & 2) or Right Trigger (gamepad mode 3 & 4).</summary>
    private void OnClick(InputAction.CallbackContext ctx)
    {
        if (_menuOpen) return;

        // If there's a cycle selection active, confirm it
        if (_cycledView != null)
        {
            NavigateTo(_cycledView);
            return;
        }

        // Otherwise use cursor position
        if (_cursorTransform == null) return;

        // Did we hit an interactable?
        InteractableView view = GetInteractableAtWorld(_cursorTransform.position);
        if (view != null)
        {
            NavigateTo(view);
            return;
        }

        // Hit the ground — navigate there (no callback)
        Vector2 worldPoint = _cursorTransform.position;
        if (GroundBounds.Instance != null)
            worldPoint = GroundBounds.Instance.ClampToGround(worldPoint);

        _nina.MoveTo(worldPoint);
    }

    /// <summary>Enter key — same as click when using keyboard cycling (KBM mode 2).</summary>
    private void OnConfirm(InputAction.CallbackContext ctx)
    {
        // Delegate to OnClick — identical behaviour
        OnClick(ctx);
    }

    private void OnCycleNext(InputAction.CallbackContext ctx) => CycleInteractable(+1);
    private void OnCyclePrev(InputAction.CallbackContext ctx) => CycleInteractable(-1);

    private void OnMenuExamine(InputAction.CallbackContext ctx)
    {
        if (!_menuOpen) return;
        // The existing InteractionMenu uses Buttons with onClick listeners.
        // On gamepad we simulate pressing the examine button.
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

        // Cancel pending navigation
        if (_nina != null) _nina.CancelMovement();
        ClearSelection();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Cycling
    // ────────────────────────────────────────────────────────────────────────

    private void CycleInteractable(int direction)
    {
        if (_sceneInteractables.Count == 0) return;

        // Clear cursor hover so the two modes don't conflict
        if (_hoveredView != null) _hoveredView.SetHighlight(false);
        _hoveredView = null;

        _cycleIndex = (_cycleIndex + direction + _sceneInteractables.Count)
                      % _sceneInteractables.Count;

        // Unhighlight previous
        if (_cycledView != null) _cycledView.SetHighlight(false);

        _cycledView = _sceneInteractables[_cycleIndex];
        _cycledView.SetHighlight(true);

        // Move cursor visual to the selected interactable
        if (_cursorTransform != null)
            _cursorTransform.position = _cycledView.InteractionPosition;
    }

    private void ClearSelection()
    {
        if (_cycledView != null) _cycledView.SetHighlight(false);
        _cycledView  = null;
        _cycleIndex  = -1;
        _pendingView = null;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Navigation
    // ────────────────────────────────────────────────────────────────────────

    private void NavigateTo(InteractableView view)
    {
        _pendingView = view;

        // Unhighlight cycle selection; the highlight stays on the pending view
        // so the player knows where they're going.
        if (_cycledView != null && _cycledView != view)
            _cycledView.SetHighlight(false);
        _cycledView = null;

        _nina.MoveTo(view.InteractionPosition, OnNinaArrived);
    }

    private void OnNinaArrived()
    {
        if (_pendingView == null) return;

        _menuOpen = true;
        _pendingView.OpenMenu();
        // Unhighlight — the menu is open, the object is "active" now
        _pendingView.SetHighlight(false);
        _pendingView = null;

        // Subscribe to menu-closed event so we can reset _menuOpen
        // InteractionMenu.CloseMenu is called from the menu itself;
        // we hook in via the Update poll below.
    }

    // Poll for menu close (InteractionMenu has no close event — add one if desired)
    private void LateUpdate()
    {
        if (!_menuOpen) return;
        if (InteractionMenu.Instance == null) return;

        // InteractionMenu.menuContainer.activeSelf goes false when closed
        if (!InteractionMenu.Instance.menuContainer.activeSelf)
        {
            _menuOpen = false;
            ClearSelection();
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the <see cref="InteractableView"/> whose PolygonCollider2D
    /// contains <paramref name="worldPos"/>, or null if none.
    /// </summary>
    private InteractableView GetInteractableAtWorld(Vector2 worldPos)
    {
        foreach (InteractableView view in _sceneInteractables)
        {
            if (view == null) continue;

            PolygonCollider2D poly;
            if (view.TryGetComponent(out poly) && poly.OverlapPoint(worldPos))
                return view;
        }
        return null;
    }

    /// <summary>Gathers all <see cref="InteractableView"/> objects currently in the scene.</summary>
    public void RefreshInteractables()
    {
        _sceneInteractables.Clear();
        InteractableView[] found = FindObjectsByType<InteractableView>(FindObjectsSortMode.None);
        _sceneInteractables.AddRange(found);
    }
}

# BellCaster Character Controller – Setup Guide

## Scripts in this package

| Script | GameObject | Purpose |
|--------|-----------|---------|
| `NinaController.cs` | Nina (player) | Movement only – no input, no physics |
| `InteractableView.cs` | Each interactable | Outline, interaction point, opens menu on arrival |
| `GroundBounds.cs` | "Ground" (scene singleton) | EdgeCollider2D walkable floor + clamp utility |
| `PlayerInputHandler.cs` | Any persistent manager GO | All input modes, cursor, cycling |

---

## 1. Nina GameObject

```
Nina
├── SpriteRenderer
├── Animator          (needs bool param "IsWalking")
├── NinaController    ← add this
└── (NO Rigidbody)
```

---

## 2. Each Interactable GameObject

The interactable already has `Interactable.cs`. Add:

```
MyInteractable
├── Interactable          (existing)
├── PolygonCollider2D     ← shape this to the sprite boundary in the Inspector
│     isTrigger = true
├── LineRenderer          ← auto-configured by InteractableView
└── InteractableView      ← add this; assign optional InteractionPoint child
```

`InteractionPoint` is an **empty child Transform** placed where Nina should stand
(e.g. the base of the object). If omitted, Nina walks to the object's pivot.

---

## 3. Ground GameObject

```
Ground
└── EdgeCollider2D    ← draw the floor path; isTrigger forced true at runtime
    GroundBounds      ← singleton, auto-finds the EdgeCollider2D
```

Shape the EdgeCollider2D to follow the walkable floor of your scene (left edge →
right edge). GroundBounds.ClampToGround() snaps any point to this path.

---

## 4. Input Actions Asset

Create a new **Input Actions** asset (`PlayerActions.inputactions`) with:

**Action Map: `Player`**

| Action | Type | KBM Binding | Gamepad Binding |
|--------|------|------------|-----------------|
| `Click` | Button | Left Mouse Button | Right Trigger |
| `CursorDelta` | Value (Vector2) | Mouse Position (delta) | Right Stick |
| `CycleNext` | Button | Right Arrow, D Right | Left Stick Right, D-Pad Right |
| `CyclePrev` | Button | Left Arrow, D Left | Left Stick Left, D-Pad Left |
| `Confirm` | Button | Enter | Right Trigger (alias of Click) |
| `MenuExamine` | Button | *(unused KBM – mouse clicks button)* | Square / X |
| `MenuInteract` | Button | *(unused KBM)* | Triangle / Y |
| `MenuPickUp` | Button | *(unused KBM)* | Circle / B |
| `Cancel` | Button | Escape | Circle / B |

Enable **Generate C# Class** on the asset and set the class name to `PlayerActions`.

---

## 5. PlayerInputHandler GameObject

Attach `PlayerInputHandler.cs` to any scene manager object. Assign:

- **Nina** → the NinaController component
- **Cursor Transform** → a child GameObject ("Cursor") with a sprite you want
  shown as the on-screen cursor. Hide the system cursor with:
  `Cursor.visible = false;` in a startup script if desired.
- **Camera** → leave blank to auto-use Camera.main

`RefreshInteractables()` is called automatically on `OnEnable`. If you spawn
interactables at runtime, call it again.

---

## 6. InteractionMenu

No changes needed. The existing `InteractionMenu.cs` is used as-is. The only
requirement is that `InteractionMenu.Instance.menuContainer` is the GameObject
toggled by `ShowMenu` / `CloseMenu` (it already is).

---

## Input mode summary

| # | Device | Navigate | Confirm | Interaction buttons |
|---|--------|----------|---------|---------------------|
| 1 | KBM | Mouse cursor | Left Click | Mouse click on UI buttons |
| 2 | KBM | Arrow keys | Left Click / Enter | Mouse click on UI buttons |
| 3 | Gamepad | Right Stick cursor | Right Trigger | Square/Triangle/Circle/Cross |
| 4 | Gamepad | Left Stick / D-Pad cycling | Right Trigger | Square/Triangle/Circle/Cross |

Modes 1 & 2 coexist automatically (last active device detection).
Modes 3 & 4 coexist automatically.

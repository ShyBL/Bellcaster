using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author-controlled, ordered list of every souvenir that exists in the
/// game — found or not. JournalPanelController renders one slot per entry
/// here, cross-referenced against InventoryManager.CollectedSouvenirs to
/// decide locked vs. unlocked. One asset per project, assigned manually on
/// JournalPanelController rather than looked up via AssetDatabase, so this
/// also works correctly in a build.
/// </summary>
[CreateAssetMenu(fileName = "SouvenirRegistry", menuName = "Bell/Souvenir Registry")]
public class SouvenirRegistry : ScriptableObject
{
    [Tooltip("Every souvenir in the game, in the order slots should display. Drag InteractableData assets in manually.")]
    public List<InteractableData> AllSouvenirs = new List<InteractableData>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var entry in AllSouvenirs)
        {
            if (entry != null && entry.pickupDestination != PickupDestination.Souvenir)
            {
                Debug.LogWarning(
                    $"[SouvenirRegistry] '{entry.objectName}' is listed here but its " +
                    "PickupDestination isn't set to Souvenir.", this);
            }
        }
    }
#endif
}
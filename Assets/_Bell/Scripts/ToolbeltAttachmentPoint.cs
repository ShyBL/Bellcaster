using UnityEngine;

/// <summary>
/// Lives on a GameObject that already has a Spine BoneFollower component
/// (Spine.Unity.BoneFollower) pointed at the correct bone on Nina's rig —
/// this class doesn't touch the Spine API at all, it only owns whichever
/// ToolbeltItemView is currently equipped at this socket.
///
/// Setup in the Inspector: add a Spine.Unity.BoneFollower component to this
/// same GameObject, assign its Skeleton Renderer to Nina's, and pick the
/// bone name (e.g. her belt bone for Socket = Belt, a shoulder/back bone
/// for Socket = Back). ToolbeltManager finds the right
/// ToolbeltAttachmentPoint by socket and calls Attach() on it.
/// </summary>
public class ToolbeltAttachmentPoint : MonoBehaviour
{
    [SerializeField] private ToolbeltSocket _socket;
    public ToolbeltSocket Socket => _socket;

    public ToolbeltItemView CurrentView { get; private set; }

    /// <summary>Spawns the shared item-view prefab here (replacing whatever's already attached) and binds it to the equipped item's data.</summary>
    public void Attach(InteractableData item, ToolbeltItemView viewPrefab)
    {
        if (CurrentView != null)
        {
            Destroy(CurrentView.gameObject);
            CurrentView = null;
        }

        if (item == null || viewPrefab == null) return;

        CurrentView = Instantiate(viewPrefab, transform);
        CurrentView.transform.localPosition = Vector3.zero;
        CurrentView.transform.localRotation = Quaternion.identity;
        CurrentView.Bind(item);
    }
}

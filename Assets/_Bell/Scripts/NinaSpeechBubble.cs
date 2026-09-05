using UnityEngine;
using TMPro;

/// <summary>
/// Displays Nina's reaction lines — repeat-click, idle, wrong-item, and
/// self-click quips. Kept separate from ExamineTextDisplay: that panel
/// reads object descriptions in a neutral tone, this one is Nina speaking
/// in the first person, so it gets its own visual language.
///
/// Expected setup: this GameObject lives under a World Space Canvas already
/// parented to Nina (same pattern as Interactable's hover label), so no
/// runtime position-following code is needed here — it's just shown/hidden
/// and its text swapped.
/// </summary>
public class NinaSpeechBubble : MonoBehaviour
{
    public static NinaSpeechBubble Instance;

    [Header("References")]
    [SerializeField] private GameObject _bubbleRoot;
    [SerializeField] private TextMeshProUGUI _bubbleText;

    [Header("Settings")]
    [SerializeField] private float _displayDuration = 3f;

    [Header("Quip Content")]
    [SerializeField] private NinaQuipBank _quipBank;

    private float _hideTimer;
    private bool _isShowing;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Hide();
    }

    void Update()
    {
        if (!_isShowing) return;

        _hideTimer -= Time.deltaTime;
        if (_hideTimer <= 0f) Hide();
    }

    /// <summary>Shows a specific line — used for the item-specific first wrong-item response.</summary>
    public void Show(string text, AudioClip vo = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (_bubbleText != null) _bubbleText.text = text;
        if (_bubbleRoot != null) _bubbleRoot.SetActive(true);

        _isShowing = true;
        _hideTimer = _displayDuration;

        if (vo != null)
            AudioSource.PlayClipAtPoint(vo, transform.position);
    }

    /// <summary>Shows a random line from one of NinaQuipBank's categories.</summary>
    public void ShowCategory(NinaQuipCategory category)
    {
        if (_quipBank == null) return;

        NinaQuipBank.QuipLine line = _quipBank.GetRandomLine(category);
        if (line == null) return;

        Show(line.text, line.vo);
    }

    private void Hide()
    {
        _isShowing = false;
        if (_bubbleRoot != null) _bubbleRoot.SetActive(false);
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author-controlled bank of Nina's reaction lines, grouped by trigger
/// category. NinaSpeechBubble holds the one instance of this in the scene
/// and picks from it. Each entry pairs text with an optional VO clip, same
/// shape as InteractableData's examineText/examineVO pairing.
/// </summary>
[CreateAssetMenu(fileName = "NinaQuipBank", menuName = "Bell/Nina Quip Bank")]
public class NinaQuipBank : ScriptableObject
{
    [System.Serializable]
    public class QuipLine
    {
        [TextArea(1, 3)]
        public string text;
        public AudioClip vo;
    }

    [Header("Repeat Click")]
    [Tooltip("Shown when the player clicks the same object several times in a row.")]
    public List<QuipLine> repeatClickLines = new List<QuipLine>();

    [Header("Idle")]
    [Tooltip("Shown after Nina hasn't received any input for a while.")]
    public List<QuipLine> idleLines = new List<QuipLine>();

    [Header("Wrong Item (Fallback)")]
    [Tooltip("Shown on the 2nd+ failed attempt to use an item on the same object, once that object's own wrongItemText has already been shown once.")]
    public List<QuipLine> wrongItemFallbackLines = new List<QuipLine>();

    [Header("Self Click")]
    [Tooltip("Shown when the player clicks Nina herself.")]
    public List<QuipLine> selfClickLines = new List<QuipLine>();

    // Tracks the last index shown per category so the same line never repeats twice in a row.
    private readonly Dictionary<NinaQuipCategory, int> _lastIndexShown = new Dictionary<NinaQuipCategory, int>();

    public QuipLine GetRandomLine(NinaQuipCategory category)
    {
        List<QuipLine> pool = GetPool(category);
        if (pool == null || pool.Count == 0) return null;
        if (pool.Count == 1) return pool[0];

        int lastIndex = _lastIndexShown.TryGetValue(category, out var idx) ? idx : -1;
        int index;
        do
        {
            index = Random.Range(0, pool.Count);
        } while (index == lastIndex);

        _lastIndexShown[category] = index;
        return pool[index];
    }

    private List<QuipLine> GetPool(NinaQuipCategory category)
    {
        switch (category)
        {
            case NinaQuipCategory.RepeatClick:        return repeatClickLines;
            case NinaQuipCategory.Idle:                return idleLines;
            case NinaQuipCategory.WrongItemFallback:   return wrongItemFallbackLines;
            case NinaQuipCategory.SelfClick:           return selfClickLines;
            default:                                   return null;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExamineTextDisplay : MonoBehaviour
{
    public static ExamineTextDisplay Instance;

    [Header("UI References")] public GameObject textPanel;
    public TextMeshProUGUI examineText;
    public Animator textAnimator; // Optional: for custom animations

    [Header("Settings")] public float displayDuration = 3f; // How long text stays visible
    public bool autoHide = true; // Auto-hide after duration

    private float displayTimer = 0f;
    private bool isDisplaying = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideText();
    }

    void Update()
    {
        if (isDisplaying && autoHide)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0)
            {
                HideText();
            }
        }

        // Allow manual close with click or key
        if (isDisplaying && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            HideText();
        }
    }

    public void ShowText(string text)
    {
        examineText.text = text;
        textPanel.SetActive(true);
        isDisplaying = true;
        displayTimer = displayDuration;

        // Trigger animation if animator is set
        if (textAnimator != null)
        {
            textAnimator.SetTrigger("Show");
        }

        Debug.Log($"Displaying examine text: {text}");
    }

    public void HideText()
    {
        // Trigger hide animation if animator is set
        if (textAnimator != null && isDisplaying)
        {
            textAnimator.SetTrigger("Hide");
        }

        textPanel.SetActive(false);
        isDisplaying = false;
        displayTimer = 0f;
    }
}
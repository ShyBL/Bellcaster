using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InteractionMenu : MonoBehaviour
{
    public static InteractionMenu Instance;
    
    [Header("UI References")]
    public GameObject menuContainer;
    public Button examineButton;
    public Button interactButton;
    public Button pickUpButton;
    
    [Header("Cross Settings")]
    public float buttonDistance = 100f; // Distance from center
    
    private bool isMenuOpen = false;
    private Interactable currentObject;
    private RectTransform menuRect;
    private Dictionary<InteractionType, Button> buttonMap;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        menuRect = menuContainer.GetComponent<RectTransform>();
        
        buttonMap = new Dictionary<InteractionType, Button>
        {
            { InteractionType.Examine, examineButton },
            { InteractionType.Interact, interactButton },
            { InteractionType.PickUp, pickUpButton }
        };
        
        examineButton.onClick.AddListener(() => OnButtonClicked(InteractionType.Examine));
        interactButton.onClick.AddListener(() => OnButtonClicked(InteractionType.Interact));
        pickUpButton.onClick.AddListener(() => OnButtonClicked(InteractionType.PickUp));
        
        CloseMenu();
    }

    void Update()
    {
        if (isMenuOpen && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
        {
            CloseMenu();
        }
    }

    public void ShowMenu(Interactable obj, Vector3 worldPosition, List<InteractionType> interactions)
    {
        if (interactions.Count == 0) return;
        
        currentObject = obj;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        menuRect.position = screenPos;
        
        UpdateButtonVisibility(interactions);
        ArrangeButtonsInCross();
        
        menuContainer.SetActive(true);
        isMenuOpen = true;
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        currentObject = null;
        menuContainer.SetActive(false);
    }

    void UpdateButtonVisibility(List<InteractionType> interactions)
    {
        examineButton.interactable = interactions.Contains(InteractionType.Examine);
        interactButton.interactable = interactions.Contains(InteractionType.Interact);
        pickUpButton.interactable = interactions.Contains(InteractionType.PickUp);
    }

    void ArrangeButtonsInCross()
    {
        // West - Examine (left)
        examineButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-buttonDistance, 0);
        
        // North - Interact (up)
        interactButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, buttonDistance);
        
        // East - PickUp (right)
        pickUpButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonDistance, 0);
    }

    void OnButtonClicked(InteractionType type)
    {
        if (currentObject == null) return;
        
        switch (type)
        {
            case InteractionType.Examine:
                currentObject.OnExamine();
                break;
            case InteractionType.Interact:
                currentObject.OnInteract();
                break;
            case InteractionType.PickUp:
                currentObject.OnPickUp();
                break;
        }
        
        CloseMenu();
    }
}
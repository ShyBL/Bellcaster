using UnityEngine;

[System.Serializable]
public enum InteractableTemplateType
{
    SimpleExamine,              // Just shows text (e.g., paintings, decorations)
    PickupToInventory,          // Goes to inventory bar (e.g., tools, keys)
    PickupToJournal,            // Goes to journal (e.g., notes, collectibles)
    BasicInteract,              // Changes scene state (e.g., switches, levers)
    ConditionalPickup,          // Requires world state to pick up (e.g., Lantern needs Chair)
    RequiresInventoryItem,      // Needs item to interact (e.g., Doorbell needs broken piece)
    CraftingStation,            // Opens crafting interface (e.g., Anvil)
    DialogueTrigger,            // Shows dialogue then updates state
    AchievementUnlock,          // Unlocks achievement on interaction
    Custom                      // Full manual control
}

public static class InteractableTemplates
{
    public static InteractableData ApplyTemplate(InteractableTemplateType template, string objectName = "New Interactable")
    {
        InteractableData data = ScriptableObject.CreateInstance<InteractableData>();
        data.objectName = objectName;
        
        switch (template)
        {
            case InteractableTemplateType.SimpleExamine:
                ConfigureSimpleExamine(data);
                break;
                
            case InteractableTemplateType.PickupToInventory:
                ConfigurePickupToInventory(data);
                break;
                
            case InteractableTemplateType.PickupToJournal:
                ConfigurePickupToJournal(data);
                break;
                
            case InteractableTemplateType.BasicInteract:
                ConfigureBasicInteract(data);
                break;
                
            case InteractableTemplateType.ConditionalPickup:
                ConfigureConditionalPickup(data);
                break;
                
            case InteractableTemplateType.RequiresInventoryItem:
                ConfigureRequiresInventoryItem(data);
                break;
                
            case InteractableTemplateType.CraftingStation:
                ConfigureCraftingStation(data);
                break;
                
            case InteractableTemplateType.DialogueTrigger:
                ConfigureDialogueTrigger(data);
                break;
                
            case InteractableTemplateType.AchievementUnlock:
                ConfigureAchievementUnlock(data);
                break;
                
            case InteractableTemplateType.Custom:
                ConfigureCustom(data);
                break;
        }
        
        return data;
    }
    
    // ==================== Template Configurations ====================
    
    static void ConfigureSimpleExamine(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = $"It's a {data.objectName}.";
        data.canPickUp = false;
        data.canInteract = false;
    }
    
    static void ConfigurePickupToInventory(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = $"A {data.objectName}.";
        data.canPickUp = true;
        data.pickupDestination = PickupDestination.Inventory;
        data.canInteract = false;
    }
    
    static void ConfigurePickupToJournal(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = "An interesting note.";
        data.canPickUp = true;
        data.pickupDestination = PickupDestination.Journal;
        data.canInteract = false;
    }
    
    static void ConfigureBasicInteract(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = $"A {data.objectName}.";
        data.canPickUp = false;
        data.canInteract = true;
        data.interactResultState = ""; // User will fill this
    }
    
    static void ConfigureConditionalPickup(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = "I can't reach it from here.";
        data.canPickUp = true;
        data.pickupDestination = PickupDestination.Inventory;
        data.pickupRequirement = ""; // User will fill this
        data.canInteract = false;
    }
    
    static void ConfigureRequiresInventoryItem(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = "Something's missing here.";
        data.canPickUp = false;
        data.canInteract = true;
        data.requiredInventoryItem = ""; // User will fill this
        data.interactResultState = ""; // User will fill this
    }
    
    static void ConfigureCraftingStation(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = "A crafting station.";
        data.canPickUp = false;
        data.canInteract = true;
        data.interactResultState = ""; // Used to track crafting completion
    }
    
    static void ConfigureDialogueTrigger(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = "...";
        data.canPickUp = true;
        data.pickupDestination = PickupDestination.Journal;
        data.canInteract = false;
    }
    
    static void ConfigureAchievementUnlock(InteractableData data)
    {
        data.canExamine = true;
        data.examineText = "A hidden secret.";
        data.canPickUp = true;
        data.pickupDestination = PickupDestination.Inventory;
        data.canInteract = true;
        data.interactResultState = ""; // Achievement ID
    }
    
    static void ConfigureCustom(InteractableData data)
    {
        // Leave everything at defaults, user configures manually
        data.canExamine = true;
        data.examineText = "";
    }
    
    // ==================== Template Descriptions ====================
    
    public static string GetTemplateDescription(InteractableTemplateType template)
    {
        switch (template)
        {
            case InteractableTemplateType.SimpleExamine:
                return "Shows examine text only. No pickup or interaction.";
                
            case InteractableTemplateType.PickupToInventory:
                return "Can be examined and picked up to inventory.";
                
            case InteractableTemplateType.PickupToJournal:
                return "Can be examined and added to journal (notes, collectibles).";
                
            case InteractableTemplateType.BasicInteract:
                return "Interact to change world state (switches, doors).";
                
            case InteractableTemplateType.ConditionalPickup:
                return "Requires world state condition before pickup is available.";
                
            case InteractableTemplateType.RequiresInventoryItem:
                return "Needs specific inventory item to interact.";
                
            case InteractableTemplateType.CraftingStation:
                return "Opens crafting interface (anvil, workbench).";
                
            case InteractableTemplateType.DialogueTrigger:
                return "Shows dialogue then adds to journal.";
                
            case InteractableTemplateType.AchievementUnlock:
                return "Unlocks hidden achievement when collected.";
                
            case InteractableTemplateType.Custom:
                return "Configure everything manually.";
                
            default:
                return "";
        }
    }
    
    // ==================== Workshop-Specific Helpers ====================
    
    public static InteractableData CreateGarricksNote()
    {
        var data = ApplyTemplate(InteractableTemplateType.PickupToJournal, "Garrick's Note");
        data.examineText = @"Nina
My dear daughter
It appears the king's men
had finally caught up with me
Please do not worry, I will be fine
I hid the most powerful magical bells
You can finish this latest one I started
Ring it, and it shall lead you to the others
Find all three of them, then come and find me
Love,
Papa";
        return data;
    }
    
    public static InteractableData CreateAnvil()
    {
        var data = ApplyTemplate(InteractableTemplateType.CraftingStation, "Anvil");
        data.examineText = "A sturdy anvil for bell casting.";
        data.interactResultState = "bellCrafted";
        return data;
    }
    
    public static InteractableData CreateRedCap()
    {
        var data = ApplyTemplate(InteractableTemplateType.PickupToInventory, "Red Cap");
        data.examineText = "A stylish red cap.";
        return data;
    }
    
    public static InteractableData CreateDoorBell()
    {
        var data = ApplyTemplate(InteractableTemplateType.AchievementUnlock, "Door Bell");
        data.examineText = "The broken doorbell. I could fix this.";
        data.interactResultState = "achievementAttentionToDetail";
        return data;
    }
    
    public static InteractableData CreateBellcastersHammer()
    {
        var data = ApplyTemplate(InteractableTemplateType.ConditionalPickup, "Bellcaster's Hammer");
        data.examineText = "It's protected by a magical orb.";
        data.pickupRequirement = "orbFaded";
        return data;
    }
}
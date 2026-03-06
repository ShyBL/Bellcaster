using System.Collections.Generic;
using UnityEngine;

public static class WorkshopPresets
{
    public static List<WorkshopInteractablePreset> GetAllPresets()
    {
        return new List<WorkshopInteractablePreset>
        {
            new WorkshopInteractablePreset 
            { 
                name = "Garrick's Note", 
                position = new Vector3(0, 0, 0),
                dataCreator = InteractableTemplates.CreateGarricksNote
            },
            new WorkshopInteractablePreset 
            { 
                name = "Anvil", 
                position = new Vector3(2, 0, 0),
                dataCreator = InteractableTemplates.CreateAnvil
            },
            new WorkshopInteractablePreset 
            { 
                name = "Red Cap", 
                position = new Vector3(-2, 1, 0),
                dataCreator = InteractableTemplates.CreateRedCap
            },
            new WorkshopInteractablePreset 
            { 
                name = "Door Bell", 
                position = new Vector3(-3, 2, 0),
                dataCreator = InteractableTemplates.CreateDoorBell
            },
            new WorkshopInteractablePreset 
            { 
                name = "Bellcaster's Hammer", 
                position = new Vector3(1, 1.5f, 0),
                dataCreator = InteractableTemplates.CreateBellcastersHammer
            }
        };
    }
}
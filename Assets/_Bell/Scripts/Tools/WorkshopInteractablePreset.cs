using UnityEngine;

[System.Serializable]
public class WorkshopInteractablePreset
{
    public string name;
    public Vector3 position;
    public System.Func<InteractableData> dataCreator;
}
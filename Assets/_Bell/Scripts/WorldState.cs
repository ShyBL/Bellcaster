using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{
    public static WorldState Instance;
    
    private Dictionary<string, bool> states = new Dictionary<string, bool>();
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void SetState(string key, bool value)
    {
        states[key] = value;
        Debug.Log($"WorldState: {key} = {value}");
    }
    
    public bool GetState(string key)
    {
        return states.ContainsKey(key) && states[key];
    }
    
    public bool CheckRequirement(string requirement)
    {
        if (string.IsNullOrEmpty(requirement)) return true;
        return GetState(requirement);
    }
}
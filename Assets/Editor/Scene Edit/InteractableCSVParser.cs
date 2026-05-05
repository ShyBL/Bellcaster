// ==================== InteractableCSVImporter.cs ====================
// Place in Assets/Editor/InteractableCSVImporter.cs
#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[System.Serializable]
public class InteractableCSVRow
{
    // Required
    public string object_name;
    public string template;
    
    // Template overrides (optional)
    public string can_examine;
    public string can_pickup;
    public string can_interact;
    
    // Examine
    public string examine_text;
    
    // Pickup
    public string pickup_destination; // 0 = Inventory, 1 = Journal
    public string pickup_requirement;
    
    // Interact
    public string required_inventory_item;
    public string interact_result_state;
    public string interact_result_object;
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(object_name) && !string.IsNullOrEmpty(template);
    }
    
    public bool? GetBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim().ToUpper();
        if (value == "TRUE" || value == "1") return true;
        if (value == "FALSE" || value == "0") return false;
        return null;
    }
    
    public int? GetInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (int.TryParse(value.Trim(), out int result)) return result;
        return null;
    }
}

public static class InteractableCSVParser
{
    public static List<InteractableCSVRow> ParseCSV(string csvContent)
    {
        List<InteractableCSVRow> rows = new List<InteractableCSVRow>();
        
        // Split into lines, handling different line endings
        string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        
        if (lines.Length < 2)
        {
            Debug.LogError("CSV must have at least a header row and one data row");
            return rows;
        }
        
        // Find header row (skip any instruction/comment lines at top)
        int headerIndex = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("object_name"))
            {
                headerIndex = i;
                break;
            }
        }
        
        if (headerIndex == -1)
        {
            Debug.LogError("Could not find header row starting with 'object_name'");
            return rows;
        }
        
        string[] headers = ParseCSVLine(lines[headerIndex]);
        
        // Parse data rows
        for (int i = headerIndex + 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            // Skip empty lines or comment lines
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#"))
                continue;
            
            // Skip separator lines
            if (line.StartsWith("=="))
                continue;
            
            string[] values = ParseCSVLine(line);
            
            if (values.Length == 0) continue;
            
            InteractableCSVRow row = new InteractableCSVRow();
            
            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                string header = headers[j].Trim().ToLower();
                string value = values[j].Trim();
                
                // Handle escaped newlines in text
                value = value.Replace("\\n", "\n");
                
                switch (header)
                {
                    case "object_name":
                        row.object_name = value;
                        break;
                    case "template":
                        row.template = value;
                        break;
                    case "can_examine":
                        row.can_examine = value;
                        break;
                    case "can_pickup":
                        row.can_pickup = value;
                        break;
                    case "can_interact":
                        row.can_interact = value;
                        break;
                    case "examine_text":
                        row.examine_text = value;
                        break;
                    case "pickup_destination":
                        row.pickup_destination = value;
                        break;
                    case "pickup_requirement":
                        row.pickup_requirement = value;
                        break;
                    case "required_inventory_item":
                        row.required_inventory_item = value;
                        break;
                    case "interact_result_state":
                        row.interact_result_state = value;
                        break;
                    case "interact_result_object":
                        row.interact_result_object = value;
                        break;
                }
            }
            
            if (row.IsValid())
            {
                rows.Add(row);
            }
            else
            {
                Debug.LogWarning($"Skipping invalid row at line {i + 1}: missing object_name or template");
            }
        }
        
        return rows;
    }
    
    // Simple CSV line parser that handles quoted fields with commas
    private static string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                // Handle escaped quotes
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField += '"';
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        // Add last field
        fields.Add(currentField);
        
        return fields.ToArray();
    }
}
#endif
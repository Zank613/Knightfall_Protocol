using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance; 
    
    private Dictionary<string, List<string>> dialogueData = new Dictionary<string, List<string>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllDialogue();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void LoadAllDialogue()
    {
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(""); 

        foreach (TextAsset ta in textAssets)
        {
            List<string> lines = new List<string>(ta.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries));
            
            dialogueData.Add(ta.name, lines);
            Debug.Log($"Loaded dialogue for: {ta.name} with {lines.Count} lines.");
        }
    }

    // Public method for characters to request a random line
    public string GetRandomLine(string key)
    {
        if (dialogueData.ContainsKey(key))
        {
            List<string> lines = dialogueData[key];
            if (lines.Count > 0)
            {
                // Return a random line from the list
                int randomIndex = Random.Range(0, lines.Count);
                return lines[randomIndex];
            }
        }
        
        return "ERROR: Dialogue not found."; 
    }
}
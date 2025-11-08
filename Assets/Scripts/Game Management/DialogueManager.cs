using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance; 
    private Dictionary<string, List<string>> dialogueData = new Dictionary<string, List<string>>();
    
    public GameObject dialogueBoxPrefab; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllDialogue();
        }
        else Destroy(gameObject);
    }

    private void LoadAllDialogue()
    {
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(""); 
        foreach (TextAsset ta in textAssets)
        {
            List<string> lines = new List<string>(ta.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries));
            dialogueData.Add(ta.name, lines);
        }
    }

    public string GetRandomLine(string key)
    {
        if (dialogueData.ContainsKey(key))
        {
            List<string> lines = dialogueData[key];
            if (lines.Count > 0) return lines[Random.Range(0, lines.Count)];
        }
        return "ERROR: Dialogue not found."; 
    }
    
    public static void SimplePopUp(Transform entity, string dialogueKey, System.Action onComplete)
    {
        if (Instance == null || Instance.dialogueBoxPrefab == null) return;
        
        string line = Instance.GetRandomLine(dialogueKey);
        if (string.IsNullOrEmpty(line) || line.Contains("ERROR")) return;

        GameObject dialogueBoxInstance = Instantiate(Instance.dialogueBoxPrefab, entity.position, Quaternion.identity);
        DialogueBoxController controller = dialogueBoxInstance.GetComponentInChildren<DialogueBoxController>();
        
        if (controller != null)
        {
            controller.Initialize(entity, line, onComplete);
        }
    }
}
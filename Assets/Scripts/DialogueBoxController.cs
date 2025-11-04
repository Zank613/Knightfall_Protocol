using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class DialogueBoxController : MonoBehaviour
{
    public TextMesh dialogueText; 
    
    public int charsPerLine = 20;
    public float destroyDelay = 2.0f;
    public float pageBreakDelay = 1.5f;
    public float charsPerSecond = 20f;   
    public Vector3 offset = new Vector3(0, 1.5f, 0); 
    
    private Transform entityToFollow;
    
    private List<string> dialoguePages = new List<string>(); 
    
    void Awake()
    {
        if (dialogueText == null)
        {
            dialogueText = GetComponentInChildren<TextMesh>();
        }
        if (dialogueText == null)
        {
            Debug.LogError("FATAL ERROR: DialogueBox prefab is missing the TextMesh component!", gameObject);
            Destroy(gameObject); 
        }
    }
    
    public void Initialize(Transform targetEntity, string textToDisplay)
    {
        entityToFollow = targetEntity;
        
        dialoguePages = SplitIntoPages(textToDisplay, charsPerLine);
        
        UpdatePosition(); 
        
        StartCoroutine(TypePagesRoutine());
    }
    
    void LateUpdate()
    {
        if (entityToFollow != null)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        transform.position = entityToFollow.position + offset;
    }
    
    IEnumerator TypePagesRoutine()
    {
        float charDelay = 1f / charsPerSecond;

        foreach (string page in dialoguePages)
        {
            dialogueText.text = ""; 

            foreach (char c in page)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(charDelay);
            }

            yield return new WaitForSeconds(pageBreakDelay);
        }

        yield return new WaitForSeconds(destroyDelay);
        
        Destroy(gameObject);
    }
    
    private List<string> SplitIntoPages(string text, int maxLineLength)
    {
        if (string.IsNullOrEmpty(text)) return new List<string>();

        string[] words = text.Split(' ');
        List<string> pages = new List<string>();
        StringBuilder currentPage = new StringBuilder();
        int currentLineLength = 0;

        foreach (string word in words)
        {
            if (currentLineLength + word.Length + 1 > maxLineLength)
            {
                if (currentLineLength > 0)
                {
                    pages.Add(currentPage.ToString().Trim());
                    currentPage.Clear();
                    currentLineLength = 0;
                }
            }

            if (currentLineLength > 0)
            {
                currentPage.Append(" ");
                currentLineLength += 1;
            }

            currentPage.Append(word);
            currentLineLength += word.Length;
        }
        if (currentPage.Length > 0)
        {
            pages.Add(currentPage.ToString().Trim());
        }

        return pages;
    }
}
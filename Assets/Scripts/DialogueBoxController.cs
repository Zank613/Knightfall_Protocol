using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxController : MonoBehaviour
{
    public Text dialogueText;
    public float destroyDelay = 2.0f;     
    public float charsPerSecond = 20f;   
    public Vector3 offset = new Vector3(0, 1.5f, 0); 
    
    private Transform entityToFollow;
    private string currentDialogue;
    
    public void Initialize(Transform targetEntity, string textToDisplay)
    {
        entityToFollow = targetEntity;
        currentDialogue = textToDisplay;
        
        UpdatePosition(); 
        
        // Start the typing effect
        StartCoroutine(TypeDialogueRoutine());
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

    IEnumerator TypeDialogueRoutine()
    {
        float delay = 1f / charsPerSecond;
        int charIndex = 0;
        
        while (charIndex < currentDialogue.Length)
        {
            dialogueText.text = currentDialogue.Substring(0, charIndex + 1);
            charIndex++;
            yield return new WaitForSeconds(delay);
        }
        
        dialogueText.text = currentDialogue;
        
        yield return new WaitForSeconds(destroyDelay);
        
        Destroy(gameObject);
    }
}
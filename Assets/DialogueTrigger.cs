using System.Collections.Generic;
using UnityEngine;

// --- Data Structures (No Changes Needed) ---
[System.Serializable]
public class DialogueCharacter
{
    public string name;
    public Sprite icon;
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}
// ------------------------------------------

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    public void TriggerDialogue()
    {
        // Check if the dialogue system is ready to start a new dialogue
        if (DialogueManager.Instance != null && !DialogueManager.Instance.isDialogueActive)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Using CompareTag is slightly more efficient
        {
            TriggerDialogue();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public Animator animator;

    // State Variables
    private Queue<DialogueLine> lines;
    public bool isDialogueActive = false;
    private bool isTyping = false;
    private DialogueLine currentDialogueLine;

    private void Awake()
    {
        // Singleton Implementation
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        lines = new Queue<DialogueLine>();

        // Ensure the dialogue box is hidden/off-screen at start
        if (animator != null)
        {
            // Assumes "idle" is the off-screen state
            animator.Play("idle");
        }
    }

    private void Update()
    {
        // Check for Space key input to advance dialogue
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Action 1: If currently typing, skip the animation
                SkipTyping();
            }
            else
            {
                // Action 2: If typing is finished, move to the next line
                DisplayNextDialogueLine();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        // Prevent starting dialogue if one is already running
        if (isDialogueActive) return;

        isDialogueActive = true;

        // Animate the box to "show" (slide onto the screen)
        if (animator != null)
        {
            animator.Play("show");
        }

        lines.Clear();

        // Enqueue all lines from the Dialogue object
        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        // Check if there are no more lines
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentDialogueLine = lines.Dequeue();

        // Update UI elements
        characterIcon.sprite = currentDialogueLine.character.icon;
        characterName.text = currentDialogueLine.character.name;

        // Stop any previous typing coroutine before starting a new one
        StopAllCoroutines();

        // Start typing the new line
        StartCoroutine(TypeSentence(currentDialogueLine));
    }

    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        isTyping = true;
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false; // Typing finished
    }

    private void SkipTyping()
    {
        StopAllCoroutines();
        dialogueArea.text = currentDialogueLine.line;
        isTyping = false;
    }

    void EndDialogue()
    {
        isDialogueActive = false;

        // Animate the box to "hide" (slide off-screen, back to idle state)
        if (animator != null)
        {
            animator.Play("hide");
        }

        dialogueArea.text = ""; // Clear text for next dialogue
    }
}
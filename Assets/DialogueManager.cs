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

    private AudioSource audioSource;

    private Queue<DialogueLine> lines;
    public bool isDialogueActive = false;
    private bool isTyping = false;
    private DialogueLine currentDialogueLine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        lines = new Queue<DialogueLine>();

        audioSource = GetComponent<AudioSource>();

        if (animator != null)
        {
            animator.Play("idle");
        }
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                DisplayNextDialogueLine();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (MovementController.Instance != null)
        {
            MovementController.Instance.SetMovementState(false);
        }

        if (isDialogueActive) return;

        isDialogueActive = true;

        if (animator != null)
        {
            animator.Play("show");
        }

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentDialogueLine = lines.Dequeue();

        characterIcon.sprite = currentDialogueLine.character.icon;
        characterName.text = currentDialogueLine.character.name;

        if (currentDialogueLine.lineSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentDialogueLine.lineSound);
        }

        StopAllCoroutines();
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
        if (MovementController.Instance != null)
        {
            MovementController.Instance.SetMovementState(true);
        }

        isDialogueActive = false;

        // Animate the box to "hide" (its slide off-screen, back to idle state)
        if (animator != null)
        {
            animator.Play("hide");
        }

        dialogueArea.text = ""; 
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private const float MessageDuration = 4f;
    private const float AudioPlaybackDuration = 2f;

    private readonly Queue<GameObject> messageQueue = new Queue<GameObject>();
    private readonly Dictionary<string, GameObject> messageObjects = new Dictionary<string, GameObject>();

    private Transform textBox;
    private GameObject startUI;
    private GameObject tvScreen;
    private GameObject npcQueue;
    private GameObject endUI;
    private PlayerMovement playerMovement;
    private Coroutine messageRoutine;
    private bool gameStarted;
    private bool introFinished;
    private bool npcDialogueStarted;
    private bool objectiveOneTriggered;
    private bool objectiveTwoTriggered;
    private bool objectiveThreeTriggered;
    private bool remoteAvailable;
    private bool laptopAvailable;
    private bool laptopSubmitted;

    [SerializeField] private GameObject laptopUI;
    [SerializeField] private GameObject objectiveThreePlane;
    [SerializeField] private AudioSource talkingAudioSource;
    [SerializeField] private AudioSource victoryAudioSource;

    // Finds the UI and scene objects, then hides elements that should start inactive.
    private void Awake()
    {
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject == null)
        {
            return;
        }

        Transform textBoxTransform = canvasObject.transform.Find("PlayerTextBox");
        if (textBoxTransform == null)
        {
            return;
        }

        textBox = textBoxTransform;
        Transform startUITransform = canvasObject.transform.Find("Start");
        if (startUITransform != null)
        {
            startUI = startUITransform.gameObject;
            startUI.SetActive(true);
        }

        playerMovement = GetComponent<PlayerMovement>();

        if (objectiveThreePlane != null)
        {
            objectiveThreePlane.SetActive(false);
        }

        Transform endUITransform = canvasObject.transform.Find("End");
        if (endUITransform != null)
        {
            endUI = endUITransform.gameObject;
            endUI.SetActive(false);
        }

        TMP_Text[] texts = textBoxTransform.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            messageObjects[text.gameObject.name] = text.gameObject;
        }

        HideAllMessages();
        if (laptopUI != null)
        {
            laptopUI.SetActive(false);
        }

        tvScreen = GameObject.Find("Tv_screen");
        if (tvScreen != null)
        {
            tvScreen.SetActive(false);
        }

        npcQueue = GameObject.Find("NPCs");
    }

    // Closes the start UI, begins player control, and starts the opening dialogue.
    public void OnStartButtonPressed()
    {
        if (gameStarted)
        {
            return;
        }

        if (startUI == null || playerMovement == null)
        {
            return;
        }

        gameStarted = true;
        startUI.SetActive(false);
        playerMovement.BeginGameplay();
        EnqueueMessages("Gosh I really wanna travel", "go get plane ticket", "Instruction 1");
    }

    // Handles entering trigger colliders and checks for objective planes.
    private void OnTriggerEnter(Collider other)
    {
        HandleObjectiveContact(other.gameObject);
    }

    // Handles CharacterController contacts and checks for objective planes.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        HandleObjectiveContact(hit.collider.gameObject);
    }

    // Starts the NPC dialogue sequence after the opening instruction is complete.
    public void OnNpcClicked()
    {
        if (!introFinished || npcDialogueStarted)
        {
            return;
        }

        npcDialogueStarted = true;
        EnqueueMessages(
            "Npc dialogue1",
            "Npc dialogue 2",
            "Npc dialogue 3",
            "Npc dialogue 4",
            "Npc dialogue 5",
            "Instruction 2");
    }

    // Turns on the TV and queues the realization message when the remote is available.
    public void OnRemoteClicked()
    {
        if (!remoteAvailable)
        {
            return;
        }

        remoteAvailable = false;
        if (tvScreen != null)
        {
            tvScreen.SetActive(true);
        }

        EnqueueMessages("Realization");
    }

    // Opens the laptop UI when the laptop interaction is available.
    public void OnLaptopClicked()
    {
        if (!laptopAvailable)
        {
            return;
        }

        if (laptopUI == null)
        {
            return;
        }

        laptopUI.SetActive(true);
    }

    // Completes the laptop submission, hides the queue, and enables Objective 3.
    public void OnLaptopSubmitted()
    {
        if (!laptopAvailable || laptopSubmitted)
        {
            return;
        }

        laptopSubmitted = true;
        laptopAvailable = false;
        PlayFirstTwoSeconds(victoryAudioSource);

        if (laptopUI != null)
        {
            laptopUI.SetActive(false);
        }

        if (npcQueue != null)
        {
            npcQueue.SetActive(false);
        }

        if (objectiveThreePlane != null)
        {
            objectiveThreePlane.SetActive(true);
        }

        EnqueueMessages("Excitement", "Instruction 4");
    }

    // Runs the appropriate one-time action when the player contacts an objective.
    private void HandleObjectiveContact(GameObject contactedObject)
    {
        string objectiveTag = GetObjectiveTag(contactedObject.transform);
        if (objectiveTag == "Objective")
        {
            if (objectiveOneTriggered)
            {
                return;
            }

            objectiveOneTriggered = true;
            EnqueueMessages("Npc dialogue 6");
        }
        else if (objectiveTag == "Objective2")
        {
            if (objectiveTwoTriggered)
            {
                return;
            }

            objectiveTwoTriggered = true;
            EnqueueMessages("Abandon", "Abandon 2", "Instruction 3");
        }
        else if (objectiveTag == "Objective3")
        {
            if (objectiveThreeTriggered)
            {
                return;
            }

            objectiveThreeTriggered = true;
            if (endUI != null)
            {
                endUI.SetActive(true);
            }
        }
    }

    // Finds and returns the objective tag on an object or one of its parents.
    private static string GetObjectiveTag(Transform contactedTransform)
    {
        for (Transform current = contactedTransform; current != null; current = current.parent)
        {
            if (current.CompareTag("Objective"))
            {
                return "Objective";
            }

            if (current.CompareTag("Objective2") || current.CompareTag("Objective 2"))
            {
                return "Objective2";
            }

            if (current.CompareTag("Objective 3"))
            {
                return "Objective3";
            }
        }

        return string.Empty;
    }

    // Adds named dialogue objects to the queue and starts playback if needed.
    private void EnqueueMessages(params string[] names)
    {
        foreach (string name in names)
        {
            if (messageObjects.TryGetValue(name, out GameObject messageObject))
            {
                messageQueue.Enqueue(messageObject);
            }
        }

        if (messageRoutine == null && messageQueue.Count > 0)
        {
            messageRoutine = StartCoroutine(PlayMessages());
        }
    }

    // Shows queued messages one at a time and updates progression after each one.
    private IEnumerator PlayMessages()
    {
        while (messageQueue.Count > 0)
        {
            GameObject message = messageQueue.Dequeue();
            textBox.gameObject.SetActive(true);
            message.SetActive(true);
            PlayFirstTwoSeconds(talkingAudioSource);

            yield return new WaitForSecondsRealtime(MessageDuration);

            message.SetActive(false);

            if (message.name == "Instruction 1")
            {
                introFinished = true;
            }
            else if (message.name == "Instruction 3")
            {
                remoteAvailable = true;
            }
            else if (message.name == "Realization")
            {
                laptopAvailable = true;
            }
        }

        textBox.gameObject.SetActive(false);
        messageRoutine = null;
    }

    // Plays an assigned audio source from the beginning and stops it after two seconds.
    private void PlayFirstTwoSeconds(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.Play();
        StartCoroutine(StopAudioAfterTwoSeconds(audioSource));
    }

    // Stops an audio source after the requested short playback interval.
    private IEnumerator StopAudioAfterTwoSeconds(AudioSource audioSource)
    {
        yield return new WaitForSecondsRealtime(AudioPlaybackDuration);
        audioSource.Stop();
    }

    // Hides all dialogue messages and their containing text box.
    private void HideAllMessages()
    {
        foreach (GameObject message in messageObjects.Values)
        {
            message.SetActive(false);
        }

        textBox.gameObject.SetActive(false);
    }
}

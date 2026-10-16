using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MouseTracker : MonoBehaviour
{
    public float maxDistance = 6f;

    private Camera cam;
    private InputAction clickAction;
    private DialogueManager dialogueManager;
    private Transform npcRoot;

    // Finds the camera, dialogue manager, NPC group, and click targets.
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("MouseTracker must be attached to a Camera.", this);
        }

        dialogueManager = GetComponentInParent<DialogueManager>();
        if (dialogueManager == null)
        {
            dialogueManager = FindAnyObjectByType<DialogueManager>();
        }

        GameObject npcObject = GameObject.Find("NPCs");
        if (npcObject != null)
        {
            npcRoot = npcObject.transform;
        }

        AddRemoteColliderIfNeeded();
        AddTravelGuyColliderIfNeeded();
        clickAction = new InputAction("Interact", InputActionType.Button, "<Mouse>/leftButton");
    }

    // Enables mouse-click input while this component is active.
    private void OnEnable()
    {
        clickAction.Enable();
    }

    // Disables mouse-click input while this component is inactive.
    private void OnDisable()
    {
        clickAction.Disable();
    }

    // Releases the mouse-click input action when this component is destroyed.
    private void OnDestroy()
    {
        clickAction.Dispose();
    }

    // Casts a ray from the cursor or screen center and dispatches clicked interactions.
    private void Update()
    {
        if (!clickAction.WasPressedThisFrame())
        {
            return;
        }

        if (cam == null || dialogueManager == null)
        {
            Debug.LogWarning("MouseTracker needs a Camera and DialogueManager.", this);
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked &&
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 pointerPosition = Cursor.lockState == CursorLockMode.Locked
            ? new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            : Mouse.current.position.ReadValue();

        Ray ray = cam.ScreenPointToRay(pointerPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;
            if (HasAncestorWithTag(hitTransform, "Remote"))
            {
                dialogueManager.OnRemoteClicked();
                return;
            }

            if (HasAncestorWithTag(hitTransform, "travel_guy") ||
                (npcRoot != null && hitTransform.IsChildOf(npcRoot)))
            {
                dialogueManager.OnNpcClicked();
                return;
            }

            if (HasAncestorWithTag(hitTransform, "Laptop") ||
                HasAncestorContaining(hitTransform, "laptop"))
            {
                dialogueManager.OnLaptopClicked();
                return;
            }
        }
    }

    // Checks whether an object or one of its parents has the specified tag.
    private static bool HasAncestorWithTag(Transform target, string tag)
    {
        for (Transform current = target; current != null; current = current.parent)
        {
            if (current.CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }

    // Checks whether an object or one of its parents has a name containing the text.
    private static bool HasAncestorContaining(Transform target, string text)
    {
        for (Transform current = target; current != null; current = current.parent)
        {
            if (current.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    // Adds a box collider to the TV remote when it has no collider.
    private void AddRemoteColliderIfNeeded()
    {
        GameObject remote = GameObject.Find("TV_remote_control");
        if (remote == null || remote.GetComponent<Collider>() != null)
        {
            return;
        }

        MeshFilter meshFilter = remote.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning("TV_remote_control needs a Collider or a MeshFilter with a mesh to be clickable.", remote);
            return;
        }

        BoxCollider remoteCollider = remote.AddComponent<BoxCollider>();
        remoteCollider.center = meshFilter.sharedMesh.bounds.center;
        remoteCollider.size = meshFilter.sharedMesh.bounds.size;
    }

    // Adds a click collider to tagged travel NPCs that lack a usable collider.
    private static void AddTravelGuyColliderIfNeeded()
    {
        Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Exclude);
        foreach (Transform target in sceneTransforms)
        {
            if (!target.CompareTag("travel_guy") || HasUsableCollider(target))
            {
                continue;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogWarning("The travel_guy needs a Collider or a Renderer so a click collider can be created.", target);
                continue;
            }

            Bounds localBounds = default;
            bool hasBounds = false;
            foreach (Renderer renderer in renderers)
            {
                Bounds worldBounds = renderer.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 worldPoint = worldBounds.center + Vector3.Scale(
                        worldBounds.extents,
                        new Vector3(
                            (corner & 1) == 0 ? -1f : 1f,
                            (corner & 2) == 0 ? -1f : 1f,
                            (corner & 4) == 0 ? -1f : 1f));
                    Vector3 localPoint = target.InverseTransformPoint(worldPoint);
                    if (hasBounds)
                    {
                        localBounds.Encapsulate(localPoint);
                    }
                    else
                    {
                        localBounds = new Bounds(localPoint, Vector3.zero);
                        hasBounds = true;
                    }
                }
            }

            BoxCollider clickCollider = target.gameObject.AddComponent<BoxCollider>();
            clickCollider.center = localBounds.center;
            clickCollider.size = localBounds.size;
            clickCollider.isTrigger = true;
        }
    }

    // Checks for an enabled collider with valid geometry on an object or its children.
    private static bool HasUsableCollider(Transform target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (!collider.enabled || !collider.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh == null)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}

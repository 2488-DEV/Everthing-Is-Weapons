using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneController : MonoBehaviour
{
    [Header("Scenes to Randomize")]
    public string[] randomScenes = new string[] {
        "inGame_2",
        "inGame_3",
        "inGame_4",
        "inGame_5"
    };

    [Header("Detection Settings")]
    public float detectionRadius = 1.5f;

    private bool hasTriggered = false;

    void Start()
    {
        Debug.Log("[PlayerSceneController] Script is attached and running on: " + gameObject.name);
    }

    void Update()
    {
        if (hasTriggered) return;

        // Use OverlapCircle to detect any "Door" tagged colliders nearby
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Door"))
            {
                InGameScript.NextStage();
                Debug.Log("[PlayerSceneController] Detected Door via OverlapCircle!");
                hasTriggered = true;
                LoadRandomScene();
                return;
            }
        }
    }

    // Keep this as backup in case trigger works
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("[PlayerSceneController] OnTriggerEnter2D fired by: " + collision.gameObject.name + " | Tag: " + collision.gameObject.tag);

        if (!hasTriggered && collision.CompareTag("Door"))
        {
            hasTriggered = true;
            LoadRandomScene();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("[PlayerSceneController] OnCollisionEnter2D fired by: " + collision.gameObject.name + " | Tag: " + collision.gameObject.tag);

        if (!hasTriggered && collision.gameObject.CompareTag("Door"))
        {
            hasTriggered = true;
            LoadRandomScene();
        }
    }

    private void LoadRandomScene()
    {
        if (randomScenes != null && randomScenes.Length > 0)
        {
            int randomIndex = Random.Range(0, randomScenes.Length);
            string sceneToLoad = randomScenes[randomIndex];
            Debug.Log("[PlayerSceneController] Loading random scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("[PlayerSceneController] No scenes assigned to randomize!");
        }
    }

    // Draw the detection radius in the Scene view so you can see it
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

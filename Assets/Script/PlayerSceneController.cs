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

    public bool isEnemyLeft;
    public CatGuide catGuide; 
    public GameObject[] enemies;
    public GameObject[] bosses;
    void Start()
    {
        if (catGuide == null)
            catGuide = FindObjectOfType<CatGuide>();
        Debug.Log("[PlayerSceneController] Script is attached and running on: " + gameObject.name);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasTriggered = false;
        catGuide = FindObjectOfType<CatGuide>();
    }

    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        isEnemyLeft = enemies.Length > 0 || bosses.Length > 0;
        if (hasTriggered) return;

        // Use OverlapCircle to detect any "Door" tagged colliders nearby
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Door"))
            {
                Debug.Log("[PlayerSceneController] Detected Door via OverlapCircle!");
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
            LoadRandomScene();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("[PlayerSceneController] OnCollisionEnter2D fired by: " + collision.gameObject.name + " | Tag: " + collision.gameObject.tag);

        if (!hasTriggered && collision.gameObject.CompareTag("Door"))
        {
            LoadRandomScene();
        }
    }

    private void LoadRandomScene()
    {
        if (!isEnemyLeft && catGuide.hasReachedStopPoint)
        {
            if (randomScenes != null && randomScenes.Length > 0)
            {
                hasTriggered = true;
                InGameScript.NextStage();
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
    }

    // Draw the detection radius in the Scene view so you can see it
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
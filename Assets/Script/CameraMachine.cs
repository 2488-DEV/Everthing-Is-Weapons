using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraMachine : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    void Awake()
    {
        FindPlayer();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + offset;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            offset = transform.position - target.position;
        }
    }
}

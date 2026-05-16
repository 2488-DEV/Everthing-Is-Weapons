using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        if (SceneManager.GetActiveScene().name.Contains("inGame"))
            DontDestroyOnLoad(gameObject);
        else
            Destroy(gameObject);
    }
}

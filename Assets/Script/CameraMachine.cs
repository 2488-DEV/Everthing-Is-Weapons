using UnityEngine;

public class CameraMachine : MonoBehaviour
{
    private Transform target;

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
    }
}

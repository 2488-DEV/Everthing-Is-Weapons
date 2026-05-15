using UnityEngine;

public class CameraMachine : MonoBehaviour
{
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private Transform target;

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
    }
}

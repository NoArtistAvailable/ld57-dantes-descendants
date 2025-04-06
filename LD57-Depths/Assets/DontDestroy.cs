using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.SetParent(null);
        DontDestroyOnLoad(this.gameObject);
    }
}

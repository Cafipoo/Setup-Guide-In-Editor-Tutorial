using UnityEngine;

public class circlePOP : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Destroy this object when something enters its trigger collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}

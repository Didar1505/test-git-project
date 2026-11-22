using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject platform;
    private bool isActivated = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isActivated = !isActivated;
            platform.SetActive(isActivated);
            transform.Rotate(0, 0, isActivated ? 45f : 0f); // Поворачиваем рычаг
            Debug.Log("Рычаг переключен!");
        }
    }
}

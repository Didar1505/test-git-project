using UnityEngine;

public class ButtonMove : MonoBehaviour
{
    public GameObject platform;
    private Vector3 originalPosition;
    private bool isPressed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPressed)
        {
            isPressed = true;
            transform.position = originalPosition + new Vector3(0, -0.1f, 0); // Опускаем кнопку
            platform.SetActive(!platform.activeSelf);
            Debug.Log("Кнопка нажата!");
        }
    }
}

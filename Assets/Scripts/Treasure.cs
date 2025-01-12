using UnityEngine;

public class Treasure : MonoBehaviour
{
    private bool isCollected = false; // Track if the treasure has been collected

    // Property to check if the treasure has been collected
    public bool IsCollected
    {
        get { return isCollected; }
    }

    // Called when the player collects the treasure
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true; // Mark the treasure as collected
            gameObject.SetActive(false); // Disable the treasure object

            // Notify the GameManager that a treasure has been collected
            GameManager.Instance.OnTreasureCollected();
        }
    }

    // Reset the treasure for the next level
    public void ResetTreasure()
    {
        isCollected = false; // Mark the treasure as not collected
        gameObject.SetActive(true); // Enable the treasure object
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    public Transform player; // Reference to the player's Transform
    State currentState;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();

        // Automatically find the player if not assigned in the Inspector
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("PlayerEnemy"); // Find the player by tag
            if (playerObject != null)
            {
                player = playerObject.transform; // Assign the player's Transform
            }
            else
            {
                Debug.LogError("Player not found! Make sure the player has the tag 'Player'.");
            }
        }

        // Initialize the initial state
        currentState = new Idle(this.gameObject, agent, anim, player);
    }
    
    private void OnEnable()
    {
        // Suscribirse al evento cuando el jugador es recreado
        GameManager.OnPlayerRecreated += UpdatePlayerReference;
    }

    private void OnDisable()
    {
        // Desuscribirse del evento cuando el script es desactivado
        GameManager.OnPlayerRecreated -= UpdatePlayerReference;
    }

    // Método para actualizar la referencia al jugador
    private void UpdatePlayerReference(GameObject newPlayer)
    {
        player = newPlayer.transform;
    }
    void Update()
    {
        currentState = currentState.Process();
    }
}
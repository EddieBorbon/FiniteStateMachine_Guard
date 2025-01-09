# Finite State Machine (FSM) Demo Project 🎮

Welcome to the **Finite State Machine (FSM) Demo Project**! This project demonstrates the implementation of a Finite State Machine (FSM) in Unity using `NavMeshAgent` for AI-controlled NPCs. The NPCs transition between different states (Idle, Patrol, Pursue, Attack, RunAway) based on the player's proximity and actions. 🚀

---

## Features ✨
- **State Machine Implementation**: NPCs use a Finite State Machine to manage behaviors.
- **States**:
  - **Idle** 🛑: NPC stands still and waits for events.
  - **Patrol** 🚶: NPC moves between predefined checkpoints.
  - **Pursue** 🏃: NPC chases the player when detected.
  - **Attack** 🔫: NPC attacks the player when in range.
  - **RunAway** 🏃‍♂️💨: NPC flees to a safe location when the player is behind.
- **Dynamic Player Reference**: NPCs dynamically update their reference to the player if the player is recreated.
- **Game Over** ⛔: Triggers a Game Over state when the player is attacked.

---

## Requirements 📋
- **Unity**: Version 2022.3 or higher.
- **NavMesh**: Ensure NavMesh is baked in your scene for NPC navigation.
- **TextMeshPro**: Used for UI elements (if applicable).

---

## Installation 🛠️
1. Clone this repository or download the source code.
2. Open the project in Unity.
3. Ensure all dependencies (e.g., `NavMesh`, `TextMeshPro`) are set up correctly.
4. Open the main scene and press **Play** to start the demo.

---

## How It Works 🧠

### State Machine Overview
The NPCs use a `State` base class with the following structure:
- **States**: `Idle`, `Patrol`, `Pursue`, `Attack`, `RunAway`.
- **Events**: `ENTER`, `UPDATE`, `EXIT`.
- Each state has `Enter()`, `Update()`, and `Exit()` methods to handle behavior during state transitions.

### Key Scripts 📜
- **`State.cs`**: Base class for all states. Handles state transitions and common NPC behaviors (e.g., detecting the player).
- **`Idle.cs`**: NPC waits and transitions to `Patrol` or `Pursue` based on conditions.
- **`Patrol.cs`**: NPC moves between checkpoints and transitions to `Pursue` or `RunAway` if the player is detected.
- **`Pursue.cs`**: NPC chases the player and transitions to `Attack` if the player is within range.
- **`Attack.cs`**: NPC attacks the player and triggers a Game Over if the attack is successful.
- **`RunAway.cs`**: NPC flees to a safe location when the player is behind.

### Dynamic Player Reference 🔄
The NPCs subscribe to the `GameManager.OnPlayerRecreated` event to update their reference to the player dynamically. This ensures the NPCs can always track the player, even if the player is recreated during gameplay.

### Game Over ⛔
When the NPC successfully attacks the player, the `Attack` state triggers the `GameManager.Instance.GameOver()` method, which pauses the game and displays the Game Over screen.

---

## Code Example 💻
Here’s a snippet of the `State` base class:

```csharp
public class State
{
    public enum STATE
    {
        IDLE, PATROL, PURSUE, ATTACK, SLEEP, RUNAWAY
    };
    public enum EVENT
    {
        ENTER, UPDATE, EXIT
    };
    public STATE name;
    protected EVENT stage;
    protected GameObject npc;
    protected Animator anim;
    protected Transform player;
    protected State nextState;
    protected NavMeshAgent agent;

    float visDist = 10.0f;
    float visAngle = 30.0f;
    float shootDist = 7.0f;

    public State(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    {
        npc = _npc;
        agent = _agent;
        anim = _anim;
        stage = EVENT.ENTER;
        player = _player;

        // Subscribe to player recreation event
        GameManager.OnPlayerRecreated += UpdatePlayerReference;
    }

    private void UpdatePlayerReference(GameObject newPlayer)
    {
        player = newPlayer.transform;
    }

    public virtual void Enter() { stage = EVENT.UPDATE; }
    public virtual void Update() { stage = EVENT.UPDATE; }
    public virtual void Exit() { stage = EVENT.EXIT; }

    public State Process()
    {
        if (stage == EVENT.ENTER) Enter();
        if (stage == EVENT.UPDATE) Update();
        if (stage == EVENT.EXIT)
        {
            Exit();
            return nextState;
        }
        return this;
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        return direction.magnitude < visDist && angle < visAngle;
    }
}

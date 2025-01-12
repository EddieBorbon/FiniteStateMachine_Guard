using UnityEngine;
using UnityEngine.AI;

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

        // Suscribirse al evento cuando el jugador es recreado
        GameManager.OnPlayerRecreated += UpdatePlayerReference;
    }

    // Método para actualizar la referencia al jugador
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
        // Verificar si el jugador existe
        if (player == null)
        {
            Debug.LogWarning("El jugador no existe o ha sido destruido.");
            return false;
        }
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        if (direction.magnitude < visDist && angle < visAngle)
        {
            return true;
        }
        return false;
    }

    public bool IsPlayerBehind()
    {
        // Verificar si el jugador existe
        if (player == null)
        {
            Debug.LogWarning("El jugador no existe o ha sido destruido.");
            return false;
        }
        Vector3 direction = npc.transform.position - player.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        if (direction.magnitude < 2 && angle < 30)
        {
            return true;
        }
        return false;
    }

    public bool CanAttackPlayer()
    {
        Vector3 direction = player.position - npc.transform.position;
        if (direction.magnitude < shootDist)
        {
            return true;
        }
        return false;
    }
}

// Clase Idle
public class Idle : State
{
    public Idle(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
        : base(_npc, _agent, _anim, _player)
    {
        name = STATE.IDLE;
    }
    public override void Enter()
    {
        anim.SetTrigger("isIdle");
        base.Enter();
    }
    public override void Update()
    {
        if (CanSeePlayer())
        {
            nextState = new Pursue(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
        else if (Random.Range(0, 100) < 10)
        {
            nextState = new Patrol(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
    }
    public override void Exit()
    {
        anim.ResetTrigger("isIdle");
        base.Exit();
    }
}

// Clase Patrol
public class Patrol : State
{
    int currentIndex = -1;

    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
        : base(_npc, _agent, _anim, _player)
    {
        name = STATE.PATROL;
        agent.speed = 2;
        agent.isStopped = false;
    }

    public override void Enter()
    {
        if (GameEnviroment.Singleton.CheckPoints.Count == 0)
        {
            Debug.LogError("No checkpoints available for patrol.");
            stage = EVENT.EXIT;
            return;
        }

        float lastDist = Mathf.Infinity;
        for (int i = 0; i < GameEnviroment.Singleton.CheckPoints.Count; i++)
        {
            GameObject thisWp = GameEnviroment.Singleton.CheckPoints[i];
            float distance = Vector3.Distance(npc.transform.position, thisWp.transform.position);
            if (distance < lastDist)
            {
                currentIndex = i; // Set currentIndex to the closest checkpoint
                lastDist = distance;
            }
        }

        if (currentIndex == -1)
        {
            Debug.LogError("Failed to find a valid checkpoint.");
            stage = EVENT.EXIT;
            return;
        }

        anim.SetTrigger("isWalking");
        agent.SetDestination(GameEnviroment.Singleton.CheckPoints[currentIndex].transform.position);
        base.Enter();
    }

    public override void Update()
    {
        if (currentIndex < 0 || currentIndex >= GameEnviroment.Singleton.CheckPoints.Count)
        {
            Debug.LogError("Invalid checkpoint index.");
            stage = EVENT.EXIT;
            return;
        }

        if (agent.remainingDistance < 1)
        {
            currentIndex = (currentIndex + 1) % GameEnviroment.Singleton.CheckPoints.Count; // Move to the next checkpoint
            agent.SetDestination(GameEnviroment.Singleton.CheckPoints[currentIndex].transform.position);
        }

        if (CanSeePlayer())
        {
            nextState = new Pursue(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
        else if (IsPlayerBehind())
        {
            nextState = new RunAway(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isWalking");
        base.Exit();
    }
}

// Clase Pursue
public class Pursue : State
{
    public Pursue(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
        : base(_npc, _agent, _anim, _player)
    {
        name = STATE.PURSUE;
        agent.speed = 5;
        agent.isStopped = false;
    }
    public override void Enter()
    {
        anim.SetTrigger("isRunning");
        base.Enter();
    }
    public override void Update()
    {
        agent.SetDestination(player.position);
        if (agent.hasPath)
        {
            if (CanAttackPlayer())
            {
                nextState = new Attack(npc, agent, anim, player);
                stage = EVENT.EXIT;
            }
            else if (!CanSeePlayer())
            {
                nextState = new Patrol(npc, agent, anim, player);
                stage = EVENT.EXIT;
            }
        }
    }
    public override void Exit()
    {
        anim.ResetTrigger("isRunning");
        base.Exit();
    }
}

// Clase Attack
public class Attack : State
{
    float rotationSpeed = 2.0f;
    AudioSource shoot;
    public Attack(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
        : base(_npc, _agent, _anim, _player)
    {
        name = STATE.ATTACK;
        shoot = _npc.GetComponent<AudioSource>();
    }

    public override void Enter()
    {
        anim.SetTrigger("isShooting");
        agent.isStopped = true;
        shoot.Play();
        base.Enter();
    }

    public override void Update()
    {
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        direction.y = 0;

        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);

        // Si el enemigo deja de poder atacar, vuelve al estado Idle
        if (!CanAttackPlayer())
        {
            nextState = new Idle(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }

        // Llamamos a GameOver si el ataque es exitoso
        if (CanAttackPlayer())
        {
            TriggerGameOver();
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isShooting");
        shoot.Stop();
        base.Exit();
    }

    // Método para activar el Game Over cuando el enemigo ataque al jugador
    private void TriggerGameOver()
    {
        Debug.Log("El enemigo ha atacado al jugador. Fin del juego.");
        
        // Llamamos al GameManager para ejecutar el Game Over
        GameManager.Instance.GameOver();
    }
}

// Clase RunAway
public class RunAway : State
{
    GameObject safeLocation;
    public RunAway(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
        : base(_npc, _agent, _anim, _player)
    {
        name = STATE.RUNAWAY;
        safeLocation = GameObject.FindGameObjectWithTag("Safe");
    }
    public override void Enter()
    {
        anim.SetTrigger("isRunning");
        agent.isStopped = false;
        agent.speed = 6;
        agent.SetDestination(safeLocation.transform.position);
        base.Enter();
    }
    public override void Update()
    {
        if (agent.remainingDistance < 1)
        {
            nextState = new Idle(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
        base.Update();
    }
    public override void Exit()
    {
        anim.ResetTrigger("isRunning");
        base.Exit();
    }
}
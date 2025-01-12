using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton para acceder al GameManager desde otros scripts
    public static GameManager Instance;

    // Variables públicas
    public TextMeshProUGUI roundText;
    public GameObject youWinPanel;
    public GameObject gameOverPanel;
    public int currentLevel = 1;
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public GameObject playerPrefab;
    public Treasure[] treasures;
    public float spawnRadius = 10f;
    public float minDistanceFromTreasure = 5f;

    private GameObject player;

    // Evento para notificar cuando el jugador es recreado
    public delegate void PlayerRecreatedHandler(GameObject newPlayer);
    public static event PlayerRecreatedHandler OnPlayerRecreated;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        SpawnPlayer();
        treasures = FindObjectsOfType<Treasure>();

        if (youWinPanel != null) youWinPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateRoundUI();
        SpawnEnemies();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            // Destruir al jugador actual si existe
            if (player != null)
            {
                Destroy(player);
            }

            // Crear una nueva instancia del jugador
            Vector3 spawnPosition = GetRandomSpawnPosition();
            player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("Jugador creado en: " + spawnPosition);

            // Notificar a otros scripts que el jugador ha sido recreado
            OnPlayerRecreated?.Invoke(player);
        }
        else
        {
            Debug.LogError("Player prefab not assigned!");
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPosition;
        bool positionValid = false;

        do
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            spawnPosition = new Vector3(randomCircle.x, 0f, randomCircle.y);
            positionValid = IsPositionValid(spawnPosition);
        } while (!positionValid);

        return spawnPosition;
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (Treasure treasure in treasures)
        {
            float distance = Vector3.Distance(position, treasure.transform.position);
            if (distance < minDistanceFromTreasure)
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text = "Round: " + currentLevel;
        }
    }

    public void OnTreasureCollected()
    {
        CheckAllTreasuresCollected();
    }

    private void CheckAllTreasuresCollected()
    {
        bool allCollected = true;

        foreach (Treasure treasure in treasures)
        {
            if (!treasure.IsCollected)
            {
                allCollected = false;
                break;
            }
        }

        if (allCollected)
        {
            ShowYouWinPanel();
            StartCoroutine(LoadNextLevelAfterDelay(1f));
        }
    }

    private void ShowYouWinPanel()
    {
        if (youWinPanel != null)
        {
            youWinPanel.SetActive(true);
        }
    }

    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        youWinPanel.SetActive(false);
        NextLevel();
    }

    private void SpawnEnemies()
    {
        int enemyCount = currentLevel;
        for (int i = 0; i < enemyCount; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    public void NextLevel()
    {
        currentLevel++;
        ResetGameElements();
        UpdateRoundUI();
    }

    private void ResetGameElements()
    {
        SpawnPlayer();

        foreach (Treasure treasure in treasures)
        {
            treasure.ResetTreasure();
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        SpawnEnemies();
    }

    // Método para manejar el Game Over
    public void GameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Mostrar el panel de Game Over
            Time.timeScale = 0; // Pausar el juego
        }
    }

    // Método para reiniciar el nivel
    public void RestartLevel()
    {
        Time.timeScale = 1; // Reanudar el juego
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recargar la escena actual
    }

    // Método para salir del juego (opcional)
    public void QuitGame()
    {
        Application.Quit();
    }
}
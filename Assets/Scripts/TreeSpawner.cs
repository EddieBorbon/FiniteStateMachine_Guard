using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject[] treePrefabs; // Array de prefabs de árboles
    public int numberOfTrees = 10; // Número de árboles a generar
    public float spawnArea = 10f; // Área de generación

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        for (int i = 0; i < numberOfTrees; i++)
        {
            // Genera una posición aleatoria dentro del área definida
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnArea / 2, spawnArea / 2),
                0,
                Random.Range(-spawnArea / 2, spawnArea / 2)
            );

            // Selecciona un prefab de árbol aleatorio del array
            GameObject randomTree = treePrefabs[Random.Range(0, treePrefabs.Length)];

            // Instancia el árbol en la posición generada
            Instantiate(randomTree, spawnPosition, Quaternion.identity);
        }
    }
}
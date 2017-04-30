using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour
{
    [Range(0,10)]
    public int numberOfPurpleEnemies;
    [Range(0, 10)]
    public int numberOfRedEnemies;
    [Range(0, 10)]
    public int numberOfBlueEnemies;
    public GameObject area;

    public GameObject enemyPurplePrefab;
    public GameObject enemyRedPrefab;
    public GameObject enemyBluePrefab;

    GameObject player;
    GameObject enemy;

    Transform[] waypoints;

    int numberOfPlayers;

    public override void OnStartServer()
    {
        //player = GameObject.FindGameObjectWithTag("Player");
        numberOfPlayers = Network.connections.Length;
        print("Number of players " + numberOfPlayers);

        waypoints = area.GetComponentsInChildren<Transform>();

        for (int i = 0; i < numberOfPurpleEnemies; i++)
        {
            var spawnPosition = waypoints[Random.Range(0, waypoints.Length)].position;

            var spawnRotation = Quaternion.Euler(0.0f, Random.Range(0, 180), 0.0f);

            enemy = Instantiate(enemyPurplePrefab, spawnPosition, spawnRotation);
            enemy.GetComponent<Enemy>().waypoints = waypoints;
            enemy.SetActive(true);
            NetworkServer.Spawn(enemy);
        }

        for (int i = 0; i < numberOfRedEnemies; i++)
        {
            var spawnPosition = waypoints[Random.Range(0, waypoints.Length)].position;

            var spawnRotation = Quaternion.Euler(0.0f, Random.Range(0, 180), 0.0f);

            enemy = Instantiate(enemyRedPrefab, spawnPosition, spawnRotation);
            enemy.GetComponent<Enemy>().waypoints = area.GetComponentsInChildren<Transform>();
            enemy.SetActive(true);
            NetworkServer.Spawn(enemy);
        }

        for (int i = 0; i < numberOfBlueEnemies; i++)
        {
            var spawnPosition = waypoints[Random.Range(0, waypoints.Length)].position;

            var spawnRotation = Quaternion.Euler(0.0f, Random.Range(0, 180), 0.0f);

            enemy = Instantiate(enemyBluePrefab, spawnPosition, spawnRotation);
            enemy.GetComponent<Enemy>().waypoints = area.GetComponentsInChildren<Transform>();
            enemy.SetActive(true);
            NetworkServer.Spawn(enemy);
        }
    }
}
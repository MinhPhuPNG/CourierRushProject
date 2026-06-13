using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class TargetSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject target;
    public WorldSpawner worldSpawner;
    public float gameDurationSeconds = 60f;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI statusText;
    public AudioClip backgroundMusic;
    public AudioClip pickupSound;
    public AudioClip deliverySound;
    public AudioSource musicSource;
    float minSpawnDistance = 10f;
    float collectDistance = 1f;
    int collectCount;
    int deliveryScore;
    float timeRemaining;
    bool isGameOver;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeRemaining = gameDurationSeconds;
        collectCount = 0;
        deliveryScore = 0;
        isGameOver = false;
        UpdateUI();
        if (musicSource != null)
        {
            musicSource.volume = 0.1f;
            if (!musicSource.isPlaying || musicSource.clip != backgroundMusic)
            {
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
        if (worldSpawner != null && target != null)
        {
            worldSpawner.target = target.transform;
        }
        Invoke(nameof(SpawnTargetInitial), 0.1f);
    }

    void SpawnTargetInitial()
    {
        SpawnTarget(true);
    }
    void SpawnTarget(bool ignoreDistance)
    {
        List<Vector3> candidates = new List<Vector3>();
        foreach (var kvp in worldSpawner.GetActiveChunks())
        {
            GameObject chunkObject = kvp.Value;
            if (chunkObject == null)
            {
                continue;
            }
            if (!ignoreDistance && Vector3.Distance(player.position, chunkObject.transform.position) < minSpawnDistance)
            {
                continue;
            }

            Tilemap road = chunkObject.transform.Find("Road")?.GetComponent<Tilemap>();

            if (road == null)
            {
                continue;
            }

            foreach (Vector3Int cellPosition in road.cellBounds.allPositionsWithin)
            {
                if (road.HasTile(cellPosition))
                {
                    Vector3 worldPosition = road.CellToWorld(cellPosition);
                    candidates.Add(worldPosition);
                }
            }
        }
        if (candidates.Count == 0)
        {
            Debug.LogWarning("No valid target spawn positions found.");
            return;
        }
        target.transform.position = candidates[Random.Range(0, candidates.Count)];
        target.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                RestartGame();
            }
            return;
        }

        if (target == null || player == null || worldSpawner == null)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isGameOver = true;
            Time.timeScale = 0f;
            if (target != null)
            {
                target.SetActive(false);
            }
            if (statusText != null)
            {
                statusText.text = "Time's up! Press SPACE to restart. Final score: " + deliveryScore;
                statusText.gameObject.SetActive(true);
            }
            UpdateUI();
            return;
        }

        UpdateUI();

        float distance = Vector3.Distance(player.position, target.transform.position);
        
        if (distance < collectDistance)
        {
            CollectTarget();
            return;
        }

        Vector2Int targetCoord = worldSpawner.WorldToGrid(target.transform.position);
        
        if (!worldSpawner.IsChunkActive(targetCoord))
        {
            SpawnTarget(false);
        }
    }

    void CollectTarget()
    {
        collectCount++;
        if (collectCount % 2 == 0)
        {
            deliveryScore++;
            statusText.text = "Delivered, pick up next target.";
            
            AudioSource.PlayClipAtPoint(deliverySound, player.position);
        }
        else
        {
            statusText.text = "Package picked up, deliver to target.";
            AudioSource.PlayClipAtPoint(pickupSound, player.position);
        }

        UpdateUI();
        SpawnTarget(false);
    }

    void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining);
        }

        if (scoreText != null)
        {
            scoreText.text = "Score: " + deliveryScore;
        }
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

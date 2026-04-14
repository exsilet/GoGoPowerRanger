using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic5_4 : MonoBehaviour, IBossMechanic
{
    [SerializeField] private Transform boss;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform player;
    [SerializeField] private AudioSource shootSound;

    [SerializeField] private int shotsPerSequence = 3;
    [SerializeField] private float blinkDuration = 0.3f;
    [SerializeField] private float teleportDelay = 0.5f;
    [SerializeField] private int sequenceCount = 3;
    [SerializeField] private float projectileSpeed = 5f;

    private Vector2 minBounds;
    private Vector2 maxBounds;
    private float bossWidth;
    private float bossHeight;
    private SpriteRenderer bossSpriteRenderer;

    private readonly List<GameObject> projectiles = new List<GameObject>();
    private Collider2D[] boundaryColliders;

    private void Start()
    {
        if (boss == null)
            boss = transform;

        if (player == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
                player = pc.transform;
        }

        if (boss == null || projectilePrefab == null || player == null)
        {
            Debug.LogError("Mechanic5_4: не назначены обязательные ссылки.");
            enabled = false;
            return;
        }

        CalculateCameraBounds();

        bossSpriteRenderer = boss.GetComponent<SpriteRenderer>();
        if (bossSpriteRenderer == null)
        {
            Debug.LogError("Mechanic5_4: у boss нет SpriteRenderer.");
            enabled = false;
            return;
        }

        bossWidth = bossSpriteRenderer.bounds.extents.x;
        bossHeight = bossSpriteRenderer.bounds.extents.y;

        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("Boundary");
        boundaryColliders = new Collider2D[boundaries.Length];
        for (int i = 0; i < boundaries.Length; i++)
        {
            boundaryColliders[i] = boundaries[i].GetComponent<Collider2D>();
        }
    }

    public IEnumerator Execute()
    {
        if (!enabled)
            yield break;

        SetBoundaryCollidersEnabled(false);

        for (int i = 0; i < sequenceCount; i++)
        {
            yield return StartCoroutine(BlinkBoss(2, blinkDuration));

            SetBossVisibility(false);
            yield return new WaitForSeconds(teleportDelay);

            TeleportBoss();
            SetBossVisibility(true);

            yield return StartCoroutine(ShootProjectiles());

            yield return new WaitForSeconds(teleportDelay);
        }

        yield return StartCoroutine(BlinkBoss(2, blinkDuration));

        SetBoundaryCollidersEnabled(true);
        DestroyAllProjectiles();
    }

    private IEnumerator BlinkBoss(int blinkCount, float duration)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            SetBossVisibility(false);
            yield return new WaitForSeconds(duration);
            SetBossVisibility(true);
            yield return new WaitForSeconds(duration);
        }
    }

    private void TeleportBoss()
    {
        float randomX = Random.Range(minBounds.x + bossWidth, maxBounds.x - bossWidth);
        float randomY = Random.Range(minBounds.y + bossHeight, maxBounds.y - bossHeight);
        boss.position = new Vector2(randomX, randomY);
    }

    private void CalculateCameraBounds()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Mechanic5_4: Camera.main не найдена.");
            enabled = false;
            return;
        }

        minBounds = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        maxBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));
    }

    private IEnumerator ShootProjectiles()
    {
        if (player == null || boss == null || projectilePrefab == null)
            yield break;

        for (int i = 0; i < shotsPerSequence; i++)
        {
            if (shootSound != null)
                shootSound.Play();

            GameObject projectile = Instantiate(projectilePrefab, boss.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 direction = ((Vector2)player.position - (Vector2)boss.position).normalized;
                rb.velocity = direction * projectileSpeed;
            }
            else
            {
                Debug.LogWarning("Mechanic5_4: у projectilePrefab нет Rigidbody2D.");
            }

            projectiles.Add(projectile);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SetBossVisibility(bool visible)
    {
        if (bossSpriteRenderer != null)
            bossSpriteRenderer.enabled = visible;
    }

    private void SetBoundaryCollidersEnabled(bool enabledValue)
    {
        if (boundaryColliders == null)
            return;

        foreach (Collider2D collider in boundaryColliders)
        {
            if (collider != null)
                collider.enabled = enabledValue;
        }
    }

    private void DestroyAllProjectiles()
    {
        foreach (GameObject projectile in projectiles)
        {
            if (projectile != null)
                Destroy(projectile);
        }

        projectiles.Clear();
    }
}
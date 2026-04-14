using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic3_1 : MonoBehaviour, IBossMechanic
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float flashInterval = 0.3f;
    [SerializeField] private float wallFallSpeed = 5f;
    [SerializeField] private float wallDestroyY = -10f;
    [SerializeField] private float gapWidth = 2f;
    [SerializeField] private int fallCountMin = 5;
    [SerializeField] private int fallCountMax = 8;
    [SerializeField] private float delayBetweenFalls = 1.8f;
    [SerializeField] private float spawnOffsetY = 1.5f;
    [SerializeField] private float wallThickness = 1.2f;

    private SpriteRenderer spriteRenderer;
    private readonly List<GameObject> activeWalls = new List<GameObject>();

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator Execute()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("Mechanic3_1: wallPrefab не назначен.");
            yield break;
        }

        Debug.Log("Механика 3_1 запущена.");

        yield return StartCoroutine(BossFlash());
        SetVisibility(false);

        int fallCount = Random.Range(fallCountMin, fallCountMax + 1);
        for (int i = 0; i < fallCount; i++)
        {
            SpawnWallPairWithGap();
            yield return new WaitForSeconds(delayBetweenFalls);
        }

        while (activeWalls.Count > 0)
        {
            activeWalls.RemoveAll(w => w == null);
            yield return null;
        }

        SetVisibility(true);
        Debug.Log("Механика 3_1 завершена.");
    }

    private IEnumerator BossFlash()
    {
        for (int i = 0; i < 2; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            yield return new WaitForSeconds(flashInterval);

            if (spriteRenderer != null)
                spriteRenderer.enabled = true;

            yield return new WaitForSeconds(flashInterval);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void SpawnWallPairWithGap()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Mechanic3_1: Camera.main не найдена.");
            return;
        }

        float screenHalfWidth = cam.orthographicSize * cam.aspect;
        float topY = cam.orthographicSize + spawnOffsetY;
        float fullWidth = screenHalfWidth * 2f;

        float minGapCenter = -screenHalfWidth + gapWidth * 0.5f;
        float maxGapCenter = screenHalfWidth - gapWidth * 0.5f;
        float gapCenterX = Random.Range(minGapCenter, maxGapCenter);

        float leftWidth = Mathf.Max(0f, gapCenterX - gapWidth * 0.5f + screenHalfWidth);
        float rightWidth = Mathf.Max(0f, screenHalfWidth - (gapCenterX + gapWidth * 0.5f));

        if (leftWidth > 0.05f)
        {
            Vector3 leftPos = new Vector3(-screenHalfWidth + leftWidth * 0.5f, topY, 0f);
            GameObject leftWall = Instantiate(wallPrefab, leftPos, Quaternion.identity);
            leftWall.transform.localScale = new Vector3(leftWidth, wallThickness, 1f);
            activeWalls.Add(leftWall);
            StartCoroutine(FallAndDestroyWall(leftWall));
        }

        if (rightWidth > 0.05f)
        {
            Vector3 rightPos = new Vector3(gapCenterX + gapWidth * 0.5f + rightWidth * 0.5f, topY, 0f);
            GameObject rightWall = Instantiate(wallPrefab, rightPos, Quaternion.identity);
            rightWall.transform.localScale = new Vector3(rightWidth, wallThickness, 1f);
            activeWalls.Add(rightWall);
            StartCoroutine(FallAndDestroyWall(rightWall));
        }
    }

    private IEnumerator FallAndDestroyWall(GameObject wall)
    {
        while (wall != null && wall.transform.position.y > wallDestroyY)
        {
            wall.transform.position += Vector3.down * wallFallSpeed * Time.deltaTime;
            yield return null;
        }

        if (wall != null)
        {
            activeWalls.Remove(wall);
            Destroy(wall);
        }
    }

    private void SetVisibility(bool isVisible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = isVisible;

        foreach (Transform child in transform)
        {
            SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
            if (childRenderer != null)
                childRenderer.enabled = isVisible;
        }
    }
}
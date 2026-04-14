using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic4_5 : MonoBehaviour, IBossMechanic
{
    [Header("Main")]
    [SerializeField] private GameObject weakSpotPrefab;
    [SerializeField] private GameObject[] colorObjects;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float flashInterval = 1f;
    [SerializeField] private int maxRounds = 3;
    [SerializeField] private float playerChoiceTimeout = 8f;

    [Header("Audio")]
    [SerializeField] private AudioSource colorFlashSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctChoiceClip;
    [SerializeField] private AudioClip incorrectChoiceClip;

    private List<string> colorSequence;
    private List<string> playerSequence;
    private readonly List<GameObject> spawnedColorObjects = new List<GameObject>();

    private Vector3[] spawnPositions;
    private int roundCount;
    private SpriteRenderer spriteRenderer;

    private bool isCollecting;
    private bool isRoundComplete;
    private bool isWeakSpotDestroyed;

    private GameObject weakSpot;
    private Transform eyeTransform;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        eyeTransform = transform.Find("Eye");

        if (Camera.main == null)
        {
            Debug.LogError("Mechanic4_5: Camera.main не найдена.");
            return;
        }

        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float halfHeight = Camera.main.orthographicSize;

        spawnPositions = new Vector3[]
        {
            new Vector3(-halfWidth + 1f, -halfHeight + 1f, 0f),
            new Vector3(-halfWidth / 3f, -halfHeight + 1f, 0f),
            new Vector3( halfWidth / 3f, -halfHeight + 1f, 0f),
            new Vector3( halfWidth - 1f, -halfHeight + 1f, 0f)
        };
    }

    public IEnumerator Execute()
    {
        if (weakSpotPrefab == null || colorObjects == null || colorObjects.Length < 4)
        {
            Debug.LogError("Mechanic4_5: не назначены weakSpotPrefab/colorObjects.");
            yield break;
        }

        Debug.Log("Механика 4_5 запущена.");

        roundCount = 0;
        isWeakSpotDestroyed = false;
        weakSpot = null;

        SetEyeVisible(false);

        yield return StartCoroutine(MoveToCorner());
        yield return new WaitForSeconds(0.5f);

        while (roundCount < maxRounds)
        {
            colorSequence = GenerateRandomColorSequence();
            playerSequence = new List<string>();

            yield return StartCoroutine(FlashSequence(colorSequence));
            yield return new WaitForSeconds(0.5f);

            SpawnColorObjects();
            yield return StartCoroutine(FlashColorObjects());

            isCollecting = true;
            isRoundComplete = false;

            float elapsed = 0f;
            while (isCollecting && !isRoundComplete && elapsed < playerChoiceTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= playerChoiceTimeout)
            {
                Debug.LogWarning("Mechanic4_5: timeout выбора игрока.");
                isCollecting = false;
                isRoundComplete = false;
            }

            if (!isRoundComplete)
            {
                Debug.Log("Механика 4_5 завершена из-за ошибки игрока или timeout.");
                RemoveAllColorObjects();
                SetEyeVisible(true);
                yield break;
            }

            Debug.Log($"Раунд {roundCount + 1} завершён успешно.");
            roundCount++;
            RemoveAllColorObjects();

            yield return new WaitForSeconds(0.5f);

            if (roundCount >= maxRounds)
            {
                Debug.Log("Все раунды завершены успешно. Появляется weak spot.");
                SpawnWeakSpot();
                SetEyeVisible(true);

                while (!isWeakSpotDestroyed)
                    yield return null;

                Debug.Log("Weak spot уничтожен. Механика завершена.");
                yield break;
            }
        }

        SetEyeVisible(true);
        Debug.Log("Механика 4_5 завершена.");
    }

    private IEnumerator MoveToCorner()
    {
        if (Camera.main == null)
            yield break;

        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
            yield break;

        float objectWidth = rend.bounds.extents.x;
        float objectHeight = rend.bounds.extents.y;

        float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float screenHalfHeight = Camera.main.orthographicSize;

        Vector3 targetPosition = new Vector3(
            Mathf.Clamp(screenHalfWidth - objectWidth, -screenHalfWidth + objectWidth, screenHalfWidth - objectWidth),
            Mathf.Clamp(screenHalfHeight - objectHeight, -screenHalfHeight + objectHeight, screenHalfHeight - objectHeight),
            0f
        );

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
    }

    private List<string> GenerateRandomColorSequence()
    {
        string[] colors = { "Red", "Blue", "Yellow", "Black" };
        List<string> sequence = new List<string>();

        while (sequence.Count < 4)
        {
            string color = colors[Random.Range(0, colors.Length)];
            if (!sequence.Contains(color))
                sequence.Add(color);
        }

        return sequence;
    }

    private IEnumerator FlashSequence(List<string> sequence)
    {
        foreach (string color in sequence)
        {
            SetBossColor(color);

            if (colorFlashSound != null)
                colorFlashSound.Play();

            yield return new WaitForSeconds(flashInterval);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    private void SetBossColor(string color)
    {
        if (spriteRenderer == null)
            return;

        switch (color)
        {
            case "Red": spriteRenderer.color = Color.red; break;
            case "Blue": spriteRenderer.color = Color.blue; break;
            case "Yellow": spriteRenderer.color = Color.yellow; break;
            case "Black": spriteRenderer.color = Color.black; break;
            default: spriteRenderer.color = Color.white; break;
        }
    }

    private void SpawnColorObjects()
    {
        RemoveAllColorObjects();

        string[] colors = { "Red", "Blue", "Yellow", "Black" };

        for (int i = 0; i < colorObjects.Length && i < spawnPositions.Length; i++)
        {
            GameObject obj = Instantiate(colorObjects[i], spawnPositions[i], Quaternion.identity);
            obj.SetActive(false);
            SetObjectColor(obj, colors[i]);
            spawnedColorObjects.Add(obj);
        }
    }

    private IEnumerator FlashColorObjects()
    {
        foreach (GameObject obj in spawnedColorObjects)
        {
            if (obj == null) continue;

            Collider2D collider = obj.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;
        }

        for (int i = 0; i < 2; i++)
        {
            foreach (GameObject obj in spawnedColorObjects)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            yield return new WaitForSeconds(0.3f);

            foreach (GameObject obj in spawnedColorObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }

            yield return new WaitForSeconds(0.3f);
        }

        foreach (GameObject obj in spawnedColorObjects)
        {
            if (obj == null) continue;

            Collider2D collider = obj.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = true;
        }
    }

    private void SetObjectColor(GameObject obj, string color)
    {
        if (obj == null) return;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (color)
        {
            case "Red": sr.color = Color.red; break;
            case "Blue": sr.color = Color.blue; break;
            case "Yellow": sr.color = Color.yellow; break;
            case "Black": sr.color = Color.black; break;
        }
    }

    public void OnColorObjectTouched(string colorName)
    {
        if (!isCollecting || playerSequence == null || colorSequence == null)
            return;

        playerSequence.Add(colorName);

        int currentIndex = playerSequence.Count - 1;
        Debug.Log($"Игрок выбрал: {colorName}, ожидалось: {colorSequence[currentIndex]}");

        if (playerSequence[currentIndex] != colorSequence[currentIndex])
        {
            if (audioSource != null && incorrectChoiceClip != null)
                audioSource.PlayOneShot(incorrectChoiceClip);

            Debug.Log("Mechanic4_5: ошибка в последовательности.");
            isCollecting = false;
            isRoundComplete = false;
            RemoveAllColorObjects();
            return;
        }

        if (audioSource != null && correctChoiceClip != null)
            audioSource.PlayOneShot(correctChoiceClip);

        if (playerSequence.Count >= colorSequence.Count)
        {
            Debug.Log("Mechanic4_5: последовательность собрана правильно.");
            isCollecting = false;
            isRoundComplete = true;
        }
    }

    private void RemoveAllColorObjects()
    {
        foreach (GameObject obj in spawnedColorObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedColorObjects.Clear();
    }

    private void SpawnWeakSpot()
    {
        if (weakSpot != null)
            return;

        weakSpot = Instantiate(weakSpotPrefab, Vector3.zero, Quaternion.identity);
        WeakSpot_1 weakSpotScript = weakSpot.GetComponent<WeakSpot_1>();

        if (weakSpotScript == null)
        {
            Debug.LogError("Mechanic4_5: на weakSpotPrefab нет WeakSpot_1.");
            return;
        }

        weakSpotScript.OnDestroyed += OnWeakSpotDestroyed;
    }

    private void OnWeakSpotDestroyed()
    {
        if (isWeakSpotDestroyed)
            return;

        isWeakSpotDestroyed = true;
        Debug.Log("Mechanic4_5: weak spot уничтожен.");

        BossController_2 bossController = FindObjectOfType<BossController_2>();
        if (bossController != null)
            bossController.TakeDamage(1);

        StopAllCoroutines();
    }

    private void SetEyeVisible(bool visible)
    {
        if (eyeTransform == null)
            return;

        Renderer eyeRenderer = eyeTransform.GetComponent<Renderer>();
        if (eyeRenderer != null)
            eyeRenderer.enabled = visible;
    }
}
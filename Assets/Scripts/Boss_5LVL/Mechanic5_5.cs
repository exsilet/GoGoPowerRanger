using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic5_5 : MonoBehaviour, IBossMechanic
{
    [SerializeField] private Transform boss;
    [SerializeField] private GameObject thimblePrefab;
    [SerializeField] private GameObject soughtPrefab;
    [SerializeField] private GameObject weakSpotPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private int repetitions = 3;
    [SerializeField] private float choiceTimeout = 10f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip thimbleAppearClip;
    [SerializeField] private AudioClip shuffleLoopClip;
    [SerializeField] private AudioClip correctChoiceClip;
    [SerializeField] private AudioClip wrongChoiceClip;

    private readonly List<GameObject> thimbles = new List<GameObject>();
    private readonly List<Vector2> thimblePositions = new List<Vector2>();

    private GameObject soughtObject;
    private int correctThimbleIndex;
    private BossController_3 bossController;
    private int correctChoices;
    private bool mechanicActive;
    private bool playerMadeChoice;

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

        bossController = GetComponentInParent<BossController_3>();

        if (bossController == null)
            Debug.LogError("Mechanic5_5: BossController_3 не найден.");

        if (boss == null || thimblePrefab == null || soughtPrefab == null || weakSpotPrefab == null || player == null)
            Debug.LogError("Mechanic5_5: не назначены обязательные ссылки.");
    }

    public IEnumerator Execute()
    {
        if (boss == null || thimblePrefab == null || soughtPrefab == null || weakSpotPrefab == null || player == null)
            yield break;

        mechanicActive = true;
        playerMadeChoice = false;
        correctChoices = 0;

        CleanupObjects();

        yield return StartCoroutine(MoveBossToCenter());

        while (correctChoices < repetitions && mechanicActive)
        {
            playerMadeChoice = false;

            CreateThimbles();
            yield return StartCoroutine(CreateAndHideSoughtObject());
            yield return StartCoroutine(ShuffleThimbles());

            EnableThimbleInteraction(true);

            float elapsed = 0f;
            while (!playerMadeChoice && mechanicActive && elapsed < choiceTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            EnableThimbleInteraction(false);

            if (!playerMadeChoice)
            {
                Debug.LogWarning("Mechanic5_5: выбор игрока не сделан вовремя. Механика завершена.");
                mechanicActive = false;
            }

            if (!mechanicActive)
            {
                EndMechanic();
                yield break;
            }

            if (correctChoices >= repetitions)
            {
                if (soughtObject != null)
                    soughtObject.transform.SetParent(null);

                yield return StartCoroutine(BlinkThimbles(2, 0.3f));
                DestroyThimbles();
                break;
            }
            else
            {
                DestroyThimbles();
            }

            yield return new WaitForSeconds(1f);
        }

        if (mechanicActive && correctChoices >= repetitions)
        {
            TransformSoughtObjectToWeakSpot();
            yield return new WaitUntil(() => mechanicActive == false);
        }
        else
        {
            Debug.LogWarning("Mechanic5_5: механика завершена до появления weak spot.");
        }
    }

    private IEnumerator MoveBossToCenter()
    {
        Renderer bossRenderer = boss.GetComponent<Renderer>();
        if (bossRenderer == null || Camera.main == null)
            yield break;

        float bossHalfWidth = bossRenderer.bounds.extents.x;
        float bossHalfHeight = bossRenderer.bounds.extents.y;

        float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float screenHalfHeight = Camera.main.orthographicSize;

        Vector2 targetPosition = new Vector2(
            Mathf.Clamp(0f, -screenHalfWidth + bossHalfWidth, screenHalfWidth - bossHalfWidth),
            Mathf.Clamp(Camera.main.orthographicSize - bossHalfHeight, -screenHalfHeight + bossHalfHeight, screenHalfHeight - bossHalfHeight)
        );

        Vector2 startPosition = boss.position;
        float elapsedTime = 0f;
        float moveDuration = 2f;

        while (elapsedTime < moveDuration)
        {
            boss.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        boss.position = targetPosition;
    }

    private void CreateThimbles()
    {
        thimbles.Clear();
        thimblePositions.Clear();

        float spacing = 3f;
        int centralThimbleIndex = 1;

        for (int i = -1; i <= 1; i++)
        {
            Vector2 position = new Vector2(i * spacing, -1f);
            thimblePositions.Add(position);

            GameObject thimble = Instantiate(thimblePrefab, position, Quaternion.identity);

            Thimble thimbleScript = thimble.GetComponent<Thimble>();
            if (thimbleScript == null)
                thimbleScript = thimble.AddComponent<Thimble>();

            bool isCorrect = (i + 1) == centralThimbleIndex;
            thimbleScript.Initialize(this, isCorrect);

            thimbles.Add(thimble);
        }

        if (audioSource != null && thimbleAppearClip != null)
            audioSource.PlayOneShot(thimbleAppearClip);

        EnableThimbleInteraction(false);
    }

    private IEnumerator CreateAndHideSoughtObject()
    {
        if (thimbles.Count < 3)
            yield break;

        correctThimbleIndex = 1;

        Vector3 startPosition = new Vector3(
            thimbles[correctThimbleIndex].transform.position.x,
            -Camera.main.orthographicSize - 1f,
            0f
        );

        if (soughtObject != null)
            Destroy(soughtObject);

        soughtObject = Instantiate(soughtPrefab, startPosition, Quaternion.identity);

        SpriteRenderer sr = soughtObject.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingLayerName = "BehindThimble";

        yield return new WaitForSeconds(1f);

        Vector3 targetPosition = thimbles[correctThimbleIndex].transform.position;
        float moveDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            if (soughtObject == null)
                yield break;

            soughtObject.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (soughtObject != null)
        {
            soughtObject.transform.position = targetPosition;
            soughtObject.transform.SetParent(thimbles[correctThimbleIndex].transform);
            soughtObject.SetActive(false);
        }
    }

    private IEnumerator ShuffleThimbles()
    {
        if (soughtObject == null)
            yield break;

        soughtObject.SetActive(true);

        int shuffleCount = 5;
        float shuffleDuration = 0.5f;

        if (audioSource != null && shuffleLoopClip != null)
        {
            audioSource.clip = shuffleLoopClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        for (int i = 0; i < shuffleCount; i++)
        {
            int indexA = Random.Range(0, thimbles.Count);
            int indexB = (indexA + 1) % thimbles.Count;

            yield return StartCoroutine(SwapThimbles(indexA, indexB, shuffleDuration));
            shuffleDuration *= 0.9f;
        }

        if (audioSource != null && audioSource.clip == shuffleLoopClip)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    private IEnumerator SwapThimbles(int indexA, int indexB, float duration)
    {
        Vector2 startPosA = thimbles[indexA].transform.position;
        Vector2 startPosB = thimbles[indexB].transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (thimbles[indexA] == null || thimbles[indexB] == null)
                yield break;

            thimbles[indexA].transform.position = Vector2.Lerp(startPosA, startPosB, elapsed / duration);
            thimbles[indexB].transform.position = Vector2.Lerp(startPosB, startPosA, elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        thimbles[indexA].transform.position = startPosB;
        thimbles[indexB].transform.position = startPosA;

        if (correctThimbleIndex == indexA)
            correctThimbleIndex = indexB;
        else if (correctThimbleIndex == indexB)
            correctThimbleIndex = indexA;
    }

    public void SetPlayerChoice(bool isCorrect)
    {
        if (!mechanicActive)
            return;

        if (isCorrect)
        {
            correctChoices++;
            Debug.Log($"Mechanic5_5: правильный выбор. Счет = {correctChoices}");

            if (audioSource != null && correctChoiceClip != null)
                audioSource.PlayOneShot(correctChoiceClip);
        }
        else
        {
            mechanicActive = false;
            Debug.Log("Mechanic5_5: неправильный выбор. Механика завершена.");

            if (audioSource != null && wrongChoiceClip != null)
                audioSource.PlayOneShot(wrongChoiceClip);
        }

        playerMadeChoice = true;
        EnableThimbleInteraction(false);
    }

    private void EnableThimbleInteraction(bool enable)
    {
        foreach (GameObject thimble in thimbles)
        {
            if (thimble == null)
                continue;

            Collider2D collider = thimble.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = enable;
        }
    }

    private void EndMechanic()
    {
        DestroyThimbles();

        if (correctChoices < repetitions && soughtObject != null)
            Destroy(soughtObject);

        soughtObject = null;
        StopAllCoroutines();
    }

    private void DestroyThimbles()
    {
        foreach (GameObject thimble in thimbles)
        {
            if (thimble != null)
                Destroy(thimble);
        }

        thimbles.Clear();
        thimblePositions.Clear();
    }

    private void TransformSoughtObjectToWeakSpot()
    {
        if (soughtObject == null)
            return;

        Vector2 weakSpotPosition = soughtObject.transform.position;
        Destroy(soughtObject);
        soughtObject = null;

        GameObject weakSpot = Instantiate(weakSpotPrefab, weakSpotPosition, Quaternion.identity);
        WeakSpot_2 weakSpotScript = weakSpot.GetComponent<WeakSpot_2>();

        if (weakSpotScript == null)
        {
            Debug.LogError("Mechanic5_5: на weakSpotPrefab нет WeakSpot_2.");
            return;
        }

        weakSpotScript.OnDestroyed += () =>
        {
            if (bossController != null)
                bossController.TakeDamage(1);

            mechanicActive = false;
        };

        Debug.Log("Mechanic5_5: weak spot создан.");
    }

    private IEnumerator BlinkThimbles(int blinkCount, float blinkDuration)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            foreach (GameObject thimble in thimbles)
            {
                if (thimble == null) continue;

                SpriteRenderer renderer = thimble.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.enabled = false;
            }

            yield return new WaitForSeconds(blinkDuration);

            foreach (GameObject thimble in thimbles)
            {
                if (thimble == null) continue;

                SpriteRenderer renderer = thimble.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.enabled = true;
            }

            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private void CleanupObjects()
    {
        DestroyThimbles();

        if (soughtObject != null)
            Destroy(soughtObject);

        soughtObject = null;
    }
}
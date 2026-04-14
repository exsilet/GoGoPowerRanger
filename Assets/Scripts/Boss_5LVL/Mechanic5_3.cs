using System.Collections;
using UnityEngine;

public class Mechanic5_3 : MonoBehaviour, IBossMechanic
{
    [SerializeField] private Transform boss;
    [SerializeField] private GameObject followerPrefab;
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private AudioSource hitSound;

    [SerializeField] private int repetitions = 3;
    [SerializeField] private float bossMoveDuration = 2f;
    [SerializeField] private float followerSpeed = 2f;
    [SerializeField] private float repetitionTimeout = 8f;
    [SerializeField] private float targetHitDistance = 0.5f;

    private int currentRepetition = 0;
    private GameObject currentFollower;
    private GameObject currentTarget;

    private void Awake()
    {
        if (boss == null)
            boss = transform;

        if (player == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
                player = pc.transform;
        }
    }

    public IEnumerator Execute()
    {
        if (boss == null || followerPrefab == null || targetPrefab == null || player == null)
        {
            Debug.LogError("Mechanic5_3: не назначены обязательные ссылки.");
            yield break;
        }

        CleanupCurrentObjects();

        yield return StartCoroutine(MoveBossToUpperRight());

        for (int i = 0; i < repetitions; i++)
        {
            currentRepetition = i + 1;

            CreateFollower();
            yield return new WaitForSeconds(0.5f);
            CreateTarget();

            float elapsed = 0f;
            while (elapsed < repetitionTimeout)
            {
                if (currentFollower == null && currentTarget == null)
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= repetitionTimeout)
            {
                Debug.LogWarning($"Mechanic5_3: timeout на повторе {currentRepetition}. Принудительно завершаем повтор.");
                CleanupCurrentObjects();
            }

            yield return new WaitForSeconds(0.2f);
        }

        CleanupCurrentObjects();
        Debug.Log("Механика 5_3 завершена.");
    }

    private IEnumerator MoveBossToUpperRight()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Mechanic5_3: Camera.main не найдена.");
            yield break;
        }

        Vector2 upperRightPosition = new Vector2(
            cam.orthographicSize * cam.aspect - 1f,
            cam.orthographicSize - 1f
        );

        Vector2 startPosition = boss.position;
        float elapsedTime = 0f;

        while (elapsedTime < bossMoveDuration)
        {
            boss.position = Vector2.Lerp(startPosition, upperRightPosition, elapsedTime / bossMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        boss.position = upperRightPosition;
    }

    private void CreateFollower()
    {
        if (currentFollower != null)
            Destroy(currentFollower);

        currentFollower = Instantiate(followerPrefab, boss.position, Quaternion.identity);
        StartCoroutine(FollowPlayer(currentFollower));
    }

    private IEnumerator FollowPlayer(GameObject follower)
    {
        while (follower != null && player != null)
        {
            Vector2 direction = ((Vector2)player.position - (Vector2)follower.transform.position).normalized;
            follower.transform.position += (Vector3)(direction * followerSpeed * Time.deltaTime);

            if (currentTarget != null &&
                Vector2.Distance(follower.transform.position, currentTarget.transform.position) <= targetHitDistance)
            {
                if (hitSound != null)
                    hitSound.Play();

                Destroy(follower);
                Destroy(currentTarget);

                currentFollower = null;
                currentTarget = null;

                Debug.Log($"Mechanic5_3: попадание в цель. Повтор {currentRepetition} завершен.");
                yield break;
            }

            yield return null;
        }

        if (follower != null && currentFollower == follower)
            currentFollower = null;
    }

    private void CreateTarget()
    {
        if (currentTarget != null)
            Destroy(currentTarget);

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Mechanic5_3: Camera.main не найдена.");
            return;
        }

        float xMin = -cam.orthographicSize * cam.aspect + 1f;
        float xMax = cam.orthographicSize * cam.aspect - 1f;
        float yMin = -cam.orthographicSize + 1f;
        float yMax = cam.orthographicSize - 1f;

        Vector2 targetPosition = new Vector2(
            Random.Range(xMin + 1f, xMax - 1f),
            Random.Range(yMin + 1f, yMax - 1f)
        );

        currentTarget = Instantiate(targetPrefab, targetPosition, Quaternion.identity);
        StartCoroutine(BlinkTarget(currentTarget, 2));
    }

    private IEnumerator BlinkTarget(GameObject target, int blinks)
    {
        if (target == null)
            yield break;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null)
            yield break;

        for (int i = 0; i < blinks; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(0.2f);

            if (sr != null)
                sr.enabled = true;

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void CleanupCurrentObjects()
    {
        if (currentFollower != null)
            Destroy(currentFollower);

        if (currentTarget != null)
            Destroy(currentTarget);

        currentFollower = null;
        currentTarget = null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController_3 : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float targetX = 0f;
    [SerializeField] private int bossHP = 5;

    private readonly List<IBossMechanic> mechanics = new List<IBossMechanic>();
    private bool isDead;

    private void Start()
    {
        TryAddMechanic(GetComponent<Mechanic5_1>());
        TryAddMechanic(GetComponent<Mechanic5_2>());
        TryAddMechanic(GetComponent<Mechanic5_3>());
        TryAddMechanic(GetComponent<Mechanic5_4>());
        TryAddMechanic(GetComponent<Mechanic5_5>());

        if (mechanics.Count < 5)
        {
            Debug.LogError("BossController_3: не все механики найдены на боссе.");
            enabled = false;
            return;
        }

        StartCoroutine(MoveToPositionX());
    }

    private void TryAddMechanic(IBossMechanic mechanic)
    {
        if (mechanic != null)
            mechanics.Add(mechanic);
    }

    private IEnumerator MoveToPositionX()
    {
        while (transform.position.x > targetX)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(targetX, transform.position.y, transform.position.z),
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        StartCoroutine(BossBehaviorCycle());
    }

    private IEnumerator BossBehaviorCycle()
    {
        while (bossHP > 0 && !isDead)
        {
            yield return StartCoroutine(mechanics[0].Execute());

            List<int> mechanicsOrder = GenerateRandomOrder();
            foreach (int mechanicIndex in mechanicsOrder)
            {
                if (isDead) yield break;

                yield return StartCoroutine(mechanics[mechanicIndex].Execute());

                if (isDead) yield break;

                yield return StartCoroutine(mechanics[0].Execute());
                yield return new WaitForSeconds(0.1f);
            }

            if (isDead) yield break;

            yield return StartCoroutine(mechanics[4].Execute());

            if (bossHP > 0)
                yield return new WaitForSeconds(1f);
        }

        if (!isDead)
        {
            Debug.Log("Босс побежден!");
            isDead = true;
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        bossHP -= damage;
        Debug.Log("Босс получил урон! Оставшееся здоровье: " + bossHP);

        if (bossHP <= 0)
        {
            isDead = true;
            Debug.Log("Босс побежден!");
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }

    private List<int> GenerateRandomOrder()
    {
        List<int> mechanicsOrder = new List<int> { 1, 2, 3 };

        for (int i = 0; i < mechanicsOrder.Count; i++)
        {
            int randomIndex = Random.Range(i, mechanicsOrder.Count);
            int temp = mechanicsOrder[i];
            mechanicsOrder[i] = mechanicsOrder[randomIndex];
            mechanicsOrder[randomIndex] = temp;
        }

        return mechanicsOrder;
    }
}
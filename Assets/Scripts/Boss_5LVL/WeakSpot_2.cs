using UnityEngine;
using System;

public class WeakSpot_2 : MonoBehaviour
{
    public event Action OnDestroyed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.isBoosting)
        {
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}
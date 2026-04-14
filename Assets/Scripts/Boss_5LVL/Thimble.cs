using UnityEngine;

public class Thimble : MonoBehaviour
{
    private Mechanic5_5 mechanic;
    private bool isCorrectThimble;

    public void Initialize(Mechanic5_5 mechanicRef, bool isCorrect)
    {
        mechanic = mechanicRef;
        isCorrectThimble = isCorrect;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (mechanic == null)
            return;

        if (collision.CompareTag("Player"))
            mechanic.SetPlayerChoice(isCorrectThimble);
    }
}
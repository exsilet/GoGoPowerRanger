using UnityEngine;

public class Thimble : MonoBehaviour
{
    private Mechanic5_5 mechanic;
    private bool isCorrectThimble;

    // Инициализация наперстка, задаем, правильный ли это наперсток
    public void Initialize(Mechanic5_5 mechanic, bool isCorrect)
    {
        this.mechanic = mechanic;
        isCorrectThimble = isCorrect;
    }

    // Метод для обработки столкновения с игроком
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Сообщаем основной механике, был ли выбран правильный наперсток
            mechanic.SetPlayerChoice(isCorrectThimble);
        }
    }
}

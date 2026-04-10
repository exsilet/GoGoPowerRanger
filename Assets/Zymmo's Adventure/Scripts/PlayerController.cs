using System;
using UnityEngine;

public class PlayerController1 : MonoBehaviour
{
    public Action OnPlayerUpdate;

    [SerializeField] private float Speed = 0.5f;

    private SpriteRenderer SpriteRenderer;
    private bool Grounded;
    private bool Jumping;

    private float HorizontalAxis;
    private float VerticalAxis;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        var NewValue = Input.GetAxis("Horizontal");

        if (NewValue != 0f)
        {
            HorizontalAxis = NewValue;

            SpriteRenderer.flipX = HorizontalAxis < 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Grounded)
            {
                VerticalAxis = 1f;
            
                Jumping = true;
                Grounded = false;
            }
        }

        if (Jumping)
        {
            if (Grounded)
            {
                VerticalAxis = 0f;

                Jumping = false;
            }
        }
    }

    private void FixedUpdate()
    {
        transform.position =
            new Vector3(
                transform.position.x + HorizontalAxis * Speed,
                transform.position.y + VerticalAxis * Speed
                );


        OnPlayerUpdate?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D Collision)
    {
        if (!Collision.gameObject.CompareTag("Ground"))
            return;

        Grounded = true;
    }

    private void OnCollisionExit2D(Collision2D Collision)
    {
        if (!Collision.gameObject.CompareTag("Ground"))
            return;

        Grounded = false;
    }
}

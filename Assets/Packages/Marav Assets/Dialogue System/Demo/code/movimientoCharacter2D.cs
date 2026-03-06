using Unity.Mathematics;
using UnityEngine;

namespace MaravStudios.DialogueSystem
{
    public class movimientoCharacter2D : MonoBehaviour
    {
        public float velocidadMovimiento = 5f;

        public Rigidbody2D rb;
        float direccionMovimiento = 0f;
        public Animator animator;
        public SpriteRenderer spriteRenderer;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            Mover();

            if (direccionMovimiento > 0)
            {
                spriteRenderer.flipX = true;
                animator.SetBool("walk", true);
            }
            else if (direccionMovimiento < 0)
            {
                spriteRenderer.flipX = false;
                animator.SetBool("walk", true);
            }
            else
            {
                animator.SetBool("walk", false);
            }

        }

        private void Mover()
        {
            direccionMovimiento = Input.GetAxis("Horizontal");
            rb.linearVelocity = new Vector2(direccionMovimiento * velocidadMovimiento, rb.linearVelocity.y);
        }
    }
}
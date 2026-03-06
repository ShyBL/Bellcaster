using UnityEngine;
using UnityEngine.Events;

namespace MaravStudios.DialogueSystem
{
    public class eventosOnTrigger : MonoBehaviour
    {
        public string target = "Player";
        public UnityEvent triggerEnter, triggerStay, triggerExit;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(target))
            {
                triggerEnter.Invoke();
            }
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag(target))
            {
                triggerStay.Invoke();
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(target))
            {
                triggerExit.Invoke();
            }
        }
    }
}
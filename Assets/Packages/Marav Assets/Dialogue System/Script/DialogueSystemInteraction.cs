using System;
using UnityEngine;
using UnityEngine.Events;
namespace MaravStudios.DialogueSystem
{
    [RequireComponent(typeof(DialogueSystemTrigger))]

    public class DialogueSystemInteraction : MonoBehaviour
    {
        public DialogueSystemTrigger dialogueSystemTrigger;
        bool ready;
        [Header("Elements")]
        [Tooltip("Is activated and deactivated when an object with the “Player” tag enters or exits.")]
        public GameObject signal;
        [Header("Execute")]
        public bool activateWhenEntering = false;
        public DialogueSystemInteraction_Button executeButton = DialogueSystemInteraction_Button.Jump;
        [Serializable]
        public enum DialogueSystemInteraction_Button
        {
            Jump,
            Submit,
            Fire1,
            Fire2,
            Fire3
            // Agrega más etiquetas aquí según sea necesario
        }
        public DialogueSystemInteraction_Tags executeTag = DialogueSystemInteraction_Tags.Player;
        [Serializable]
        public enum DialogueSystemInteraction_Tags
        {
            Player,
            Enemy
            // Agrega más etiquetas aquí según sea necesario
        }
        [Header("Trigger")]
        public dimenciones dimensions;
        public Vector3 boxSize = Vector3.one;
        Vector3 direction = Vector3.zero;
        public LayerMask layerMask = -1;

        [Header("Events")]

        public UnityEvent Interaction;
        public enum dimenciones
        {
            _2D,
            _3D
        }

        private void Start()
        {
            dialogueSystemTrigger = GetComponent<DialogueSystemTrigger>();
            if (signal != null)
                signal.SetActive(false);
        }
        private void Inside(Collider2D collision)
        {
            if (collision.CompareTag(executeTag.ToString()))
            {
                if (signal != null)
                    signal.SetActive(true);
                if (activateWhenEntering && !ready) ejecutar();
                ready = true;

            }

        }
        private void Inside(Collider collision)
        {
            if (collision.CompareTag(executeTag.ToString()))
            {
                if (signal != null)
                    signal.SetActive(true);

                if (activateWhenEntering && !ready) ejecutar();
                ready = true;
            }
        }
        private void Exit()
        {
            if (signal != null)
                signal.SetActive(false);
            ready = false;

        }
        void ejecutar()
        {
            dialogueSystemTrigger.Z_TriggerTheDialogue();
            Interaction.Invoke();
        }
        private void Update()
        {
            if (Input.GetButtonDown(executeButton.ToString()) && ready)
            {
                ejecutar();
            }
        }
        void LateUpdate()
        {



            switch (dimensions)
            {
                case dimenciones._2D:
                    RaycastHit2D hit2d = Physics2D.BoxCast(transform.position, boxSize, 0, direction, 0, layerMask);
                    if (hit2d.collider)
                    {
                        Inside(hit2d.collider);
                    }
                    else if (ready)
                    {
                        Exit();
                    }
                    break;
                case dimenciones._3D:
                    RaycastHit hit3d;
                    bool hit = Physics.BoxCast(transform.position, boxSize / 2, direction, out hit3d, Quaternion.identity, 0, layerMask);
                    if (hit)
                    {
                        Inside(hit3d.collider);
                    }
                    else if (ready)
                    {
                        Exit();
                    }

                    break;
                default:
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            switch (dimensions)
            {
                case dimenciones._2D:
                    Gizmos.DrawWireCube(transform.position, new Vector2(boxSize.x, boxSize.y));
                    break;
                case dimenciones._3D:
                    Gizmos.DrawWireCube(transform.position, boxSize);
                    break;
                default:
                    break;
            }
        }



    }
}

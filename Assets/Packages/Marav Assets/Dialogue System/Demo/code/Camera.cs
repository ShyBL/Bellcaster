using UnityEngine;

namespace MaravStudios.DialogueSystem { 

    public class Camera : MonoBehaviour
    {

        public Transform player;
        public float velocidad = 1;
        public Vector3 offset;
        void Update()
        {
            Vector3 a = (offset + (player.position - transform.position)) * velocidad * Time.deltaTime;
            if(a.magnitude > 0.0001f)
                transform.Translate(a);
        }
    }
}
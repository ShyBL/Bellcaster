//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;

namespace Esper.FeelSpeak.Examples
{
    public class PingPongMovement : MonoBehaviour
    {
        /// <summary>
        /// Movement speed.
        /// </summary>
        public float speed = 2f;

        /// <summary>
        /// Total distance to move.
        /// </summary>
        public float distance = 100f;

        /// <summary>
        /// The starting position.
        /// </summary>
        private Vector3 startPosition;

        void Start()
        {
            startPosition = transform.position;
        }

        void Update()
        {
            float offset = Mathf.PingPong(Time.time * speed, distance);
            transform.position = startPosition + transform.forward * offset;
        }
    }
}
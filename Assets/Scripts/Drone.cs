namespace GoogleARCore.ARDDPS
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Drone : MonoBehaviour
    {
        public GameObject Building1;
        public GameObject Building2;
        public float speed = 2f;
        bool move = false;

        public void BeginMove()
        {
            move = true;
        }

        public void StopMove()
        {
            move = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (move)
            {
                float step = speed * Time.deltaTime;
                Vector3 mover = transform.position;
                mover.x += 1;
                transform.position = Vector3.MoveTowards(transform.position, mover, step);
            }
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Nathan
{
    public class DoorController : MonoBehaviour
    {
        Animator animator;
        
        public bool open = false;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (open != animator.GetBool("open") )
            {
                animator.SetBool("open", open);
            }

            
        }
    }
}
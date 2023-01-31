using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Nathan
{
    public class NathanController : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] NavMeshAgent agent;

        Animator animator;

        DoorController[] door_controllers;

        // Vector3 bed_pos = new Vector3(5.15f, 0.5803499f, 2.08f);
        // Vector3 outside_pos = new Vector3(6.0f, 0.5803499f, -6.26f);
        // Vector3 door_pos = new Vector3(2.84f, 0.5803499f, -7.46f);
        // Vector3 pills_pos = new Vector3(4.81f, 0.5803499f, -4.65f);
        // Vector3 pre_pills_pos = new Vector3(3.81f, 0.5803499f, -4.65f);
        // Vector3 couch_pos = new Vector3(-.88f, 0.5803499f, -1.71f);
        public Transform bed_pos;
        public Transform outside_pos;
        public Transform door_pos;
        public Transform pills_pos;
        public Transform pre_pills_pos;
        public Transform couch_pos;
        private int protocol = -1;
        private int loc = -1;
        private bool waited = true;
        private long wait_until = 0;
        private bool animation_played = false;

        void Start()
        {
            animator = GetComponent<Animator>();
            door_controllers = FindObjectsOfType<DoorController>();
        }

        void midnightRunAway()
        {
            if (loc == 0)
            {
                agent.SetDestination(door_pos.position);
                animation_played = false;
            }
            else if (loc == 1)
            {
                if (!animation_played)
                {
                    door_controllers[0].open = true;
                    animator.SetBool("open_door", true);
                    animation_played = true;
                }
            }
            else if (loc == 2)
            {
                agent.SetDestination(outside_pos.position);
            }
            else
            {
                loc = -1;
            }
        }

        void midnightGoDoorThenBed()
        {
            if (loc == 0)
            {
                agent.SetDestination(door_pos.position);
            }
            else if (loc == 1)
            {
                agent.SetDestination(bed_pos.position);
            }
            else
            {
                loc = -1;
            }
        }

        void midnightOpenDoorThenBed()
        {
            if (loc == 0)
            {
                agent.SetDestination(door_pos.position);
                animation_played = false;
            }
            else if (loc == 1)
            {
                if (!animation_played)
                {
                    door_controllers[0].open = true;
                    animator.SetBool("open_door", true);
                    animation_played = true;
                }
            }
            else if (loc == 2)
            {
                animation_played = false;
                loc++;
            }
            else if (loc == 3)
            {
                if (!animation_played)
                {
                    door_controllers[0].open = false;
                    animator.SetBool("open_door", true);
                    animation_played = true;
                }
            }
            else if (loc == 4)
            {
                agent.SetDestination(bed_pos.position);
            }
            else
            {
                loc = -1;
            }
        }

        void takePills()
        {
            if (loc == 0)
            {
                agent.SetDestination(couch_pos.position);
            } else if (loc == 1)
            {
                agent.SetDestination(pre_pills_pos.position);
            }
            else if (loc == 2)
            {
                agent.SetDestination(pills_pos.position);
                animation_played = false;
            }
            else if (loc == 3)
            {
                if (!animation_played)
                {
                    animator.SetBool("take_pills", true);
                    animation_played = true;
                }
            }
            else if (loc == 4)
            {
                agent.SetDestination(couch_pos.position);
            }
            else
            {
                loc = -1;
            }
        }

        void recover()
        {
            if (loc == 0)
            {
                agent.SetDestination(bed_pos.position);
            }
            else if (loc == 1)
            {
                door_controllers[0].open = false;
                loc = -1;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.RightArrow)&& Input.GetKey(KeyCode.RightShift))
            {
                var tmp = agent.transform.eulerAngles;
                tmp.y += 5f;
                agent.transform.eulerAngles = tmp;
            }
            
            if (Input.GetKey(KeyCode.LeftArrow)&& Input.GetKey(KeyCode.RightShift))
            {
                var tmp = agent.transform.eulerAngles;
                tmp.y -= 5f;
                agent.transform.eulerAngles = tmp;
            }
            
            if (Input.GetKey(KeyCode.UpArrow)&& Input.GetKey(KeyCode.RightShift))
            {
                var tmp = agent.transform.forward; 
                tmp = 0.02f*tmp.normalized;
                agent.transform.position += tmp;
                // tmp = 1.0f*tmp.normalized;
                agent.velocity = tmp;
                animator.SetFloat("speed", 1.0f);
                agent.enabled = false;
                return;
            }
            
            agent.enabled = true;
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (milliseconds > wait_until && Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.RightShift))
            {
                agent.SetDestination(door_pos.position);
                wait_until = milliseconds + 1000;
                return;
            }
            if (milliseconds > wait_until && Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.RightShift))
            {
                agent.SetDestination(outside_pos.position);
                wait_until = milliseconds + 1000;
                return;
            }
            if (milliseconds > wait_until && Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.RightShift))
            {
                agent.SetDestination(couch_pos.position);
                wait_until = milliseconds + 1000;
                return;
            }
            if (milliseconds > wait_until && Input.GetKey(KeyCode.B) && Input.GetKey(KeyCode.RightShift))
            {
                agent.SetDestination(bed_pos.position);
                wait_until = milliseconds + 1000;
                return;
            }
            if (milliseconds > wait_until && Input.GetKey(KeyCode.K) && Input.GetKey(KeyCode.RightShift))
            {
                if ((agent.destination - pre_pills_pos.position).magnitude > 1)
                {
                    agent.SetDestination(pre_pills_pos.position);
                }
                else
                {
                    agent.SetDestination(pills_pos.position);
                }
                wait_until = milliseconds + 1000;
                return;
            }
            if (milliseconds > wait_until && Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.RightShift))
            {
                animator.SetBool("take_pills", true);
                wait_until = milliseconds + 1000;
                return;
            }
            if (milliseconds > wait_until && Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.RightShift))
            {
                door_controllers[0].open = !door_controllers[0].open;
                animator.SetBool("open_door", true);
                wait_until = milliseconds + 1000;
                return;
            }
   
            
            if (milliseconds > wait_until)
            {
                // Avoid any reload.
                waited = false;
                if (protocol==-1 && Input.GetKey(KeyCode.Space))
                {
                    protocol = 0;
                }

                if (loc == -1 && protocol != -1)
                {
                    protocol = (protocol + 1) % 8;
                    // agent.SetDestination(bed_pos);
                    // door_controllers[0].open = false;
                    loc++;
                    return;
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    loc++;
                    wait_until = milliseconds + 1000;
                    // Debug.Log("next");
                }

                if (protocol == 2 && loc >= 0)
                {
                    midnightGoDoorThenBed();
                }    
                else if (protocol == 4 && loc >= 0)
                {
                    midnightOpenDoorThenBed();
                }
                else if (protocol == 6 && loc >= 0)
                {
                    midnightRunAway();
                }
                else if (protocol == 0 && loc >= 0)
                {
                    takePills();
                }
                else
                {
                    recover();
                }
            }
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                // waited = true;
                animator.SetBool("take_pills", false);
                animator.SetBool("open_door", false);
                // animator.SetFloat("speed", 0);
            }


            animator.SetFloat("speed", agent.velocity.magnitude);
        }
    }
}
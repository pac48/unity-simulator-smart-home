using System;
using UnityEngine;

public class GridController : MonoBehaviour
{
    private MeshRenderer mr;
    private double cur_time;

    private void Start()
    {
        mr = GetComponent<MeshRenderer>();
        mr.enabled = false;
        cur_time = Time.time;
    }

    void Update()
    {
        if (Time.time - cur_time < 1)
        {
            return;
        }

        if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.LeftControl))
        {
            mr.enabled = !mr.enabled;
            cur_time = Time.time;
        }
    }
}
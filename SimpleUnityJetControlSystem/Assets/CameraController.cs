using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform[] povs;
    [SerializeField] private float speed;

    private int index = 0;
    private Vector3 target;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            index ++;
            if (povs.Length - 1 < index)
            {
                index = 0;
            }
        }

        target = povs[index].position;
    }

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        transform.forward = povs[index].forward;
    }
}

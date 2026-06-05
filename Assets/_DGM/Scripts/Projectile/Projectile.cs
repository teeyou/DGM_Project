using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private bool _isMinusSpeed = false;
    //[SerializeField] private bool _leftOrRight = true;

    void Update()
    {
        //if (_leftOrRight)
        //{
        //    // 적군
        //    transform.rotation = Quaternion.Euler(0f, 0f, 270f);
            
        //}
        //else
        //{
        //    // 아군
        //    transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        //}
        if (_isMinusSpeed)
        {
            transform.position += transform.up * _speed * Time.deltaTime;
        }
        else
        {
            transform.position += transform.up * -_speed * Time.deltaTime;
        }
    }
}

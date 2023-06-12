using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TonyDev
{
    public class PopupMovement : MonoBehaviour
    {
        private float _xMagnitude;
        public float speed;

        private void OnEnable()
        {
            _xMagnitude = Random.Range(-1, 2);
        }

        private void Update()
        {
            transform.position += new Vector3(_xMagnitude * speed, speed, 0) * Time.deltaTime;
        }
    }
}

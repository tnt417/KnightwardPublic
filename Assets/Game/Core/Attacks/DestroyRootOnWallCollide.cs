using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TonyDev
{
    public class DestroyRootOnWallCollide : MonoBehaviour
    {
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Level"))
            {
                Destroy(gameObject.transform.root.gameObject);
            }
        }
    }
}

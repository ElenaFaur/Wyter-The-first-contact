using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyPlatform : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D _collision)
    {
        if (_collision.gameObject.name == "Player")
        {
            _collision.gameObject.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D _collision)
    {
        if (_collision.gameObject.name == "Player")
        {
            _collision.gameObject.transform.SetParent(null);
        }
    }
}

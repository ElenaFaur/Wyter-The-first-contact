using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D player_rb;

    // Start is called before the first frame update
    private void Start()
    {
        player_rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        float dirX = Input.GetAxisRaw("Horizontal");
        player_rb.velocity = new Vector2(dirX * 7f, player_rb.velocity.y);

        if(Input.GetButtonDown("Jump"))
        {
            player_rb.velocity = new Vector2(player_rb.velocity.x, 14f);
        }
    }
}

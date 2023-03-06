using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerCharacter2D player;

    [HideInInspector]
    public float h, v;

    [HideInInspector]
    public bool startJump;

    [HideInInspector]
    public bool endJump;

    [HideInInspector]
    public bool dash;

    [HideInInspector]
    public bool vengefulSpirit;     // ¸´³ðÖ®»ê

    [HideInInspector]
    public bool howlingWraiths;     // º¿½ÐÓÄÁé

    [HideInInspector]
    public bool normalSlash;

    [HideInInspector]
    public bool upSlash;

    [HideInInspector]
    public bool downSlash;

    private void Start()
    {
        player = GetComponent<PlayerCharacter2D>();
    }

    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        startJump = Input.GetKeyDown(KeyCode.Z);
        endJump = Input.GetKeyUp(KeyCode.Z);
        dash = Input.GetKeyDown(KeyCode.C);
        normalSlash = Input.GetKeyDown(KeyCode.X);
        downSlash = Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.DownArrow);
        upSlash = Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.UpArrow);
        if (Input.GetKeyDown(KeyCode.F))
        {
            // º¿½ÐÓÄÁé
            if (Input.GetKeyDown(KeyCode.UpArrow)) { howlingWraiths = true; }
            // ¸´³ðÖ®»ê
            else { howlingWraiths = true; }
        }
    }
}
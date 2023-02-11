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
    public bool normalSlash;
    [HideInInspector]
    public bool upSlash;
    [HideInInspector]
    public bool downSlash;
    void Start()
    {
        player = GetComponent<PlayerCharacter2D>();
    }

    void Update()
    {
        h = Input.GetAxis("Horizontal");
        startJump = Input.GetKeyDown(KeyCode.Z);
        endJump = Input.GetKeyUp(KeyCode.Z);
        if (Input.GetKeyDown(KeyCode.X))
        {
            // ����
            if (Input.GetKey(KeyCode.DownArrow)) { downSlash = true; }
            // ����
            else if (Input.GetKey(KeyCode.UpArrow)) { upSlash = true; }
            // ��ͨ����
            else { normalSlash = true; }
        }
    }
}

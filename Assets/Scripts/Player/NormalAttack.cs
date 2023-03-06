using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttack : MonoBehaviour
{
    public ActionEffect prefabSlashImpact;          // ������Ч
    public ActionEffect prefabSwordHit;             // ����������Ч

    private PlayerCharacter2D player;

    private void Awake()
    {
        player = GetComponentInParent<PlayerCharacter2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BOSS"))
        {
            collision.GetComponent<HornetBoss>().GetHit(1);
            var impact = Instantiate(prefabSlashImpact, transform.position, Quaternion.identity);
            var attack = Instantiate(prefabSwordHit, transform.position, Quaternion.identity);
            if (player.faceLeft)
            {
                impact.transform.localScale = new Vector3(-1, 1, 1);
                attack.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }
}
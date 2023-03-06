using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disposable : MonoBehaviour
{
    public float lifeTime = 1.0f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
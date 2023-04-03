using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int dir;
    private void Start()
    {
        Destroy(gameObject, 3.5f);
    }

    private void Update()
    {
        transform.Translate(Vector2.right * 5 * Time.deltaTime * dir);
    }
}

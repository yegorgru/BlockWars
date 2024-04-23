using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Range(1, 10)]
    [SerializeField] private float speed = 10f;

    [Range(1, 10)]
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody2D rb;

    private int collisionsLifeTime = 2;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        Destroy(gameObject, lifeTime);
        rb.velocity = transform.up * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        --collisionsLifeTime;
        switch (collision.gameObject.tag)
        {
            case "Wood":
                Destroy(collision.gameObject);
                Destroy(gameObject);
                break;
            case "Enemy":
                Destroy(collision.gameObject);
                Destroy(gameObject);
                break;
            case "Hero":
                Destroy(collision.gameObject);
                Destroy(gameObject);
                break;
            case "Iron":
                Destroy(gameObject);
                break;
            case "Grass":
                break;
            case "Glass":
                ReflectBulletDirection(collision.contacts[0].normal);
                break;
            default:
                break;
        }
        if(collisionsLifeTime == 0)
        {
            Destroy(gameObject);
        }
    }

    private void ReflectBulletDirection(Vector2 normal)
    {
        Vector2 reflectedDirection = Vector2.Reflect(transform.up, normal);
        rb.velocity = reflectedDirection.normalized * speed;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShot : MonoBehaviour
{

    public SpriteRenderer selfSpriteRenderer;
    public Transform selfTransform;
    public CircleCollider2D selfCollider;
    public GameManager gm;
    public bool isActive;
    public bool moving;
    public Vector2 movementDirection;
    public float movementSpeed;
    public bool directedTowardsPlayer; // if true, player can bounce it back. if false, it will hurt the enemy.

    public Color[] colors;

    // Use this for initialization
    public void EnemyShotStart(GameManager gameManagerReference)
    {

        gm = gameManagerReference;
        if (!selfSpriteRenderer && GetComponent<SpriteRenderer>() != null) { selfSpriteRenderer = GetComponent<SpriteRenderer>(); }
        if (!selfTransform && GetComponent<Transform>() != null) { selfTransform = GetComponent<Transform>(); }

        isActive = true;

        SetColor(0);

    }

    // Update is called once per frame
    void Update()
    {

        if (!isActive) { return; }

        if (moving)
        {
            selfTransform.position = (Vector2)selfTransform.position + (movementDirection * movementSpeed * Time.deltaTime);
        }

        if (Vector2.Distance(selfTransform.position, gm.playerInput.pathRingTransform.position) > 5f)
        {
            gm.enemyShotPool.Despawn(selfTransform);
            gm.EnemyShotPassedPlayer();
        }


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (directedTowardsPlayer && collision.collider == gm.playerShip.selfCollider)
        {
            directedTowardsPlayer = false;
            movementDirection = movementDirection * -1f;
            selfTransform.localRotation = Quaternion.FromToRotation(Vector3.up, movementDirection);
            SetColor(1);
        }

        if (!directedTowardsPlayer && collision.collider == gm.enemyShip.selfCollider)
        {
            gm.enemyShotPool.Despawn(selfTransform);
            gm.EnemyShotHitEnemy();
        }
    }

    public void SetColor(int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= colors.Length) { return; }
        selfSpriteRenderer.color = colors[colorIndex];
    }
}

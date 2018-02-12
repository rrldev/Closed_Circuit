using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShot : MonoBehaviour
{

    public SpriteRenderer selfSpriteRenderer;
    public Transform selfTransform;
    public CircleCollider2D selfCollider;
    public GameManager gm;
    public bool isActive;
    public bool rotateWhileActive;
    public float rotateSpeed;
    public bool harmfulToPlayer;
    public bool moving;
    public Vector2 movementDirection;
    public float movementSpeed;
    public float spawnShipGap; // how far from player ship shot spawns before moving towards target.
    public bool hasCollidedWithGrowingCircle;
    public Color[] colors;

    // Use this for initialization
    public void PlayerShotStart(GameManager gameManagerReference)
    {

        gm = gameManagerReference;
        if (!selfSpriteRenderer && GetComponent<SpriteRenderer>() != null) { selfSpriteRenderer = GetComponent<SpriteRenderer>(); }
        if (!selfTransform && GetComponent<Transform>() != null) { selfTransform = GetComponent<Transform>(); }

        //harmfulToPlayer = false;

        SetColor(0);

        isActive = true;

        hasCollidedWithGrowingCircle = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) { return; }

        if (rotateWhileActive && selfTransform)
        {
            selfTransform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }

        if (moving)
        {
            selfTransform.position = (Vector2)selfTransform.position + (movementDirection * movementSpeed * Time.deltaTime);
        }

        //if (harmfulToPlayer && selfCollider.IsTouching(gm.playerShip.selfCollider) )
        //{
        //    Debug.Log("connect!");
        //    StopAndDeactivate();
        //    gm.playerShotPool.Despawn(selfTransform);
        //    gm.DamagePlayer();
        //}

        //if (!hasCollidedWithGrowingCircle && selfCollider.IsTouching(gm.growingCircleCollider))
        //{
        //    hasCollidedWithGrowingCircle = true;
        //    gm.ShotCollidedWithGrowingCircle();
        //}



    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.rigidbody == gm.edgeColliders)
        {
            StopAndDeactivate();
            gm.playerShotPool.Despawn(selfTransform);
        }

        if (harmfulToPlayer && collision.collider == gm.playerShip.selfCollider)
        {
            StopAndDeactivate();
            gm.playerShotPool.Despawn(selfTransform);
            gm.DamagePlayer();
        }

        if (!hasCollidedWithGrowingCircle && collision.collider == gm.growingCircleCollider)
        {
            hasCollidedWithGrowingCircle = true;
            gm.ShotCollidedWithGrowingCircle();
            SetColor(1);
        }
    }

    public void SetColor(int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= colors.Length) { return; }
        selfSpriteRenderer.color = colors[colorIndex];
    }

    void StopAndDeactivate()
    {
        moving = false;
        isActive = false;
    }
}

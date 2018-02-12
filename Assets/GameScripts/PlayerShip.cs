using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShip : MonoBehaviour
{

    public GameManager gm;
    public Transform selfTransform;
    public SpriteRenderer selfSprite;
    public CircleCollider2D selfCollider;

    public float topSpeed;

    public Transform playerShotPrefab; // bullets that player ship shoots.



    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }
}

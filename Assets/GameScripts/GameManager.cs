using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public float shipDistance;
    public float fireRate;
    float lastFireTime;
    public PlayerShip playerShip;
    public PlayerInput playerInput;

    Vector2 newShipPosition;
    float shipAngle;
    bool shipMovingCounterClockwise;
    float shipMoveAngle;

    public GOPOOL2 playerShotPool;
    public GOPOOL2 enemyShotPool;

    public int playerTotalHealth;
    public int enemyTotalHealth;
    public int baseTotalHealth;
    int playerCurrentHealth;
    int enemyCurrentHealth;
    int baseCurrentHealth;

    public Image enemyHealthBar;
    float enemyHealthDefaultSize;

    public EnemyShip enemyShip;
    public float enemyAttackInterval;
    public float lastEnemyAttack;
    float lastEnemyShotAngle;
    int enemyDamageFromShot;

    public Transform growingCircleTransform;
    public CircleCollider2D growingCircleCollider;
    float growingCircleScale;
    public float growingCircleRate;
    public float growingCircleReduction;
    public float growingCircleEnemyDamage; // how much circle grows when player misses enemy shot
    public float growingCirclePlayerDamage; // how much circle grows when player gets hit by their own shot

    public Rigidbody2D edgeColliders;

    float playerShotSpeed;
    float enemyShotSpeed;

    public bool levelActive;

    public Text levelCompleteText;
    public Transform mousePointerTransform;


    // Use this for initialization
    void Start()
    {
        playerShotPool = new GOPOOL2(playerShip.playerShotPrefab, "playerShotPool", 40, true, null);
        enemyShotPool = new GOPOOL2(enemyShip.enemyShotTransform, "enemyShotPool", 20, true, null);
        baseCurrentHealth = baseTotalHealth;
        enemyCurrentHealth = enemyTotalHealth;
        playerCurrentHealth = playerTotalHealth;
        enemyHealthDefaultSize = enemyHealthBar.rectTransform.sizeDelta.x;

        StartLevel(0);
    }

    // Update is called once per frame
    void Update()
    {

        SetPlayerShipPosition();

        CheckEnemyAttack();

        ChangeGrowingCircleScale(growingCircleRate * Time.deltaTime);

        SetGrowingCircleScale();
    }

    public void StartLevel(int difficulty = 0)
    {
        if (difficulty == 0)
        {
            enemyAttackInterval = 2.5f;
            growingCircleRate = 0.05f;
            growingCircleReduction = 0.02f;
            growingCircleEnemyDamage = 0.04f;
            growingCirclePlayerDamage = 0.04f;
            playerShotSpeed = 2f;
            enemyShotSpeed = 0.8f;
            fireRate = 0.25f;
            playerShip.topSpeed = 2f;
            enemyTotalHealth = 100;
        }
        else if (difficulty == 1)
        {
            enemyAttackInterval = 1.8f;
            growingCircleRate = 0.06f;
            growingCircleReduction = 0.02f;
            growingCircleEnemyDamage = 0.15f;
            growingCirclePlayerDamage = 0.15f;
            playerShotSpeed = 2f;
            enemyShotSpeed = 0.8f;
            fireRate = 0.25f;
            playerShip.topSpeed = 2f;
            enemyTotalHealth = 150;
        }
        else if (difficulty == 2)
        {
            enemyAttackInterval = 1.5f;
            growingCircleRate = 0.075f;
            growingCircleReduction = 0.015f;
            growingCircleEnemyDamage = 0.175f;
            growingCirclePlayerDamage = 0.175f;
            playerShotSpeed = 2.4f;
            enemyShotSpeed = 1f;
            fireRate = 0.15f;
            playerShip.topSpeed = 2.5f;
            enemyTotalHealth = 180;
        }
        else if (difficulty == 3)
        {
            enemyAttackInterval = 1.0f;
            growingCircleRate = 0.25f;
            growingCircleReduction = 0.05f;
            growingCircleEnemyDamage = 0.2f;
            growingCirclePlayerDamage = 0.2f;
            playerShotSpeed = 3f;
            enemyShotSpeed = 1.5f;
            fireRate = 0.10f;
            playerShip.topSpeed = 3f;
            enemyTotalHealth = 200;
        }
        else if (difficulty == 4)
        {
            enemyAttackInterval = 0.50f;
            growingCircleRate = 0.1f;
            growingCircleReduction = 0.05f;
            growingCircleEnemyDamage = 0.3f;
            growingCirclePlayerDamage = 0.02f;
            playerShotSpeed = 3f;
            enemyShotSpeed = 1.5f;
            fireRate = 0.20f;
            playerShip.topSpeed = 8f;
            enemyTotalHealth = 300;
        }
        else
        {
            return;
        }

        playerShotPool.DespawnAll();
        enemyShotPool.DespawnAll();

        levelCompleteText.gameObject.SetActive(false);
        enemyDamageFromShot = 10;
        enemyCurrentHealth = enemyTotalHealth;
        lastEnemyAttack = Time.time + 4;
        growingCircleScale = 0.02f;
        SetEnemyHealthBarSize();

        levelActive = true;
    }

    void SetPlayerShipPosition()
    {
        if (!playerInput || !playerShip || !levelActive) { return; }

        shipAngle = Vector2.SignedAngle(playerInput.desiredShipPosition - (Vector2)playerShip.selfTransform.position, playerInput.pathRingTransform.position - playerShip.selfTransform.position);

        if (mousePointerTransform && mousePointerTransform.gameObject.activeSelf) { mousePointerTransform.position = playerInput.desiredShipPosition; }

        shipMovingCounterClockwise = shipAngle < 0 ? false : true;

        shipMoveAngle = Vector2.SignedAngle((Vector2)playerInput.pathRingTransform.position - Vector2.right, (Vector2)playerShip.selfTransform.position - (Vector2)playerInput.pathRingTransform.position);
        shipMoveAngle += 180f;

        shipMoveAngle += shipMovingCounterClockwise ? playerShip.topSpeed : -playerShip.topSpeed;

        shipMoveAngle = shipMoveAngle > 360f ? shipMoveAngle - 360f : shipMoveAngle < 0f ? shipMoveAngle + 360f : shipMoveAngle;

        newShipPosition.x = Mathf.Cos(shipMoveAngle * Mathf.Deg2Rad);
        newShipPosition.y = Mathf.Sin(shipMoveAngle * Mathf.Deg2Rad);
        newShipPosition *= shipDistance;
        newShipPosition += (Vector2)playerInput.pathRingTransform.position;

        newShipPosition = ((Vector2)playerShip.selfTransform.position - playerInput.desiredShipPosition).sqrMagnitude < ((Vector2)playerShip.selfTransform.position - newShipPosition).sqrMagnitude ? playerInput.desiredShipPosition : newShipPosition;

        playerShip.selfTransform.SetPositionAndRotation(newShipPosition, Quaternion.FromToRotation(Vector3.up, ((newShipPosition - (Vector2)playerInput.pathRingTransform.position).normalized)));

    }

    public Vector2 PositionOnCircle(Vector2 closestWorldPosition, Vector2 centerOfCircle, float radius) // Returns a Vector2 point on a circle based on the closest point to that circle, which itself is based on the center point and radius values. 
    {
        return centerOfCircle + ((closestWorldPosition - centerOfCircle).normalized * radius);
    }

    public void FirePlayerShot()
    {
        if (levelActive && playerShotPool != null && Time.time - lastFireTime >= fireRate)
        {
            lastFireTime = Time.time;
            Transform pstrans = playerShotPool.Spawn();
            PlayerShot ps = pstrans.GetComponent<PlayerShot>();
            ps.PlayerShotStart(this);
            pstrans.position = PositionOnCircle(playerShip.selfTransform.position, playerInput.pathRingTransform.position, shipDistance - ps.spawnShipGap);
            ps.moving = true;
            ps.movementSpeed = playerShotSpeed;
            ps.movementDirection = ((Vector2)(playerInput.pathRingTransform.position - pstrans.position)).normalized;
        }
    }

    public void DamagePlayer()
    {
        ChangeGrowingCircleScale(growingCirclePlayerDamage);
    }

    void CheckEnemyAttack()
    {
        if (levelActive && Time.time >= lastEnemyAttack + enemyAttackInterval)
        {
            float enemyFireAngle = Random.Range(0, 360);
            if (Mathf.Abs(enemyFireAngle - lastEnemyShotAngle) <= 45f) { enemyFireAngle -= 180f; }
            enemyFireAngle = enemyFireAngle > 360f ? enemyFireAngle - 360f : enemyFireAngle < 0f ? enemyFireAngle + 360f : enemyFireAngle;

            Vector2 esshotangle = Vector2.zero;
            esshotangle.x = Mathf.Cos(enemyFireAngle * Mathf.Deg2Rad);
            esshotangle.y = Mathf.Sin(enemyFireAngle * Mathf.Deg2Rad);

            enemyShip.selfTransform.localRotation = Quaternion.FromToRotation(Vector3.down, esshotangle);

            lastEnemyAttack = Time.time;
            lastEnemyShotAngle = enemyFireAngle;

            if (enemyShotPool != null)
            {
                Transform estrans = enemyShotPool.Spawn();
                EnemyShot es = estrans.GetComponent<EnemyShot>();
                es.EnemyShotStart(this);
                estrans.position = enemyShip.selfTransform.position;
                es.moving = true;
                es.movementSpeed = enemyShotSpeed;
                es.directedTowardsPlayer = true;

                es.movementDirection = esshotangle;
                es.selfTransform.localRotation = Quaternion.FromToRotation(Vector3.up, esshotangle);
            }
        }
    }

    public void ShotCollidedWithGrowingCircle()
    {
        ChangeGrowingCircleScale(-growingCircleReduction);
    }

    public void EnemyShotHitEnemy()
    {
        enemyCurrentHealth = Mathf.Max(0, enemyCurrentHealth - enemyDamageFromShot);
        SetEnemyHealthBarSize();

        if (enemyCurrentHealth <= 0)
        {
            LevelComplete(true);
        }
    }

    public void EnemyShotPassedPlayer()
    {
        ChangeGrowingCircleScale(growingCircleEnemyDamage);
    }

    void SetEnemyHealthBarSize()
    {
        if (enemyHealthBar)
        {
            enemyHealthBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ((enemyCurrentHealth * 1f) / enemyTotalHealth) * enemyHealthDefaultSize);
        }
    }

    void ChangeGrowingCircleScale(float changeAmount)
    {
        if (levelActive == false) { return; }

        growingCircleScale += changeAmount;
        growingCircleScale = Mathf.Clamp(growingCircleScale, 0.02f, 1f);

        if (Mathf.Approximately(growingCircleScale, 1f)) { LevelComplete(false); }
    }

    void SetGrowingCircleScale()
    {
        growingCircleTransform.localScale = Vector2.one * growingCircleScale;
    }

    void LevelComplete(bool victory)
    {
        levelActive = false;
        levelCompleteText.gameObject.SetActive(true);
        levelCompleteText.text = victory ? "YOU" + System.Environment.NewLine + "WIN!" : "YOU" + System.Environment.NewLine + "LOSE!";
        playerShotPool.DespawnAll();
        enemyShotPool.DespawnAll();
    }

    public void TogglePointerVisibility()
    {
        if (mousePointerTransform) { mousePointerTransform.gameObject.SetActive(!mousePointerTransform.gameObject.activeSelf); }
    }
}

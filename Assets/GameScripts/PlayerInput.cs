using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    GameManager gm;
    public Vector2 desiredShipPosition;
    public Transform pathRingTransform;
    public Vector2 normalizedDirection;

    // Use this for initialization
    void Start()
    {

        if (!gm && GetComponent<GameManager>()) { gm = GetComponent<GameManager>(); }
    }

    // Update is called once per frame
    void Update()
    {

        if (!gm || !pathRingTransform) { return; }

        normalizedDirection = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - pathRingTransform.position)).normalized;
        desiredShipPosition = ((Vector2)pathRingTransform.position) + (normalizedDirection * gm.shipDistance);


        //if (Input.GetMouseButtonDown(0))
        if (Input.GetMouseButton(0))
        {
            gm.FirePlayerShot();
        }

    }
}

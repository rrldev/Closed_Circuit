using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GOPOOL2DespawnChecker : MonoBehaviour
{

    public static bool GOPOOL2CheckerActive;
    public static GOPOOL2DespawnChecker checkerComponent;
    public bool checkLate;
    public static List<GOPOOL2> GOPOOL_List;

    void Awake()
    {
        GOPOOL2CheckerActive = true;
        checkerComponent = this;
        GOPOOL_List = new List<GOPOOL2>();
    }

    void LateUpdate()
    {
        if (checkLate) { CheckForDespawnedInstances(); }
    }
    void Update()
    {
        if (!checkLate) { CheckForDespawnedInstances(); }
    }

    void CheckForDespawnedInstances()
    {
        for (int i = 0; i < GOPOOL_List.Count; i++)
        {
            GOPOOL_List[i].MoveAllInactiveToUnusedPool();
        }
    }

}

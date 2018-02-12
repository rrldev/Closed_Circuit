using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GOPOOL2 
{
    public string poolName; // name of pool... for whatever reason
    public bool disableWhenDespawned; // if set to false, objects won't disable themselves when despawned. normally, true.
    public bool autoMoveToUnusedIfDespawned; // if set to true, pool will check all active instances before spawning a new one. If any are inactive in the scene, they will be moved to the unused pool.
    public Transform spawnObject; // reference to the object to be pooled (so it can be instantiated)
    public List<Transform> unusedGO; // game objects ready to be used (INACTIVE)
    public List<Transform> activeGO; // game objects that are being used actively in the scene
    public List<Transform> pool; // list of all objects. mostly just used for iteration through all objects easily.
                                 /// <summary>
                                 /// The parent transform that spawned objects become child objects of. Use [ChangeParent] function to change this value after pool has been created. DO NOT set directly.
                                 /// </summary>
    public Transform parent; // transform that all spawned objects are parented to when created.
    private Vector3 localPosCache; // Used when maintaining local position of active objects when changing their parent transform. (ChangeParent function below)
    private int originalPoolSize; // original number of objects spawned when pool was first created. Used for returning count to default value if something increases the count briefly.



    public Transform Spawn(bool treatPositionAsLocal = false)
    {
        return Spawn(Vector3.zero, treatPositionAsLocal);
    }
    public Transform Spawn(Vector3 spawnPosition, bool treatPositionAsLocal = false) // returns new gameobject for use. adds unused GO to active pool, or instantiates new one and adds it if no usused are available.
    {
        if (autoMoveToUnusedIfDespawned) { MoveAllInactiveToUnusedPool(); }

        Transform nnnn = null;
        if (unusedGO.Count > 0)
        {
            nnnn = unusedGO[0];
            unusedGO.RemoveAt(0);
        }
        else
        {
            nnnn = (Transform)Transform.Instantiate(spawnObject) as Transform;
            if (parent != null) { nnnn.SetParent(parent, false); }
            pool.Add(nnnn);
        }
        activeGO.Add(nnnn);
        nnnn.gameObject.SetActive(true);
        if (nnnn.parent != null && treatPositionAsLocal)
            nnnn.localPosition = spawnPosition;
        else if (nnnn.parent == null && treatPositionAsLocal)
        {
            Debug.LogWarning(nnnn.ToString() + "attempted to spawn at local position, but no parent object has been set! Treating position as world position instead.");
            nnnn.position = spawnPosition;
        }
        else
            nnnn.position = spawnPosition;
        return nnnn;
    }

    public void AddNewGameObjectsToUnusedPool(int amount) // Preloading the pool, basically.
    {
        for (int i = 0; i < amount; i++)
        {
            Transform ngongo = (Transform)Transform.Instantiate(spawnObject) as Transform;
            //if (parent != null) { ngongo.parent = parent; } OBSOLETE... OR SOMETHING. RETURNS WARNING IN UNITY.
            if (parent != null) { ngongo.SetParent(parent, false); }
            pool.Add(ngongo);
            unusedGO.Add(ngongo);
            ngongo.gameObject.SetActive(false);
        }
    }

    public void Despawn(Transform objectToDespawn)
    {
        if (activeGO.Contains(objectToDespawn))
        {
            unusedGO.Add(objectToDespawn);
            activeGO.Remove(objectToDespawn);
            if (disableWhenDespawned) { objectToDespawn.gameObject.SetActive(false); }
        }
    }

    public void DespawnAll()
    {
        int timeoutNum = 999999999; // cant have more than this many objects in pool... I guess. Shouldn't be a problem.
        while (timeoutNum > 0 && activeGO.Count > 0)
        {
            timeoutNum--;
            Despawn(activeGO[0]);
        }
    }

    /// <summary>
    /// This will move any instance in the active pool that became deactivated from another script into the unused pool. Used to clean up cases where instances despawn themselves and such.
    /// </summary>
    public void MoveAllInactiveToUnusedPool()
    {
        for (int i = activeGO.Count - 1; i >= 0; i--)
        {
            if (activeGO[i].gameObject.activeSelf == false)
            {
                unusedGO.Add(activeGO[i]);
                activeGO.Remove(activeGO[i]);
            }
        }
    }

    /// <summary>
    /// Unloads a specific instance in the pool. This will destroy the game object entirely and remove it from the pool. Use Despawn for normal pool use.
    /// </summary>
    public void UnloadInstance(Transform instanceToUnload)
    {
        if (pool.Contains(instanceToUnload))
        {
            GameObject.Destroy(instanceToUnload.gameObject);
            if (pool.Contains(instanceToUnload)) { pool.Remove(instanceToUnload); }
            if (activeGO.Contains(instanceToUnload)) { activeGO.Remove(instanceToUnload); }
            if (unusedGO.Contains(instanceToUnload)) { unusedGO.Remove(instanceToUnload); }
        }
        else
        {
            Debug.LogWarning("ATTEMPTED TO REMOVE TRANSFORM FROM " + poolName + " POOL, BUT TRANSFORM IS NOT AN INSTANCE OF THIS POOL! NO ACTION WAS PERFORMED!");
        }
    }

    /// <summary>
    /// Unloads all unused instances, as in anything in the UnusedGO list. (destroys game objects)
    /// </summary>
    public void UnloadUnusedInstances(bool despawnEntirePoolFirst = false, int keepAtLeast = 0)
    {
        if (despawnEntirePoolFirst) { DespawnAll(); }
        for (int i = unusedGO.Count - 1; i >= Mathf.Max(0, keepAtLeast); i--)
        {
            UnloadInstance(unusedGO[i]);
        }
    }

    /// <summary>
    /// Unloads extra instances that may have spawned. Useful if some rare event spawns a lot of extra instances. Set param to true to ignore active state and despawn anyway. Unused instances are always despawned first.
    /// </summary>
    public void UnloadAndReturnToOriginalSpawnAmount(bool despawnActiveIfNecessary = false)
    {
        while (pool.Count > originalPoolSize)
        {
            if (unusedGO.Count > 0) { UnloadInstance(unusedGO[0]); }
            else if (despawnActiveIfNecessary && activeGO.Count > 0) { UnloadInstance(activeGO[0]); }
            else { break; }
        }
    }

    /// <summary>
    /// Unloads instances in the pool (destroys game objects). Can set a value to keep pool size above. Set bool to true to despawn active instances if necessary to reach limit. Unused instances are always despawned first.
    /// </summary>
    public void UnloadUntilLimit(int limit, bool despawnActiveIfNecessary = false)
    {
        limit = System.Math.Max(0, limit);
        while (pool.Count > limit)
        {
            if (unusedGO.Count > 0) { UnloadInstance(unusedGO[0]); }
            else if (despawnActiveIfNecessary && activeGO.Count > 0) { UnloadInstance(activeGO[0]); }
            else { break; }
        }
    }

    void RemoveAllNullReferences()
    {
        for (int i = unusedGO.Count - 1; i >= 0; i--) { if (unusedGO[i] == null) { unusedGO.RemoveAt(i); } }
        for (int i = activeGO.Count - 1; i >= 0; i--) { if (activeGO[i] == null) { activeGO.RemoveAt(i); } }
        for (int i = pool.Count - 1; i >= 0; i--) { if (pool[i] == null) { pool.RemoveAt(i); } }
    }

    /// <summary>
    /// Changes the parent transform.
    /// </summary>
    /// <param name="newParentTransform">New parent transform.</param>
    /// <param name="changeAllActiveObjectsImmediately">If set to <c>true</c> changes the parent transform of all currently active objects. Defaults to true.</param>
    /// <param name="maintainLocalPosition">If 'changeAllActiveObjectsImmediately' and this are set to <c>true</c> all active objects will keep their current local positions when switching to the new parent, which will likely change their world position. Defaults to false.</param>
    public void ChangeParent(Transform newParentTransform, bool changeAllActiveObjectsImmediately = true, bool maintainLocalPosition = false)
    {
        parent = newParentTransform;

        if (changeAllActiveObjectsImmediately)
        {
            for (int i = 0; i < activeGO.Count; i++)
            {
                if (maintainLocalPosition == false)
                {
                    activeGO[i].parent = newParentTransform;
                }
                else
                {
                    localPosCache = activeGO[i].localPosition;
                    activeGO[i].parent = newParentTransform;
                    activeGO[i].localPosition = localPosCache;
                }
            }
        }
    }



    // CONSTRUCTOR
    public GOPOOL2(Transform objectToBePooled, string nameOfPool, int preloadAmount = 0, bool disableOnDespawn = true, Transform parentTransform = null)
    {
        poolName = nameOfPool;
        disableWhenDespawned = disableOnDespawn;
        spawnObject = objectToBePooled;
        unusedGO = new List<Transform>();
        activeGO = new List<Transform>();
        pool = new List<Transform>();
        parent = parentTransform;
        localPosCache = Vector3.zero;
        originalPoolSize = preloadAmount;

        AddNewGameObjectsToUnusedPool(preloadAmount);

        if (GOPOOL2DespawnChecker.GOPOOL2CheckerActive) { GOPOOL2DespawnChecker.GOPOOL_List.Add(this); }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{


}
public abstract class Entity<T> : EntityBase where T : Entity<T>
{
    public EntityStateManager<T> states {  get; private set; }
    protected virtual void InitializeStateManager()
    {
        states = GetComponent<EntityStateManager<T>>();
    }

    protected virtual void HandleStates()
    {
        states.Step();
    }

    protected virtual void Awake()
    {
        InitializeStateManager();
    }

    protected virtual void Update()
    {
        HandleStates();
    }

}

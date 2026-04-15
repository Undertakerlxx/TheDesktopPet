using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    public Camera cam;
    public Collider2D entityCollider;

}
public abstract class Entity<T> : EntityBase where T : Entity<T>
{
    public EntityStateManager<T> states {  get; private set; }
    protected virtual void InitializeStateManager()
    {
        states = GetComponent<EntityStateManager<T>>();
    }

    protected virtual void InitializeCamera()
    {
        cam = Camera.main;
    }

    protected virtual void InitializeCollider()
    {
        entityCollider = GetComponent<Collider2D>();
    }

    protected virtual void HandleStates()
    {
        states.Step();
    }

    protected virtual void Awake()
    {
        InitializeStateManager();
        InitializeCamera();
        InitializeCollider();
    }

    protected virtual void Update()
    {
        HandleStates();
    }

}

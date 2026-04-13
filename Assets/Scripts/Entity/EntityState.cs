using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityState<T> where T : Entity<T> 
{ 
    public float timeSinceEntered {  get; private set; }
    public void Enter(T entity)
    {
        timeSinceEntered = 0;
        OnEnter(entity);
    }

    public void Exit(T entity)
    {
        OnExit(entity);
    }

    public void Step(T entity)
    {
        OnStep(entity);
        timeSinceEntered += Time.deltaTime;
    }

    protected abstract void OnEnter(T entity);
    protected abstract void OnExit(T entity);
    protected abstract void OnStep(T entity); 
    

    //∑¥…‰
    public static EntityState<T> CreateFromString(string typeName)
    {
        return (EntityState<T>)System.Activator.CreateInstance(System.Type.GetType(typeName));
    }

    public static List<EntityState<T>> CreateStringFromArray(string[] array)
    {
        var list = new List<EntityState<T>>();
        foreach (var typeName in array)
        {
            list.Add(CreateFromString(typeName));
        }
        return list;
    }
}



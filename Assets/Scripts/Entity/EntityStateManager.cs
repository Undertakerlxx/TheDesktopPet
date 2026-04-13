using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStateManager : MonoBehaviour
{

}
public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{
    protected List<EntityState<T>> m_list = new List<EntityState<T>>();
    protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

    public EntityState<T> current;//当前状态
    public EntityState<T> last;//上一个状态

    //查看当前状态在表中的索引
    public int current_index()
    {
        return m_list.IndexOf(current);
    }

    public int last_index()
    {
        return m_list.IndexOf(last);
    }


    public T entity {  get; private set; }

    protected abstract List<EntityState<T>> GetStateList();//获取列表的抽象类

    protected virtual void InitializeEntity()
    {
        entity = GetComponent<T>();
    }
    protected virtual void InitializeStates()
    {

    }

    protected virtual void Start()
    {
        InitializeEntity();
        InitializeStates();
    }

    public virtual void Change(int to)//按照状态列表排序切换
    {

    }

    public virtual void Step()
    {

    }

    public virtual void Change<TState>() where TState : EntityState<T>//按照状态名称切换
    {

    }

    public virtual void Change(EntityState<T> to)//具体切换逻辑
    {

    }

}

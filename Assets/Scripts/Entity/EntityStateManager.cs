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
        m_list = GetStateList();

        foreach(var state in m_list)
        {
            var type = state.GetType();
            if (!m_states.ContainsKey(type))
            {
                m_states.Add(type, state);
            }
        }

        if(m_list.Count > 0)
        {
            current = m_list[0];
        }
    }

    protected virtual void Start()
    {
        InitializeEntity();
        InitializeStates();
    }

    public virtual void Change(int to)//按照状态列表排序切换
    {
        if(to >= 0 && to < m_list.Count)
        {
            Change(m_list[to]);
        }
    }

    public virtual void Step()
    {
        if (current != null && Time.timeScale > 0)
        {
            current.Step(entity);
        }
    }

    public virtual void Change<TState>() where TState : EntityState<T>//按照状态名称切换
    {
        var type = typeof(TState);
        if (m_states.ContainsKey(type))
        {
            Change(m_states[type]);
        }
    }

    public virtual void Change(EntityState<T> to)//具体切换逻辑
    {
        if(to != null && Time.timeScale > 0)
        {
            if(current != null)
            {
                current.Exit(entity);
                last = current;
            }

            current = to;
            current.Enter(entity);
        }
    }

}

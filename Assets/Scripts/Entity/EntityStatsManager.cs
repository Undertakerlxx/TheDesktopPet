using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityStatsManager<T> : MonoBehaviour where T : EntityStats<T>
{
    public T[] stats;
    public T current_stats {  get; protected set; }
    
    public virtual void Change(int to)
    {
        if(to >= 0  && to < stats.Length)
        {
            if(current_stats != stats[to])
            {
                current_stats = stats[to];
            }
        }
    }

    protected virtual void Start()
    {
        if(stats.Length > 0)
        {
            current_stats = stats[0];
        }


    }
}

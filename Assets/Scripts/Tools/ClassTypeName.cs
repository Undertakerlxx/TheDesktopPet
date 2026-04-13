using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自定义属性，用于在 Inspector 中显示某个类型及其子类的类名选择器
/// 继承自 PropertyAttribute，可以附加到字段上
/// </summary>
public class ClassTypeName : PropertyAttribute
{
    /// <summary>
    /// 存储允许选择的基类类型
    /// 只有继承自这个类型的类才会在选择器中显示
    /// </summary>
    public Type type;

    /// <summary>
    /// 构造函数，指定允许的基类类型
    /// </summary>
    /// <param name="type">基类 Type 对象</param>
    public ClassTypeName(Type type)
    {
        this.type = type; // 保存传入的类型，供 PropertyDrawer 使用
    }
}

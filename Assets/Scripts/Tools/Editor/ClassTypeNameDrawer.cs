using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;



/// <summary>
/// 自定义属性绘制器（PropertyDrawer）
/// 用于在 Inspector 中显示 ClassTypeName 类型的下拉列表
/// 下拉列表内容为指定基类的所有子类
/// </summary>
[CustomPropertyDrawer(typeof(ClassTypeName))]
public class ClassTypeNameDrawer : PropertyDrawer
{
    // 当前属性对应的 ClassTypeName 特性实例
    protected ClassTypeName m_classTypeName;

    // 子类的完整命名空间名称列表（如 "Namespace.MyClass"）
    protected List<string> m_names;

    // 格式化后的子类名称列表（只显示类名，并在驼峰字母之间加空格）
    protected List<string> m_formatedNames;

    // 是否已经初始化标记，避免重复初始化
    protected bool m_initialized = false;

    /// <summary>
    /// 初始化子类名称列表
    /// </summary>
    protected virtual void Initialize()
    {
        // 将 attribute 转换为 ClassTypeName 类型
        m_classTypeName = (ClassTypeName)attribute;

        // 获取当前 AppDomain 中所有程序集的所有类型，并筛选出指定基类的子类
        var classes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(m_classTypeName.type));

        // 获取完整类型名称列表
        m_names = classes
            .Select(type => type.ToString()) // "Namespace.ClassName"
            .ToList();

        // 获取格式化名称列表（只保留类名，并将驼峰分隔成空格）
        m_formatedNames = classes
            .Select(type => type.Name) // 只取类名
            .Select(name => Regex.Replace(name, "(\\B[A-Z])", " $1")) // 驼峰转空格
            .ToList();
    }

    /// <summary>
    /// 初始化属性值
    /// 如果属性为空字符串，则默认选择列表中的第一个子类
    /// </summary>
    /// <param name="property">SerializedProperty 对象</param>
    protected virtual void InitializeProperty(SerializedProperty property)
    {
        if (property.stringValue.Length == 0)
        {
            property.stringValue = m_names[0];
        }
    }

    /// <summary>
    /// 绘制 Inspector 下拉列表 GUI
    /// </summary>
    /// <param name="position">绘制区域</param>
    /// <param name="property">当前属性</param>
    /// <param name="label">显示标签</param>
    protected virtual void HandleGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 如果当前属性值不在列表中，直接返回
        if (!m_names.Contains(property.stringValue)) return;

        // 获取当前属性值在列表中的索引
        var current = m_names.IndexOf(property.stringValue);

        // 绘制前缀标签
        position = EditorGUI.PrefixLabel(position, label);

        // 绘制下拉列表，返回选择的索引
        var selected = EditorGUI.Popup(position, current, m_formatedNames.ToArray());

        // 更新属性值为选择的完整类型名称
        property.stringValue = m_names[selected];
    }

    /// <summary>
    /// Unity 内置 OnGUI 方法
    /// </summary>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 第一次绘制时初始化
        if (!m_initialized)
        {
            m_initialized = true;
            Initialize();
        }

        // 确保有子类可供选择
        if (m_names.Count > 0)
        {
            // 初始化属性值
            InitializeProperty(property);

            // 绘制下拉列表 GUI
            HandleGUI(position, property, label);
        }
    }
}

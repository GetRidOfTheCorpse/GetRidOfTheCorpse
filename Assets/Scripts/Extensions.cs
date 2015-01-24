using UnityEngine;
using System.Collections;
using Enum = System.Enum;
using Type = System.Type;
using Attribute = System.Attribute;
using System.Reflection;
using System.ComponentModel;

public static class Extensions
{
    public static string GetDescription(this Enum value)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            FieldInfo field = type.GetField(name);
            if (field != null)
            {
                DescriptionAttribute attr = 
                    Attribute.GetCustomAttribute(field, 
                        typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return null;
    }
}   
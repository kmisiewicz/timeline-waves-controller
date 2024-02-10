using System;
using UnityEngine;

namespace Chroma.Utility
{
    // https://stackoverflow.com/questions/55583071/see-enumerated-indices-of-array-in-unity-inspector
    // https://stackoverflow.com/questions/24892935/custom-property-drawers-for-generic-classes-c-sharp-unity

    [Serializable]
    public abstract class EnumNamedArray { }

    [Serializable]
    public class EnumNamedArray<T> : EnumNamedArray
    {
        public string[] Names;
        public T[] Values;

        Type _enumType;

        public Type EnumType => _enumType;

        public EnumNamedArray(Type enumType)
        {
            this._enumType = enumType;
            this.Names = Enum.GetNames(enumType);
            this.Values = new T[Names.Length];
        }

        public EnumNamedArray(EnumNamedArray<T> sourceArray)
        {
            this._enumType = sourceArray.EnumType;
            this.Names = Enum.GetNames(this._enumType);
            this.Values = new T[Names.Length];
            for (int i = 0; i < sourceArray.Names.Length; i++)
            {
                int idx = Array.IndexOf(this.Names, sourceArray.Names[i]);
                if (idx > -1)
                    this.Values[idx] = sourceArray.Values[i];
            }
        }

        public T this[Enum enumValue]
        {
            get
            {
                Type type = enumValue.GetType();
                if (!type.IsEnum && type != _enumType)
                    throw new ArgumentException("EnumValue must be of correct Enum type", "enumValue");
                string name = Enum.GetName(type, enumValue);
                int idx = Array.IndexOf(Names, name);
                if (idx == -1)
                {
                    Debug.LogError($"Can't get value. Enum {type.FullName} was changed.");
                    return default(T);
                }
                return Values[idx];
            }
            set
            {
                Type type = enumValue.GetType();
                if (!type.IsEnum && type != _enumType)
                    throw new ArgumentException("EnumValue must be of correct Enum type", "enumValue");
                string name = Enum.GetName(type, enumValue);
                int idx = Array.IndexOf(Names, name);
                if (idx == -1)
                    Debug.LogError($"Can't set value. Enum {type.FullName} was changed.");
                Values[idx] = value;
            }
        }
    }
}

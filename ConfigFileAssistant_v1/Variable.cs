using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigFileAssistant_v1
{
    public class VariableInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public Type Type { get; set; }
        public string TypeName { get; set; }
        public object Value { get; set; }
        public object DefaultValue { get; set; }
        public Result Result { get; set; }
        public List<VariableInfo> Children { get; set; }
        public List<VariableInfo> EnumValues { get; set; }

        public VariableInfo(string path, string name, string typeName, Object value)
        {
            Name = name;
            FullName = path == string.Empty ? name : $"{path}.{Name}";
            TypeName = typeName;
            Value = value;
            DefaultValue = string.Empty;
            Result = Result.Pass;
            Children = new List<VariableInfo>();
        }
        public VariableInfo(string name, Type type, object value)
        {
            FullName = name;
            Name = FullName.Split('.').Last();
            Type = type;
            TypeName = type.Name;
            Children = new List<VariableInfo>();
            Value = value;
        }
        public VariableInfo(string name, string typeName, object value)
        {
            FullName = name;
            Name = FullName.Split('.').Last();
            Type = typeof(string);
            TypeName = typeName;
            Children = new List<VariableInfo>();
            Value = value;
        }

        public void SetEnumValues(List<VariableInfo> variables)
        {
            if (EnumValues == null)
            {

                EnumValues = new List<VariableInfo>();
            }
            EnumValues = variables;
            DefaultValue = variables[0].Value;
        }
        public bool HasChildren()
        {
            return Children.Count > 0;
        }
        public bool IsEnumType()
        {
            return EnumValues != null && EnumValues.Count > 0;
        }
    }
}

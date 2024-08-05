using CalibrationTool;
using CoPick.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.RepresentationModel;
using ConfigTypeFinder;

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
       
        public VariableInfo(string path, string name,string typeName, Object value)
        {
            Name = name;
            FullName = path == string.Empty ? name : $"{path}.{Name}";
            TypeName = typeName;
            Value = value;
            DefaultValue = string.Empty;
            Result = Result.Ok;
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
            if(EnumValues == null)
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
            return EnumValues != null && EnumValues.Count>0;
        }
    }

    public enum Result
    {
        Ok,
        OnlyInCs,
        OnlyInYml,
        WrongValue,
        NoChild
    }

    public class ConfigValidator
    {
        private static Object Instance;
        private static YamlMappingNode Root;
        private static YamlStream Yaml;
        private static List<VariableInfo> CsVariables;
        private static List<VariableInfo> YmlVariables;
        private static Dictionary<String, VariableInfo> ErrorVariables;

        public static void Init()
        {
            TypeManager.init();
            Instance = Activator.CreateInstance(typeof(Config));
            CsVariables = new List<VariableInfo>();
            Yaml = new YamlStream();
        }
        public static void ClearAllData()
        {
            ErrorVariables = null;
            CsVariables = null;
            YmlVariables = null;
            Init();
        }
        public static void LoadYamlFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                Yaml.Load(reader);
            }
            if (Yaml.Documents.Count != 0)
            {
                Root = (YamlMappingNode)Yaml.Documents[0].RootNode;
            }
            else
            {
                Root = new YamlMappingNode();
            }
        }

        public static List<VariableInfo> ExtractYmlVariables()
        {
            YmlVariables = new List<VariableInfo>();
            ExtractYmlVariablesRecursive(Root, YmlVariables, "");
            return YmlVariables;
        }

        private static void ExtractYmlVariablesRecursive(YamlNode node, List<VariableInfo> variables, string prefix)
        {
            if (node is YamlMappingNode mappingNode)
            {
                foreach (var entry in mappingNode.Children)
                {
                    var key = ((YamlScalarNode)entry.Key).Value;
                    var fullName = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

                    if (entry.Value is YamlMappingNode)
                    {
                        var childInfo = new VariableInfo(fullName, typeof(Dictionary<,>), string.Empty);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, fullName);
                        variables.Add(childInfo);
                    }
                    else if (entry.Value is YamlSequenceNode)
                    {
                        var childInfo = new VariableInfo(fullName, typeof(List<>), string.Empty);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, fullName);
                        variables.Add(childInfo);
                    }
                    else
                    {
                        var type = typeof(string);
                        variables.Add(new VariableInfo(fullName, type, (entry.Value)));
                    }
                }
            }
            else if (node is YamlSequenceNode sequenceNode)
            {
                int index = 0;
                foreach (var childNode in sequenceNode.Children)
                {
                    var childPrefix = $"{prefix}.[{index}]";

                    if (childNode is YamlMappingNode)
                    {
                        var childInfo = new VariableInfo(childPrefix, typeof(Dictionary<,>), string.Empty);
                        ExtractYmlVariablesRecursive(childNode, childInfo.Children, childPrefix);
                        variables.Add(childInfo);
                    }
                    else if (childNode is YamlSequenceNode)
                    {
                        var childInfo = new VariableInfo(childPrefix, typeof(List<>), string.Empty);
                        ExtractYmlVariablesRecursive(childNode, childInfo.Children, childPrefix);
                        variables.Add(childInfo);
                    }
                    else if (childNode is YamlScalarNode scalarNode)
                    {
                        variables.Add(new VariableInfo(childPrefix, typeof(string), scalarNode.Value));
                    }
                    index++;
                }
            }
            else if (node is YamlScalarNode scalarNode)
            {
                variables.Add(new VariableInfo(prefix, typeof(string), scalarNode.Value));
            }
        }

        public static List<VariableInfo> ExtractCsVariables()
        {
            var type = typeof(Config);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object defaultValue = GetDefaultValue(property, Instance);
                ExtractCsVariablesRecursive(property.Name, property.PropertyType, defaultValue, CsVariables);
            }
            return CsVariables;
        }
        private static void ExtractCsVariablesRecursive(string propertyName, Type propertyType, Object value, List<VariableInfo> variables)
        {
            VariableInfo variableInfo;
            if (!propertyType.IsGenericType)
            {
                variableInfo = new VariableInfo(propertyName, propertyType, value);
                if (propertyType.IsEnum)
                {
                    variableInfo.SetEnumValues(ExtractEnumValues(variableInfo,propertyType));
                    if (propertyName.Split('.').Last() != "Key")
                    {
                        TypeManager.AddType(variableInfo.TypeName, variableInfo.Type);
                    }
                }
                variables.Add(variableInfo);
                return;
            }
            Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();
            Type[] genericArgs = propertyType.GetGenericArguments();
            variableInfo = new VariableInfo(propertyName, propertyType, string.Empty);
            if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                ExtractCsVariablesRecursive($"{propertyName}.Key", genericArgs[0], "", variableInfo.Children);
                ExtractCsVariablesRecursive($"{propertyName}.Value", genericArgs[1], "", variableInfo.Children);

            }
            else if (genericTypeDefinition == typeof(List<>))
            {
                ExtractCsVariablesRecursive($"{propertyName}.Item", genericArgs[0], "", variableInfo.Children);
            }
            variables.Add(variableInfo);
        }

        private static List<VariableInfo> ExtractEnumValues(VariableInfo variableInfo, Type propertyType)
        {
            List<VariableInfo> enumValues = new List<VariableInfo> ();
            foreach (var item in propertyType.GetEnumValues())
            {
                var enumField = propertyType.GetField(item.ToString());
                var validatorAttributeData = enumField.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType == typeof(ValidatorTypeAttribute));
                
                if (validatorAttributeData == null)
                {
                    Type type = typeof(string);
                    var converterAttribute = enumField.GetCustomAttributes(typeof(TypeConverterAttribute), false).FirstOrDefault() as TypeConverterAttribute;
                    if (converterAttribute != null)
                    {
                        if (converterAttribute.ConverterTypeName.Contains("Boolean"))
                        {
                            type = typeof(bool);
                        }
                    }
                    enumValues.Add(new VariableInfo(enumField.Name, type, ""));
                }
                else
                {
                    var constructorArguments = validatorAttributeData.ConstructorArguments;
                    var validatorType = (ValidatorType)constructorArguments[0].Value;
                    TypeManager.MakeFunction(enumField.Name,validatorType, constructorArguments[1].ToString());
                    enumValues.Add(new VariableInfo(enumField.Name, validatorType.ToString(), ""));
                }
            }
            return enumValues;
        }
        public static object GetDefaultValue(PropertyInfo property, object instance)
        {
            if (instance != null)
            {
                var value = property.GetValue(instance);
                if (value != null)
                {
                    return value;
                }
            }
            return string.Empty;
        }
        public static List<VariableInfo> CompareVariables(List<VariableInfo> csVariables, List<VariableInfo> ymlVariables)
        {
            ErrorVariables = new Dictionary<string, VariableInfo>();
            var ymlDict = ymlVariables.ToDictionary(v => v.Name);

            foreach (var csVariable in csVariables)
            {
                if (ymlDict.TryGetValue(csVariable.Name, out var ymlVariable))
                {
                    SetDefaultType(ymlVariable, csVariable);

                    if (ymlVariable.Type != typeof(string) && csVariable.TypeName != ymlVariable.TypeName)
                    {
                        if (csVariable.Type.IsEnum)
                        {
                            ymlVariable.SetEnumValues(csVariable.EnumValues);
                        }
                        SetResult(ymlVariable,Result.WrongValue);
                    }
                    else
                    {
                        CompareChild(csVariable, ymlVariable, null);
                    }
                }
                else
                {
                    var variable = new VariableInfo(csVariable.Name, csVariable.Type, csVariable.Value);
                    if (csVariable.Type.IsEnum)
                    {
                        variable.SetEnumValues(csVariable.EnumValues);
                    }
                    SetResult(variable, Result.OnlyInCs);
                    ymlVariables.Add(variable);
                }
            }

            foreach (var ymlVariable in ymlVariables)
            {
                if (!csVariables.Any(v => v.Name == ymlVariable.Name))
                {
                    SetResult(ymlVariable, Result.OnlyInYml);
                }
            }
            return ymlVariables;
        }
        private static void CompareChild(VariableInfo csVariable, VariableInfo ymlVariable, VariableInfo csParentVariable)
        {
            Result result = ymlVariable.Result;

            // 둘다 자식이 있는 경우
            if (csVariable.HasChildren() && ymlVariable.HasChildren())
            {
                if (csVariable.TypeName == ymlVariable.TypeName)
                {
                    foreach (var child in ymlVariable.Children)
                    {
                        CompareChild(csVariable.Children.Last(), child, csVariable);
                    }
                }
                else
                {
                    SetDefaultType(ymlVariable, csVariable);
                    result = Result.WrongValue;
                }
            }

            // 둘다 자식이 없는 경우
            else if (!csVariable.HasChildren() && !ymlVariable.HasChildren())
            {
                if(ymlVariable.Result == Result.Ok)
                {
                    if (csParentVariable != null && csParentVariable.Children[0].Type.IsEnum)
                    {
                        result = IsValidatedEnumValue(ymlVariable, csParentVariable.Children[0]);
                    }
                    else
                    {
                        result = CompareSingleValue(ymlVariable, csVariable);

                    }
                }
                
            }
            else if(csVariable.HasChildren() && !ymlVariable.HasChildren())
            {
                result = Result.NoChild;
            }
            else
            {
                result = Result.WrongValue;
            }
            
            SetResult(ymlVariable,result);
        }

        private static void SetResult(VariableInfo variableInfo, Result result)
        {
            variableInfo.Result = result;
            if(result != Result.Ok)
            {
                ErrorVariables.Add(variableInfo.FullName, variableInfo);
            }
        }
        private static Result CompareSingleValue(VariableInfo ymlVariable, VariableInfo csVariable)
        {
            if (csVariable.IsEnumType())
            {
                ymlVariable.SetEnumValues(csVariable.EnumValues);
                ymlVariable.DefaultValue = csVariable.Value;
                var values = csVariable.EnumValues.ToDictionary(v => v.Name);
                if (values.ContainsKey(ymlVariable.Value.ToString()))
                {
                    Type targetType = csVariable.Value.GetType();
                    ymlVariable.Value = Enum.Parse(targetType, ymlVariable.Value.ToString());
                    return Result.Ok;
                }
                return Result.WrongValue;
            }
            else
            {
                if (TypeManager.IsValidateType(ymlVariable, ymlVariable.Value.ToString()) != string.Empty)
                {
                    return Result.WrongValue;
                }
                return Result.Ok;
            }
        }
        private static void SetDefaultType(VariableInfo ymlVariable, VariableInfo csVariable)
        {
            ymlVariable.Type = csVariable.Type;
            ymlVariable.TypeName = csVariable.TypeName; 
            ymlVariable.DefaultValue = csVariable.Value.ToString();
           
        }
        private static Result IsValidatedEnumValue(VariableInfo ymlVariable, VariableInfo csVariable)
        {
            var values = csVariable.EnumValues.ToDictionary(v => v.Name);
            if (values.ContainsKey(ymlVariable.Name))
            {
                ymlVariable.Type = values[ymlVariable.Name].Type;
                ymlVariable.TypeName = values[ymlVariable.Name].TypeName;
                if (TypeManager.IsValidateType(ymlVariable, ymlVariable.Value.ToString()) == string.Empty)
                {
                    return Result.Ok;
                }
                return Result.WrongValue;
            }
            return Result.OnlyInYml;
            
        }
        public static bool HasErrors()
        {
            return ErrorVariables != null && ErrorVariables.Count > 0;
        }
        public static Dictionary<string,VariableInfo> GetErrors()
        {
            return ErrorVariables;
        }

        public static void RemoveVariableFromErrorList(string error)
        {
            ErrorVariables.Remove(error);
        }
        
        public static List<VariableInfo> GetParentVariables(string fullName)
        {
            var names = fullName.Split('.');
            int depth = names.Length;

            VariableInfo parent = null;
            var currentList = YmlVariables;

            for (int i = 0; i < depth-1; i++)
            {
                var currentVar = currentList.Find(v => v.Name == names[i]);
                if (currentVar == null)
                {
                    return null;
                }
                parent = currentVar;
                currentList = currentVar.Children;
            }
            return parent == null ? YmlVariables : parent.Children;
            
        }
        public static bool InsertChildToVariable(VariableInfo variable, VariableInfo child)
        {
            var parentVariables = GetParentVariables(variable.FullName);
            var target = parentVariables.Find(v => v.Name == variable.Name);
            if(target == null)
            {
                return false;
            }

            Dictionary<string, VariableInfo> parentVariableDict = target.Children.ToDictionary(v => v.Name);
            if (target.Type == typeof(List<>))
            {
                child.Name = $"[{target.Children.Count}]";
            }
            if(parentVariableDict.ContainsKey(child.Name))
            {
                return false;
            }
            child.FullName = $"{variable.FullName}.{child.Name}";
            target.Children.Add(child); 
            return true;
        }
        public static bool InsertVariable(VariableInfo variable, VariableInfo newVariable)
        {
            if (variable == null)
            {
                Dictionary<string, VariableInfo> csVariablesDict = CsVariables.ToDictionary(v => v.Name);
                if(csVariablesDict.ContainsKey(newVariable.Name))
                {
                    return false;
                }

                newVariable.Result = Result.OnlyInYml;
                YmlVariables.Insert(0, newVariable);
                return true;
            }

            var parentVariables = GetParentVariables(variable.FullName);
            int selectedIndex = parentVariables.IndexOf(variable);
            if (selectedIndex < 0)
            {
                return false;
            }
            else
            {
                Dictionary<string, VariableInfo> parentVariableDict = parentVariables.ToDictionary(v => v.Name);
                if(parentVariableDict.ContainsKey(newVariable.Name))
                {
                    return false;
                }
                parentVariables.Insert(selectedIndex + 1, newVariable);
                return true;
            }
        }
        public static bool AddVariable(VariableInfo variableInfo)
        {
            var parentVariables = GetParentVariables(variableInfo.FullName);
            var target = parentVariables.Find(v => v.Name == variableInfo.Name);
            if (target != null)
            {
                target.Result = Result.Ok;
                return true;
            }
            return false;
        }
        public static bool RemoveVariable(VariableInfo variableInfo)
        {
            var parentVariables = GetParentVariables(variableInfo.FullName);
            
            var target = parentVariables.Find(v => v.Name == variableInfo.Name);
            if(target != null)
            {
                if (target.HasChildren())
                {
                    target.Children.Clear();
                }
                parentVariables.Remove(target);
                return true;
            }
            return false;
        }

        public static bool ModifyChildFromVariable(VariableInfo variableInfo)
        {
            var parentVariables = GetParentVariables(variableInfo.FullName);

            var target = parentVariables.Find(v => v.Name == variableInfo.Name);
            if(target != null)
            {
                if (target.HasChildren())
                {
                    target.Children.Clear();
                }
                target.Value = target.DefaultValue;
                target.Result = Result.Ok;
                return true;
            }
            return false;
        }
        public static string UpdateChild(string fullName, object value)
        {
            var parentVariables = GetParentVariables(fullName);
            var names = fullName.Split('.');
            var target = parentVariables.Find(v => v.Name == names.Last());
            var resultMessage = string.Empty;
            if (target != null) 
            {
                resultMessage = TypeManager.IsValidateType(target,value.ToString());
                if (resultMessage == string.Empty)
                {
                    target.Value = value;
                    if (ErrorVariables.ContainsKey(fullName))
                    {
                        ErrorVariables.Remove(fullName);
                    }
                }
            }
            else
            {
                resultMessage = $"{fullName} Variable not found.";
            }

            return resultMessage;
        }
        public static YamlMappingNode ConvertYamlFromCode()
        {
            YamlMappingNode root = new YamlMappingNode();
            foreach (var variable in YmlVariables)
            {
                AddVariableToYamlNode(root, variable);
            }
            return root;
        }

        private static void AddVariableToYamlNode(YamlNode parentNode, VariableInfo variable)
        {
            if (variable.HasChildren())
            {
                YamlNode childNode;

                if (variable.TypeName == typeof(Dictionary<,>).Name)
                {
                    childNode = new YamlMappingNode();
                }
                else
                {
                    childNode = new YamlSequenceNode();
                }
                
                foreach (var child in variable.Children)
                {
                    AddVariableToYamlNode(childNode, child);
                }

                if (parentNode is YamlMappingNode mappingNode)
                {
                    mappingNode.Add(variable.Name, childNode);
                }
                else if (parentNode is YamlSequenceNode sequenceNode)
                {
                    sequenceNode.Add(childNode);
                }
            }
            else
            {
                YamlNode valueNode = new YamlScalarNode(variable.Value?.ToString() ?? string.Empty);

                if (parentNode is YamlMappingNode mappingNode)
                {
                    mappingNode.Add(variable.Name, valueNode);
                }
                else if (parentNode is YamlSequenceNode sequenceNode)
                {
                    sequenceNode.Add(valueNode);
                }
            }
        }
    }
}
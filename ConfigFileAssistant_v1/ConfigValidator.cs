using WheelHetergeneousInspectionSystem.Models;
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
        private static List<ConfigVariable> CsVariables;
        private static List<ConfigVariable> YmlVariables;
        private static Dictionary<String, ConfigVariable> ErrorVariables;

        public static void Init()
        {
            TypeManager.init();
            Instance = Activator.CreateInstance(typeof(Config));
            CsVariables = new List<ConfigVariable>();
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

        public static List<ConfigVariable> ExtractYmlVariables()
        {
            YmlVariables = new List<ConfigVariable>();
            ExtractYmlVariablesRecursive(Root, YmlVariables, "");
            return YmlVariables;
        }

        private static void ExtractYmlVariablesRecursive(YamlNode node, List<ConfigVariable> variables, string prefix)
        {
            if (node is YamlMappingNode mappingNode)
            {
                foreach (var entry in mappingNode.Children)
                {
                    var key = ((YamlScalarNode)entry.Key).Value;
                    var fullName = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

                    if (entry.Value is YamlMappingNode)
                    {
                        var childInfo = new ConfigVariable(fullName, typeof(Dictionary<,>), string.Empty);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, fullName);
                        variables.Add(childInfo);
                    }
                    else if (entry.Value is YamlSequenceNode)
                    {
                        var childInfo = new ConfigVariable(fullName, typeof(List<>), string.Empty);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, fullName);
                        variables.Add(childInfo);
                    }
                    else
                    {
                        var type = typeof(string);
                        variables.Add(new ConfigVariable(fullName, type, (entry.Value)));
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
                        var childInfo = new ConfigVariable(childPrefix, typeof(Dictionary<,>), string.Empty);
                        ExtractYmlVariablesRecursive(childNode, childInfo.Children, childPrefix);
                        variables.Add(childInfo);
                    }
                    else if (childNode is YamlSequenceNode)
                    {
                        var childInfo = new ConfigVariable(childPrefix, typeof(List<>), string.Empty);
                        ExtractYmlVariablesRecursive(childNode, childInfo.Children, childPrefix);
                        variables.Add(childInfo);
                    }
                    else if (childNode is YamlScalarNode scalarNode)
                    {
                        variables.Add(new ConfigVariable(childPrefix, typeof(string), scalarNode.Value));
                    }
                    index++;
                }
            }
            else if (node is YamlScalarNode scalarNode)
            {
                variables.Add(new ConfigVariable(prefix, typeof(string), scalarNode.Value));
            }
        }

        public static List<ConfigVariable> ExtractCsVariables()
        {
            var type = typeof(Config);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object defaultValue = GetDefaultValue(property, Instance);
                ExtractCsVariablesRecursive(property.Name, property.PropertyType, defaultValue, CsVariables);
            }
            return CsVariables;
        }
        private static void ExtractCsVariablesRecursive(string propertyName, Type propertyType, Object value, List<ConfigVariable> variables)
        {
            ConfigVariable ConfigVariable;
            if (!propertyType.IsGenericType)
            {
                ConfigVariable = new ConfigVariable(propertyName, propertyType, value);
                if (propertyType.IsEnum)
                {
                    ConfigVariable.SetEnumValues(ExtractEnumValues(ConfigVariable,propertyType));
                    if (propertyName.Split('.').Last() != "Key")
                    {
                        TypeManager.AddType(ConfigVariable.TypeName, ConfigVariable.Type);
                    }
                }
                variables.Add(ConfigVariable);
                return;
            }
            Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();
            Type[] genericArgs = propertyType.GetGenericArguments();
            ConfigVariable = new ConfigVariable(propertyName, propertyType, string.Empty);
            if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                ExtractCsVariablesRecursive($"{propertyName}.Key", genericArgs[0], "", ConfigVariable.Children);
                ExtractCsVariablesRecursive($"{propertyName}.Value", genericArgs[1], "", ConfigVariable.Children);

            }
            else if (genericTypeDefinition == typeof(List<>))
            {
                ExtractCsVariablesRecursive($"{propertyName}.Item", genericArgs[0], "", ConfigVariable.Children);
            }
            variables.Add(ConfigVariable);
        }

        private static List<ConfigVariable> ExtractEnumValues(ConfigVariable ConfigVariable, Type propertyType)
        {
            List<ConfigVariable> enumValues = new List<ConfigVariable> ();
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
                    enumValues.Add(new ConfigVariable(enumField.Name, type, ""));
                }
                else
                {
                    var constructorArguments = validatorAttributeData.ConstructorArguments;
                    var validatorType = (ValidatorType)constructorArguments[0].Value;
                    TypeManager.MakeFunction(enumField.Name,validatorType, constructorArguments[1].ToString());
                    enumValues.Add(new ConfigVariable(enumField.Name, validatorType.ToString(), ""));
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
        public static List<ConfigVariable> CompareVariables(List<ConfigVariable> csVariables, List<ConfigVariable> ymlVariables)
        {
            ErrorVariables = new Dictionary<string, ConfigVariable>();
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
                    var variable = new ConfigVariable(csVariable.Name, csVariable.Type, csVariable.Value);
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
        private static void CompareChild(ConfigVariable csVariable, ConfigVariable ymlVariable, ConfigVariable csParentVariable)
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

        private static void SetResult(ConfigVariable ConfigVariable, Result result)
        {
            ConfigVariable.Result = result;
            if(result != Result.Ok && result != Result.NoChild)
            {
                ErrorVariables.Add(ConfigVariable.FullName, ConfigVariable);
            }
        }
        private static Result CompareSingleValue(ConfigVariable ymlVariable, ConfigVariable csVariable)
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
        private static void SetDefaultType(ConfigVariable ymlVariable, ConfigVariable csVariable)
        {
            ymlVariable.Type = csVariable.Type;
            ymlVariable.TypeName = csVariable.TypeName; 
            ymlVariable.DefaultValue = csVariable.Value.ToString();
           
        }
        private static Result IsValidatedEnumValue(ConfigVariable ymlVariable, ConfigVariable csVariable)
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
        public static Dictionary<string,ConfigVariable> GetErrors()
        {
            return ErrorVariables;
        }

        public static void RemoveVariableFromErrorList(string error)
        {
            ErrorVariables.Remove(error);
        }
        
        public static List<ConfigVariable> GetParentVariables(string fullName)
        {
            var names = fullName.Split('.');
            int depth = names.Length;

            ConfigVariable parent = null;
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
        public static bool InsertChildToVariable(ConfigVariable variable, ConfigVariable child)
        {
            var parentVariables = GetParentVariables(variable.FullName);
            var target = parentVariables.Find(v => v.Name == variable.Name);
            if(target == null)
            {
                return false;
            }

            Dictionary<string, ConfigVariable> parentVariableDict = target.Children.ToDictionary(v => v.Name);
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
        public static bool InsertVariable(ConfigVariable variable, ConfigVariable newVariable)
        {
            if (variable == null)
            {
                Dictionary<string, ConfigVariable> csVariablesDict = CsVariables.ToDictionary(v => v.Name);
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
                Dictionary<string, ConfigVariable> parentVariableDict = parentVariables.ToDictionary(v => v.Name);
                if(parentVariableDict.ContainsKey(newVariable.Name))
                {
                    return false;
                }
                parentVariables.Insert(selectedIndex + 1, newVariable);
                return true;
            }
        }
        public static bool AddVariable(ConfigVariable ConfigVariable)
        {
            var parentVariables = GetParentVariables(ConfigVariable.FullName);
            var target = parentVariables.Find(v => v.Name == ConfigVariable.Name);
            if (target != null)
            {
                target.Result = Result.Ok;
                return true;
            }
            return false;
        }
        public static bool RemoveVariable(ConfigVariable ConfigVariable)
        {
            var parentVariables = GetParentVariables(ConfigVariable.FullName);
            
            var target = parentVariables.Find(v => v.Name == ConfigVariable.Name);
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

        public static bool ModifyChildFromVariable(ConfigVariable ConfigVariable)
        {
            var parentVariables = GetParentVariables(ConfigVariable.FullName);

            var target = parentVariables.Find(v => v.Name == ConfigVariable.Name);
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

        private static void AddVariableToYamlNode(YamlNode parentNode, ConfigVariable variable)
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
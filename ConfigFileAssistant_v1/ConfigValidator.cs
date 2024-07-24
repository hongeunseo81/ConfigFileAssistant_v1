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
        OK,
        ONLY_IN_CS,
        ONLY_IN_YML,
        WRONG_RANGE,
        WRONG_VALUE
    }

    public class ConfigValidator
    {
        private static Object instance;
        private static YamlMappingNode root;
        private static YamlStream yaml;
        private static List<VariableInfo> csVariables;
        private static List<VariableInfo> ymlVariables;
        private static Dictionary<String, VariableInfo> errorVariableNames;

        public static void Init()
        {
            instance = Activator.CreateInstance(typeof(Config));
            csVariables = new List<VariableInfo>();
        }
        public static void LoadYamlFile(string filePath)
        {
            yaml = new YamlStream();
            using (var reader = new StreamReader(filePath))
            {
                yaml.Load(reader);
            }
            if (yaml.Documents.Count != 0)
            {
                root = (YamlMappingNode)yaml.Documents[0].RootNode;
            }
            else
            {
                root = new YamlMappingNode();
            }
        }
        public static List<VariableInfo> ExtractYmlVariables()
        {
            ymlVariables = new List<VariableInfo>();
            ExtractYmlVariablesRecursive(root, ymlVariables, "");
            return ymlVariables;
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
                object defaultValue = GetDefaultValue(property, instance);
                ExtractCsVariablesRecursive(property.Name, property.PropertyType, defaultValue, csVariables);
            }
            return csVariables;
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
                    TypeFinder.MakeFunction(enumField.Name,validatorType, constructorArguments[1].ToString());
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
            errorVariableNames = new Dictionary<string, VariableInfo>();
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
                        SetResult(ymlVariable,Result.WRONG_VALUE);
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
                    SetResult(variable, Result.ONLY_IN_CS);
                    ymlVariables.Add(variable);
                }
            }

            foreach (var ymlVariable in ymlVariables)
            {
                if (!csVariables.Any(v => v.Name == ymlVariable.Name))
                {
                    SetResult(ymlVariable, Result.ONLY_IN_YML);
                }
            }
            return ymlVariables;
        }
        private static void CompareChild(VariableInfo csVariable, VariableInfo ymlVariable, VariableInfo csParentVariable)
        {
            SetDefaultType(ymlVariable, csVariable);
            Result result = Result.OK;
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
                    result = Result.WRONG_VALUE;
                }
            }
            // cs에는 있고 yml에는 없는 경우
            else if (csVariable.HasChildren() && !ymlVariable.HasChildren())
            {
                if (ymlVariable.Value.ToString() != string.Empty)
                {
                    result = Result.WRONG_VALUE;
                }
                else
                {
                    result = Result.OK;
                }
            }
            // cs에는 없고 yml에는 있는 경우
            else if (!csVariable.HasChildren() && ymlVariable.HasChildren())
            {
                result = Result.WRONG_VALUE;
            }
            // 둘다 자식이 없는 경우
            else
            {
                if(csParentVariable != null && csParentVariable.Children[0].Type.IsEnum)
                {
                    result = IsValidatedEnumValue(ymlVariable, csParentVariable.Children[0]);
                }
                else
                {
                    result = CompareSingleValue(ymlVariable, csVariable);

                }
            }
            SetResult(ymlVariable,result);
        }

        private static void SetResult(VariableInfo variableInfo, Result result)
        {
            variableInfo.Result = result;
            if(result != Result.OK)
            {
                errorVariableNames.Add(variableInfo.FullName, variableInfo);
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
                    ymlVariable.Value = csVariable.Value;
                    return Result.OK;
                }
                return Result.WRONG_VALUE;
            }
            else
            {
                if (TypeFinder.IsValidateType(ymlVariable, ymlVariable.Value.ToString()) != string.Empty)
                {
                    return Result.WRONG_VALUE;
                }
                return Result.OK;
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
                ymlVariable.Value = values[ymlVariable.Name].Value;
                if (TypeFinder.IsValidateType(ymlVariable, ymlVariable.Value.ToString()) == string.Empty)
                {
                    return Result.OK;
                }
                return Result.WRONG_VALUE;
            }
            return Result.ONLY_IN_YML;
            
        }
        public static bool HasErrors()
        {
            return errorVariableNames != null && errorVariableNames.Count > 0;
        }
        public static Dictionary<string,VariableInfo> GetErrors()
        {
            return errorVariableNames;
        }

        public static void RemoveVariableFromErrorList(string error)
        {
            errorVariableNames.Remove(error);
        }
        
        public static VariableInfo GetParentVariable(string fullName)
        {
            var names = fullName.Split('.');
            int depth = names.Length;

            VariableInfo parent = null;
            var currentList = ymlVariables;

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
            return parent;
        }
        public static bool AddChildToVariable(VariableInfo variableInfo)
        {
            var parent = GetParentVariable(variableInfo.FullName);
            List<VariableInfo> parentVariables = parent == null ? ymlVariables : parent.Children;

            var target = parentVariables.Find(v => v.Name == variableInfo.Name);
            if (target != null)
            {
                target.Result = Result.OK;
                return true;
            }
            return false;
        }
        public static bool RemoveChildFromVariable(VariableInfo variableInfo)
        {
            var parent = GetParentVariable(variableInfo.FullName);
            List<VariableInfo> parentVariables = parent == null? ymlVariables : parent.Children;
            
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
            var parent = GetParentVariable(variableInfo.FullName);
            List<VariableInfo> parentVariables = parent == null ? ymlVariables : parent.Children;

            var target = parentVariables.Find(v => v.Name == variableInfo.Name);
            if(target != null)
            {
                if (target.HasChildren())
                {
                    target.Children.Clear();
                }
                target.Value = target.DefaultValue;
                target.Result = Result.OK;
                return true;
            }
            return false;
        }
        public static string UpdateChild(string fullName, object value)
        {
            var parent = GetParentVariable(fullName);
            List<VariableInfo> parentVariables = parent == null ? ymlVariables : parent.Children;
            var names = fullName.Split('.');
            var target = parentVariables.Find(v => v.Name == names.Last());
            var resultMessage = string.Empty;
            if (target != null) 
            {
                resultMessage = TypeFinder.IsValidateType(target,value.ToString());
                if (resultMessage == string.Empty)
                {
                    target.Value = value;
                    if (errorVariableNames.ContainsKey(fullName))
                    {
                        errorVariableNames.Remove(fullName);
                    }
                }
            }
            else
            {
                resultMessage = $"{fullName} 변수를 찾을 수 없습니다.";
            }

            return resultMessage;
        }


        public static void MakeYamlFile(List<VariableInfo> variables, string filePath)
        {

        }
        private static void MakeBackup(string filePath)
        {
            string backupFolder = Path.Combine(Path.GetDirectoryName(filePath), "configbackup");
            Directory.CreateDirectory(backupFolder);
            string backupFilePath = Path.Combine(backupFolder, $"config_{DateTime.Now:yyyyMMddHHmmss}.yml");
            File.Copy(filePath, backupFilePath, true);
        }
    }
}
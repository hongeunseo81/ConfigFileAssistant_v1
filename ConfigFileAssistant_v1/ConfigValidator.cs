using CalibrationTool;
using CoPick.Setting;
using ScintillaNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;
using YamlDotNet.RepresentationModel;

namespace ConfigFileAssistant_v1
{
    public class VariableInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public Type Type { get; set; }
        public string TypeName { get; set; }
        public object Value { get; set; }
        public Result Result { get; set; }
        public string DefaultType {  get; set; }
        public List<VariableInfo> Children { get; set; }
        
        public void AddChild(VariableInfo child)
        {
            
            Children.Add(child);
        }

        public VariableInfo(string name, Type type, object value)
        {
            FullName = name;
            Name = FullName.Split('.').Last();
            Type = type;
            TypeName = type.Name;
            Children = new List<VariableInfo>();
            Value = value;
            Result = Result.OK;
        }
        public VariableInfo(string name, string typeName, object value)
        {
            FullName = name;
            Name = FullName.Split('.').Last();
            Type = typeof(string);
            TypeName = typeName;
            Children = new List<VariableInfo>();
            Value = value;
            Result = Result.OK;
        }
        public bool HasChildren()
        {
            return Children.Count > 0;
        }
    }
    
    public enum Result 
    { 
        OK,
        ONLY_IN_CS,
        ONLY_IN_YML,
        TYPE_MISMATCH
    }

    public class ConfigValidator
    {
        public static Object instance;
        public static Dictionary<String,VariableInfo> errorVariableNames;
        public static YamlMappingNode root;
        public static YamlStream yaml;
        public static List<VariableInfo> csVariables;
        public static List<VariableInfo> ymlVariables;

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
                    else if (entry.Value is YamlScalarNode scalarNode)
                    {
                        var type = typeof(string);
                        variables.Add(new VariableInfo(fullName, type, scalarNode.Value));
                    }
                }
            }
            else if (node is YamlSequenceNode sequenceNode)
            {
                int index = 0;
                foreach (var childNode in sequenceNode.Children)
                {
                    var childPrefix = $"{prefix}[{index}]";

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
            csVariables = new List<VariableInfo>();
            var type = typeof(Config);
            instance = Activator.CreateInstance(typeof(Config));

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object defaultValue = GetDefaultValue(property,instance);
                ProcessProperty(property.Name, property.PropertyType, defaultValue, csVariables);
            }
            return csVariables;
        }
        
        private static void ProcessProperty(string propertyName, Type propertyType, Object value,List<VariableInfo> variables)
        {
            VariableInfo variableInfo; 
            if (!propertyType.IsGenericType)
            {
                variableInfo = new VariableInfo(propertyName, propertyType, value);
                if (propertyType.IsEnum)
                {
                    foreach (var item in propertyType.GetEnumValues())
                    {
                        var enumField = propertyType.GetField(item.ToString());
                        var validatorAttributeData = enumField.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType == typeof(ValidatorTypeAttribute));

                        if (validatorAttributeData == null)
                        {
                            variableInfo.Children.Add(new VariableInfo(enumField.Name, item.ToString(), ""));
                            continue;
                        }

                        var constructorArguments = validatorAttributeData.ConstructorArguments;
                        var validatorType = (ValidatorType)constructorArguments[0].Value;

                        variableInfo.Children.Add(new VariableInfo(enumField.Name, validatorType.ToString(),""));
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
                Type keyType = genericArgs[0];
                Type valueType = genericArgs[1];

                ProcessProperty($"{propertyName}.Key", keyType,"", variableInfo.Children);
                ProcessProperty($"{propertyName}.Value", valueType,"", variableInfo.Children);

            }
            else if (genericTypeDefinition == typeof(List<>))
            {
                Type itemType = genericArgs[0];
                ProcessProperty($"{propertyName}.Item", itemType, "", variableInfo.Children);
            }
            variables.Add(variableInfo);
        }
        public static object GetDefaultValue(PropertyInfo property, object instance)
        {
            var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null)
            {
                return defaultValueAttribute.Value;
            }

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
            errorVariableNames = new Dictionary<string,VariableInfo>();
            var ymlVariableDict = ymlVariables.ToDictionary(v => v.Name);

            foreach (var csVariable in csVariables)
            {
                if (ymlVariableDict.TryGetValue(csVariable.Name, out var ymlVariable))
                {
                    if((csVariable.Type.IsGenericType || ymlVariable.Type.IsGenericType) && !csVariable.TypeName.Equals(ymlVariable.TypeName))
                    {
                        ymlVariable.Result = Result.TYPE_MISMATCH;
                        ymlVariable.DefaultType = csVariable.TypeName;
                        errorVariableNames.Add(ymlVariable.FullName,ymlVariable);
                    }
                    else
                    {
                        CompareChild(csVariable, ymlVariable, null);
                    }
                }
                else
                {
                    var variable = new VariableInfo(csVariable.Name, csVariable.Type, csVariable.Value);
                    variable.Result = Result.ONLY_IN_CS;
                    ymlVariables.Add(variable); 
                    errorVariableNames.Add(variable.FullName, variable);

                }
            }

            foreach (var ymlVariable in ymlVariables)
            {
                if (!csVariables.Any(v => v.Name == ymlVariable.Name))
                {
                    ymlVariable.Result = Result.ONLY_IN_YML; 
                    errorVariableNames.Add(ymlVariable.FullName, ymlVariable);

                }
            }
            return ymlVariables;
        }
        private static void CompareChild(VariableInfo csVariable, VariableInfo ymlVariable, VariableInfo csParentVariable)
        {
            if (csVariable.HasChildren() && ymlVariable.HasChildren())
            {
                foreach (var child in ymlVariable.Children)
                {
                    CompareChild(csVariable.Children.Last(), child, csVariable);
                }
            }
            else if (csVariable.HasChildren() && !ymlVariable.HasChildren())
            {
                if (csVariable.Type.IsEnum)
                {
                    ymlVariable.Result = Result.TYPE_MISMATCH;
                    foreach (var item in csVariable.Children)
                    {
                        if (ymlVariable.Value.ToString().Equals(item.Name))
                        {
                            ymlVariable.Result = Result.OK;
                            break;
                        }
                    }
                    if(ymlVariable.Result == Result.TYPE_MISMATCH) 
                    {
                        errorVariableNames.Add(ymlVariable.FullName, ymlVariable);

                    }
                }
                else if (!csVariable.TypeName.Equals(ymlVariable.TypeName))
                {
                    ymlVariable.Result = Result.TYPE_MISMATCH;
                    ymlVariable.DefaultType = csVariable.TypeName;
                    errorVariableNames.Add(ymlVariable.FullName, ymlVariable);
                }
                else
                {
                    ymlVariable.Result = Result.OK;
                }
                
            }
            else if (!csVariable.HasChildren() && ymlVariable.HasChildren())
            {
                ymlVariable.Result = Result.TYPE_MISMATCH;
                ymlVariable.DefaultType = csVariable.TypeName;
                errorVariableNames.Add(ymlVariable.FullName, ymlVariable);
            }
            else
            {
                if (csParentVariable != null) 
                {
                    ymlVariable.Result = Result.ONLY_IN_YML;
                    var variable = csParentVariable.Children.First();
                    foreach(var v in variable.Children)
                    {
                        if(ymlVariable.Name.Equals(v.Name))
                        {
                            ymlVariable.Result = Result.OK;
                            break;
                        }
                    }
                    if(ymlVariable.Result == Result.ONLY_IN_YML)
                    {
                        errorVariableNames.Add(ymlVariable.FullName,ymlVariable);
                    }
                }
            }
        }

        public static bool HasError()
        {
            return errorVariableNames.Count > 0;
        }
        public static void MigrateVariables(List<VariableInfo> csVariables, List<VariableInfo> ymlVariables, string filePath)
        {
            foreach (var key in errorVariableNames.Keys)
            {
                if (errorVariableNames.TryGetValue(key, out VariableInfo variableInfo))
                {
                    var names = key.Split('.');
                    int index = 0;
                    YamlNode targetNode = FindYamlNode(root, names, index, -1);

                    if (targetNode != null && index < names.Length)
                    {
                        ModifyYamlNode(targetNode, names[names.Length - 1], variableInfo);
                    }
                    else
                    {
                        Debug.WriteLine($"can not find node {key}");
                    }
                }
            }
            using (var writer = new StreamWriter(filePath))
            {
                yaml.Save(writer, assignAnchors: false);
            }
        }
        public static void RemoveChild(List<VariableInfo> variables, string fullName)
        {
            VariableInfo target = variables.FirstOrDefault(v => v.FullName == fullName);
            if (target == null)
            {
                var names = fullName.Split('.');
                int index = 0;
                VariableInfo currentParent = variables.FirstOrDefault(v => fullName.StartsWith(v.FullName)); ;

                while (index < names.Length || target == null)
                {
                    var current = currentParent.Children.FirstOrDefault(v => fullName.StartsWith(v.FullName));
                    if (current == null)
                    {
                        target = currentParent;
                    }
                    else
                    {
                        currentParent = current;
                    }
                    target = currentParent.Children.FirstOrDefault(v => fullName == v.FullName);

                    index++;
                }
                currentParent.Children.Remove(target);  
            }

        }

        public static void UpdateCellValueToNode(Dictionary<string,string> editedValues, string filePath)
        {
            Debug.WriteLine("backup");
            MakeBackup(filePath);
            foreach(var key in editedValues.Keys)
            {
                var names = key.Split('.');
                int index = 0;
                YamlNode targetNode = FindYamlNode(root, names, index, -1);
                if (targetNode != null && index < names.Length)
                {
                    if (targetNode is YamlMappingNode mapping)
                    {
                        mapping.Children[names.Last()] = new YamlScalarNode(editedValues[key].ToString());
                    }
                }
            }
           
        }
        private static YamlNode FindYamlNode(YamlNode node, string[] names, int index, int arrayIndex)
        {
            if (index == names.Length - 1)
            {
                return node;
            }

            if (node is YamlMappingNode mappingNode)
            {
                var match = Regex.Match(names[index], @"^(\w+)\[(\d+)\]$");

                if (match.Success)
                {
                    string arrayName = match.Groups[1].Value;
                    arrayIndex = int.Parse(match.Groups[2].Value);

                    if (mappingNode.Children.TryGetValue(new YamlScalarNode(arrayName), out YamlNode arrayNode) &&
                        arrayNode is YamlSequenceNode sequenceNode &&
                        arrayIndex < sequenceNode.Children.Count)
                    {
                        return FindYamlNode(sequenceNode.Children[arrayIndex], names, index + 1, arrayIndex);
                    }
                }
                else
                {
                    if (mappingNode.Children.TryGetValue(new YamlScalarNode(names[index]), out YamlNode nextNode))
                    {
                        return FindYamlNode(nextNode, names, index + 1, arrayIndex);
                    }
                }
            }

            return null;
        }

        private static void ModifyYamlNode(YamlNode node, string name, VariableInfo variableInfo)
        {
            if (node is YamlMappingNode mapping)
            {
                switch (variableInfo.Result)
                {
                    case Result.ONLY_IN_CS:
                        mapping.Add(new YamlScalarNode(name), new YamlScalarNode(variableInfo.Value.ToString()));
                        break;
                    case Result.TYPE_MISMATCH:
                        if (variableInfo.DefaultType.Equals(typeof(Dictionary<,>).Name))
                        {
                            mapping.Children[name] = new YamlMappingNode();
                        }
                        else if (variableInfo.DefaultType.Equals(typeof(List<>).Name))
                        {
                            mapping.Children[name] = new YamlSequenceNode();
                        }
                        else
                        {
                            mapping.Children[name] = new YamlScalarNode(variableInfo.Value.ToString());
                        }
                        break;
                    case Result.ONLY_IN_YML:
                        mapping.Children.Remove(name);
                        break;
                    default:
                        break;
                }
            }
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
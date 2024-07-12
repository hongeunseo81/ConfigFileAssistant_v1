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
        ERROR,
        ONLY_IN_CS,
        ONLY_IN_YML,
        TYPE_MISMATCH
    }

    public class ConfigValidator
    {
        public static Object instance;
        public static Dictionary<string, Result> errorVariableNames;
        public static List<VariableInfo> ExtractYmlVariables(string filePath)
        {
            var variables = new List<VariableInfo>();

            var yaml = new YamlStream();
            using (var reader = new StreamReader(filePath))
            {
                yaml.Load(reader);
            }
            if (yaml.Documents.Count != 0)
            {
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                ExtractYmlVariablesRecursive(mapping, variables, "");
            }
            return variables;
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
                        var childInfo = new VariableInfo(fullName, typeof(Dictionary<,>), null);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, fullName);
                        variables.Add(childInfo);
                    }
                    else if (entry.Value is YamlSequenceNode)
                    {
                        var childInfo = new VariableInfo(fullName, typeof(List<>), null);
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
                        var childInfo = new VariableInfo(childPrefix, typeof(Dictionary<,>), null);
                        ExtractYmlVariablesRecursive(childNode, childInfo.Children, childPrefix);
                        variables.Add(childInfo);
                    }
                    else if (childNode is YamlSequenceNode)
                    {
                        var childInfo = new VariableInfo(childPrefix, typeof(List<>), null);
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
            var variables = new List<VariableInfo>();
            var type = typeof(Config);
            instance = Activator.CreateInstance(typeof(Config));

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object defaultValue = GetDefaultValue(property,instance);
                ProcessProperty(property.Name, property.PropertyType, defaultValue, variables);
            }
            return variables;
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
            variableInfo = new VariableInfo(propertyName, propertyType, null);
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
            errorVariableNames = new Dictionary<string,Result>();
            var ymlVariableDict = ymlVariables.ToDictionary(v => v.Name);

            foreach (var csVariable in csVariables)
            {
                if (ymlVariableDict.TryGetValue(csVariable.Name, out var ymlVariable))
                {
                    if((csVariable.Type.IsGenericType || ymlVariable.Type.IsGenericType) && !csVariable.TypeName.Equals(ymlVariable.TypeName))
                    {
                        ymlVariable.Result = Result.TYPE_MISMATCH;
                        errorVariableNames[ymlVariable.FullName] = ymlVariable.Result; 
                        
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
                    errorVariableNames[variable.FullName] = variable.Result;
                }
            }

            foreach (var ymlVariable in ymlVariables)
            {
                if (!csVariables.Any(v => v.Name == ymlVariable.Name))
                {
                    ymlVariable.Result = Result.ONLY_IN_YML; 
                    Dictionary<string, Result> errorVariable = new Dictionary<string, Result>();
                    errorVariableNames[ymlVariable.FullName] = ymlVariable.Result;
                   
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
                    if (child.Result != Result.OK)
                    {
                        ymlVariable.Result = Result.ERROR;
                    }
                }
               
            }
            else if (csVariable.HasChildren() && !ymlVariable.HasChildren())
            {
                if (csVariable.Type.IsEnum)
                {
                    ymlVariable.Result = Result.ONLY_IN_YML;
                    foreach (var item in csVariable.Children)
                    {
                        if (ymlVariable.Value.ToString().Equals(item.Name))
                        {
                            ymlVariable.Result = Result.OK;
                            break;
                        }
                    }
                    if(ymlVariable.Result == Result.ONLY_IN_YML) 
                    {
                        Dictionary<string, Result> errorVariable = new Dictionary<string, Result>();
                        errorVariableNames[ymlVariable.FullName] = ymlVariable.Result;
                        
                    }
                }
                else
                {
                    ymlVariable.Result = Result.TYPE_MISMATCH;
                    Dictionary<string, Result> errorVariable = new Dictionary<string, Result>();
                    errorVariableNames[ymlVariable.FullName] = ymlVariable.Result;
                }

            }
            else if (!csVariable.HasChildren() && ymlVariable.HasChildren())
            {
                ymlVariable.Result = Result.TYPE_MISMATCH; 
                Dictionary<string, Result> errorVariable = new Dictionary<string, Result>();
                errorVariableNames[ymlVariable.FullName] = ymlVariable.Result;
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
                        Dictionary<string, Result> errorVariable = new Dictionary<string, Result>();
                        errorVariableNames[ymlVariable.FullName] = ymlVariable.Result;
                    }
                }
            }
           
        }
        public static void MigrateVariables(List<VariableInfo> csVariables, List<VariableInfo> ymlVariables, string filePath)
        {
            var yaml = new YamlStream();
            using (var reader = new StreamReader(filePath))
            {
                yaml.Load(reader);
            }
            YamlMappingNode mapping;
            if (yaml.Documents.Count == 0)
            {
                mapping = new YamlMappingNode();
                var newDocument = new YamlDocument(mapping);
                yaml.Documents.Add(newDocument);
            }
            else
            {
                MakeBackup(filePath);
                mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            }

            foreach (var key in errorVariableNames.Keys)
            {
                VariableInfo variable = ymlVariables.Find(x => x.FullName.Equals(key));
                
                var names = key.Split('.');
                // FindYamlNode(mapping, names, 0, variable);
            }
            using (var writer = new StreamWriter(filePath))
            {
                yaml.Save(writer, assignAnchors: false);
            }
        }
       
        private static void  FindYamlNode(YamlNode node, string[] names, int index, VariableInfo variable)
        {
            
            if (node is YamlMappingNode mappingNode)
            {
                var match = Regex.Match(names[index], @"^(\w+)\[(\d+)\]$");

                if (match.Success)
                {
                    string arrayName = match.Groups[1].Value;
                    int arrayIndex = int.Parse(match.Groups[2].Value);

                    if (mappingNode.Children.TryGetValue(new YamlScalarNode(arrayName), out YamlNode arrayNode) &&
                        arrayNode is YamlSequenceNode sequenceNode &&
                        arrayIndex < sequenceNode.Children.Count)
                    {
                         FindYamlNode(sequenceNode.Children[arrayIndex], names, index + 1, variable);
                    }
                }
                else
                {
                    if (mappingNode.Children.TryGetValue(new YamlScalarNode(names[index]), out YamlNode nextNode))
                    {
                         FindYamlNode(nextNode, names, index + 1, variable);
                    }
                }
            }
            else if (node is YamlSequenceNode sequenceNode)
            {
                if (int.TryParse(names[index], out int seqIndex) && seqIndex < sequenceNode.Children.Count)
                {
                     FindYamlNode(sequenceNode.Children[seqIndex], names, index + 1, variable);
                }
            }
            else
            {
                var result = variable.Result;
                switch (result)
                {
                    case Result.ONLY_IN_CS:
                        Debug.WriteLine(names[index] + ":" + result);
                        break;

                    case Result.TYPE_MISMATCH:

                        Debug.WriteLine(names[index] + ":" + result);
                        break;
                    case Result.ONLY_IN_YML:

                        Debug.WriteLine(names[index] + ":" + result);
                        break;

                    default:

                        Debug.WriteLine(names[index] + ":" + result);
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
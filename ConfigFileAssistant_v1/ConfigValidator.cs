using CalibrationTool;
using CoPick.Setting;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using YamlDotNet.Core.Tokens;
using YamlDotNet.RepresentationModel;

namespace ConfigFileAssistant_v1
{

    public class VariableInfo
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public string TypeName { get; set; }
        public object Value { get; set; }
        public List<VariableInfo> Children { get; set; }
        public Note Note { get; set; }
        
        public void AddChild(VariableInfo child)
        {
            
            Children.Add(child);
        }

        public VariableInfo(string name, Type type, object value)
        {
            Name = name;
            Type = type;
            TypeName = type.Name;
            Children = new List<VariableInfo>();
            Value = value;
        }
        public bool HasChildren()
        {
            return Children.Count > 0;
        }
    }

    public enum Note 
    { 
        OK,
        CS_ONLY,
        YML_ONLY,
        TYPE_MISMATCH
    }

    public class ConfigValidator
    {
        public static List<VariableInfo> ExtractYmlVariables(string filePath)
        {
            var variables = new List<VariableInfo>();

            var yaml = new YamlStream();
            using (var reader = new StreamReader(filePath))
            {
                yaml.Load(reader);
            }
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            ExtractYmlVariablesRecursive(mapping, variables, "");

            return variables;
        }
        private static void ExtractYmlVariablesRecursive(YamlNode node, List<VariableInfo> variables, string prefix)
        {
            if (node is YamlMappingNode mappingNode)
            {
                foreach (var entry in mappingNode.Children)
                {
                    var key = ((YamlScalarNode)entry.Key).Value;

                    if (entry.Value is YamlMappingNode)
                    {
                        var childInfo = new VariableInfo(key, typeof(Dictionary<,>), null);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, key);
                        variables.Add(childInfo);
                    }
                    else if (entry.Value is YamlSequenceNode)
                    {
                        var childInfo = new VariableInfo(key,typeof(List<>), null);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, key);
                        variables.Add(childInfo);
                    }
                    else if (entry.Value is YamlScalarNode scalarNode)
                    {
                        var type = GetTypeFromScalar(scalarNode);
                        variables.Add(new VariableInfo(key, type, scalarNode.Value));
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
                        var childInfo = new VariableInfo(childPrefix, typeof(Dictionary<,>), null);
                        ExtractYmlVariablesRecursive(childNode, childInfo.Children, childPrefix);
                        variables.Add(childInfo);
                    }
                    else if (childNode is YamlScalarNode scalarNode)
                    {
                        var type = GetTypeFromScalar(scalarNode);
                        variables.Add(new VariableInfo(childPrefix, type, scalarNode.Value));
                    }
                    index++;
                }
            }
            else if (node is YamlScalarNode scalarNode)
            {
                var type = GetTypeFromScalar(scalarNode);
                variables.Add(new VariableInfo(prefix, type, scalarNode.Value));
            }
        }
        private static Type GetTypeFromScalar(YamlScalarNode scalarNode)
        {
            if (int.TryParse(scalarNode.Value, out _)) return typeof(int);
            if (bool.TryParse(scalarNode.Value, out _)) return typeof(bool);
            return typeof(string);
        }

        public static List<VariableInfo> ExtractCsVariables()
        {
            var variables = new List<VariableInfo>();
            var type = typeof(Config);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                ProcessProperty(property.Name, property.PropertyType, null, variables);
            }
            return variables;
        }

        private static void ProcessProperty(string propertyName, Type propertyType, object value, List<VariableInfo> variables)
        {
            var variableInfo = new VariableInfo(propertyName, propertyType, value);

            if (!propertyType.IsGenericType)
            {
                if (propertyType.IsEnum)
                {
                    foreach (var item in propertyType.GetEnumValues())
                    {
                        var enumField = propertyType.GetField(item.ToString());
                        var validatorAttributeData = enumField.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType == typeof(ValidatorTypeAttribute));

                        if (validatorAttributeData == null)
                        {
                            variableInfo.Children.Add(new VariableInfo(enumField.Name, typeof(string), item));
                            continue;
                        }

                        var constructorArguments = validatorAttributeData.ConstructorArguments;
                        var validatorType = (ValidatorType)constructorArguments[0].Value;

                        variableInfo.Children.Add(new VariableInfo(enumField.Name, typeof(string), validatorType.ToString()));
                    }
                }
                variables.Add(variableInfo);
                return;
            }

            Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();
            Type[] genericArgs = propertyType.GetGenericArguments();

            if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                Type keyType = genericArgs[0];
                Type valueType = genericArgs[1];
                variableInfo.Children.Add(new VariableInfo("Key", keyType, null));
                ProcessProperty("Value", valueType, null, variableInfo.Children);
            }
            else if (genericTypeDefinition == typeof(List<>))
            {
                Type itemType = genericArgs[0];
                ProcessProperty("Item", itemType, null, variableInfo.Children);
            }

            variables.Add(variableInfo);
        }

        
    }
}
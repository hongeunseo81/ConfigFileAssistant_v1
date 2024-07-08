using CalibrationTool;
using CoPick.Setting;
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
        public string Type { get; set; }
        public object Value { get; set; }
        public List<VariableInfo> Children { get; set; }

        public void AddChild(VariableInfo child)
        {
            Children.Add(child);
        }

        public VariableInfo(string name, string type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
            Children = new List<VariableInfo>();
        }
        public bool HasChildren()
        {
            return Children.Count > 0;
        }
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
                        var childInfo = new VariableInfo(key, "Dictionary", null);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, key);
                        variables.Add(childInfo);
                    }
                    else if (entry.Value is YamlSequenceNode)
                    {
                        var childInfo = new VariableInfo(key, "List", null);
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
                        var childInfo = new VariableInfo(childPrefix, "Dictionary", null);
                        ExtractYmlVariablesRecursive(childNode, childInfo.Children, childPrefix);
                        variables.Add(childInfo);
                    }
                    else if (childNode is YamlSequenceNode)
                    {
                        var childInfo = new VariableInfo(childPrefix, "List", null);
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
        private static string GetTypeFromScalar(YamlScalarNode scalarNode)
    {
        if (int.TryParse(scalarNode.Value, out _)) return "Integer";
        if (bool.TryParse(scalarNode.Value, out _)) return "Boolean";
        return "String";
    }

    public static Dictionary<string, string> ExtractCsVariables()
        {
            var variables = new Dictionary<string, string>();
            var type = typeof(Config);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                ProcessProperty(property.Name, property.PropertyType, variables);
            }
            return variables;

        }
        private static void ProcessProperty(string propertyName, Type propertyType, Dictionary<string, string> variables)
        {
            if (!propertyType.IsGenericType)
            {
                if (propertyType.IsEnum)
                {
                    variables[propertyName] = propertyType.Name;
                    foreach (var item in propertyType.GetEnumValues())
                    {
                        var enumField = propertyType.GetField(item.ToString());
                        var validatorAttributeData = enumField.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType == typeof(ValidatorTypeAttribute));

                        if (validatorAttributeData is null)
                        {
                            variables[$"{propertyName}.{item.ToString()}"] = "string";
                            continue;
                        }

                        var constructorArguments = validatorAttributeData.ConstructorArguments;
                        var validatorType = (ValidatorType)constructorArguments[0].Value;

                         variables[$"{propertyName}.{item.ToString()}"] = validatorType.ToString();
                    }
                }
                else
                {
                    variables[propertyName] = propertyType.Name;
                }

                return;
            }

            Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();
            Type[] genericArgs = propertyType.GetGenericArguments();
            if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                Type keyType = genericArgs[0];
                Type valueType = genericArgs[1];
                variables[propertyName] = $"Dictionary<{keyType.Name}, {GetInnermostTypeName(valueType)}>";
                ProcessProperty($"{propertyName}.Key", keyType, variables);
                ProcessProperty($"{propertyName}.Value", valueType, variables);
            }
            else if (genericTypeDefinition == typeof(List<>))
            {
                Type itemType = genericArgs[0];
                variables[propertyName] = $"List<{GetInnermostTypeName(itemType)}>";

                ProcessProperty($"{propertyName}.Item", itemType, variables);
            }
            else
            {
                variables[propertyName] = propertyType.Name;
            }
        }

        private static string GetInnermostTypeName(Type type)
        {
            while (type.IsGenericType)
            {
                type = type.GetGenericArguments().First();
            }
            return type.Name;
        }

    }
}

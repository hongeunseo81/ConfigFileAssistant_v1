using CalibrationTool;
using CoPick.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.RepresentationModel;

namespace ConfigFileAssistant_v1
{
    public enum NoteMessage
    {
        CS_ONLY,
        YML_ONLY,
        TYPE_MISMATCH,
        OK
    }
    public class VariableInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }

        public VariableInfo(string name, string type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }

    public class Variable
    {
        public string Name { get; set; }
        public string CsType { get; set; }
        public string YmlType { get; set; }
        public NoteMessage Note { get; set; }
        public Variable(string name, NoteMessage note) 
        {
   
            Name = name;
            Note = note;
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
                variables.Add(new VariableInfo(prefix, "Dictionary", new YamlMappingNode()));
                foreach (var entry in mappingNode.Children)
                {
                    var key = ((YamlScalarNode)entry.Key).Value;
                    var newPrefix = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                    ExtractYmlVariablesRecursive(entry.Value, variables, newPrefix);
                }
            }
            
            else if (node is YamlScalarNode scalarNode)
            {
                var type = "String";
                if (int.TryParse(scalarNode.Value, out _)) type = "Integer";
                else if (bool.TryParse(scalarNode.Value, out _)) type = "Boolean";

                variables.Add(new VariableInfo(prefix,type, scalarNode.Value));
            }
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

using CalibrationTool;
using CoPick.Setting;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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
        public Result Result { get; set; }
        public List<VariableInfo> Children { get; set; }
        
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
            Result = Result.OK;
        }
        public bool HasChildren()
        {
            return Children.Count > 0;
        }
    }
    public class UpdateLog
    {
        public StringBuilder Message { get; set; }
        public DateTime CreatedAt { get; set; }

        public UpdateLog() 
        { 
            CreatedAt = DateTime.Now;
            Message = new StringBuilder();
            Message.Append(CreatedAt).Append("\n\n");
        }
        public void AddMessage(string message)
        {
            Message.Append(message);
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
        public static List<VariableInfo> ExtractYmlVariables(string filePath)
        {
            var variables = new List<VariableInfo>();

            var yaml = new YamlStream();
            using (var reader = new StreamReader(filePath))
            {
                yaml.Load(reader);
            }
            if(yaml.Documents.Count != 0)
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

                    if (entry.Value is YamlMappingNode)
                    {
                        var childInfo = new VariableInfo(key, typeof(Dictionary<,>), null);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, key);
                        variables.Add(childInfo);
                    }
                    else if (entry.Value is YamlSequenceNode)
                    {
                        var childInfo = new VariableInfo(key, typeof(List<>), null);
                        ExtractYmlVariablesRecursive(entry.Value, childInfo.Children, key);
                        variables.Add(childInfo);
                    }
                    else if (entry.Value is YamlScalarNode scalarNode)
                    {
                        // var type = GetTypeFromScalar(scalarNode);
                        var type = typeof(string);
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
            VariableInfo variableInfo = new VariableInfo(propertyName, propertyType, value);
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

                ProcessProperty("Key", keyType,"", variableInfo.Children);
                ProcessProperty("Value", valueType,"", variableInfo.Children);

            }
            else if (genericTypeDefinition == typeof(List<>))
            {
                Type itemType = genericArgs[0];
                ProcessProperty("Item", itemType, "", variableInfo.Children);
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
            var ymlVariableDict = ymlVariables.ToDictionary(v => v.Name);

            foreach (var csVariable in csVariables)
            {
                if (ymlVariableDict.TryGetValue(csVariable.Name, out var ymlVariable))
                {
                    if((csVariable.Type.IsGenericType || ymlVariable.Type.IsGenericType) && !csVariable.TypeName.Equals(ymlVariable.TypeName))
                    {
                        ymlVariable.Result = Result.TYPE_MISMATCH;
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
                }
            }

            foreach (var ymlVariable in ymlVariables)
            {
                if (!csVariables.Any(v => v.Name == ymlVariable.Name))
                {
                    ymlVariable.Result = Result.ONLY_IN_YML;
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
                }

            }
            else if (!csVariable.HasChildren() && ymlVariable.HasChildren())
            {
                var ymlLastChild = ymlVariable.Children.Last();
                ymlLastChild.Result = Result.ONLY_IN_YML;
                ymlVariable.Result = Result.ONLY_IN_YML;
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
                        }
                    }
                }
            }
           
        }
        public static UpdateLog MigrateVariables(List<VariableInfo> filteredVariables, string filePath)
        {
            UpdateLog Log = new UpdateLog();
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
                mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            }

            foreach (var variable in filteredVariables)
            {
                if (variable.TypeName.Equals(typeof(Dictionary<,>).Name))
                {
                    mapping.Add(new YamlScalarNode(variable.Name), new YamlMappingNode());
                }
               else if (variable.Type.Equals(typeof(List<>).Name))
                {
                    mapping.Add(new YamlScalarNode(variable.Name), new YamlSequenceNode());
               }
               else
                {
                    mapping.Children[variable.Name] = new YamlScalarNode(variable.Value.ToString());
                }

                Log.AddMessage($"{variable.Name} ({variable.TypeName}) added to config file\n");
            }

            using (var writer = new StreamWriter(filePath))
            {
                yaml.Save(writer, assignAnchors: false);
            }

            return Log;
        }
    }
}
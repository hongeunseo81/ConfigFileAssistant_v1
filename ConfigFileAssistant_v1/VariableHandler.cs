using ConfigFileAssistant.Manager;
using ConfigTypeFinder;
using CoPick.Setting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WheelHetergeneousInspectionSystem.Models;
using YamlDotNet.RepresentationModel;

namespace ConfigFileAssistant
{
    public class VariableHandler
    {
        private Object Instance;
        private YamlMappingNode Root;
        private Object Config;
        public List<ConfigVariable> CsVariables;
        public List<ConfigVariable> YmlVariables;
        private ConfigValidator _configValidator;
        private Dictionary<String, ConfigVariable> ErrorVariables = new Dictionary<string, ConfigVariable>();

        public VariableHandler()
        {
            Config = FileHandler.Config;
            _configValidator = new ConfigValidator();
            TypeManager.Init();
        }
        
        public void ExtractYmlVariables()
        {
            Root = FileHandler.Root;
            YmlVariables = new List<ConfigVariable>();
            ExtractYmlVariablesRecursive(Root, YmlVariables, "");
        }
        private void ExtractYmlVariablesRecursive(YamlNode node, List<ConfigVariable> variables, string prefix)
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

        public void ExtractCsVariables()
        {
            CsVariables = new List<ConfigVariable>();
            var type = Config.GetType();
            Instance = Activator.CreateInstance(type);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object defaultValue = GetDefaultValue(property, Instance);
                ExtractCsVariablesRecursive(property.Name, property.PropertyType, defaultValue, CsVariables);
            }
        }
        private void ExtractCsVariablesRecursive(string propertyName, Type propertyType, Object value, List<ConfigVariable> variables)
        {
            ConfigVariable ConfigVariable;
            if (!propertyType.IsGenericType)
            {
                ConfigVariable = new ConfigVariable(propertyName, propertyType, value);
                if (propertyType.IsEnum)
                {
                    ConfigVariable.SetEnumValues(ExtractEnumValues(ConfigVariable, propertyType));
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

        private List<ConfigVariable> ExtractEnumValues(ConfigVariable ConfigVariable, Type propertyType)
        {
            List<ConfigVariable> enumValues = new List<ConfigVariable>();
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
                    TypeManager.MakeFunction(enumField.Name, validatorType, constructorArguments[1].ToString());
                    enumValues.Add(new ConfigVariable(enumField.Name, validatorType.ToString(), ""));
                }
            }
            return enumValues;
        }
        private object GetDefaultValue(PropertyInfo property, object instance)
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
        public List<ConfigVariable> GetCompareResult()
        {
            var result = _configValidator.CompareVariables(YmlVariables, CsVariables);
            ErrorVariables = _configValidator.ErrorVariables;
            return result;
        }
        public List<ConfigVariable> GetParentVariables(string fullName)
        {
            var names = fullName.Split('.');
            int depth = names.Length;

            ConfigVariable parent = null;
            var currentList = YmlVariables;

            for (int i = 0; i < depth - 1; i++)
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
        public bool InsertChildToVariable(ConfigVariable variable, ConfigVariable child)
        {
            var parentVariables = GetParentVariables(variable.FullName);
            var target = parentVariables.Find(v => v.Name == variable.Name);
            if (target == null)
            {
                return false;
            }

            Dictionary<string, ConfigVariable> parentVariableDict = target.Children.ToDictionary(v => v.Name);
            if (target.Type == typeof(List<>))
            {
                child.Name = $"[{target.Children.Count}]";
            }
            if (parentVariableDict.ContainsKey(child.Name))
            {
                return false;
            }
            child.FullName = $"{variable.FullName}.{child.Name}";
            target.Children.Add(child);
            return true;
        }
        public bool InsertVariable(ConfigVariable variable, ConfigVariable newVariable)
        {
            if (variable == null)
            {
                Dictionary<string, ConfigVariable> csVariablesDict = CsVariables.ToDictionary(v => v.Name);
                if (csVariablesDict.ContainsKey(newVariable.Name))
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
                if (parentVariableDict.ContainsKey(newVariable.Name))
                {
                    return false;
                }
                parentVariables.Insert(selectedIndex + 1, newVariable);
                return true;
            }
        }
        public bool AddVariable(ConfigVariable ConfigVariable)
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
        public bool RemoveVariable(ConfigVariable ConfigVariable)
        {
            var parentVariables = GetParentVariables(ConfigVariable.FullName);

            var target = parentVariables.Find(v => v.Name == ConfigVariable.Name);
            if (target != null)
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
        public bool ModifyChildFromVariable(ConfigVariable ConfigVariable)
        {
            var parentVariables = GetParentVariables(ConfigVariable.FullName);

            var target = parentVariables.Find(v => v.Name == ConfigVariable.Name);
            if (target != null)
            {
                if (target.HasChildren())
                {
                    target.Children.Clear();
                }
                target.Value = target.DefaultValue == null? string.Empty : target.DefaultValue;
                target.Result = Result.Ok;
                return true;
            }
            return false;
        }
        public string UpdateChild(string fullName, object value)
        {
            var parentVariables = GetParentVariables(fullName);
            var names = fullName.Split('.');
            var target = parentVariables.Find(v => v.Name == names.Last());
            var resultMessage = string.Empty;
            if (target != null)
            {
                resultMessage = TypeManager.IsValidateType(target, value.ToString());
                if (resultMessage == string.Empty)
                {
                    target.Value = value;
                    if (ErrorVariables.ContainsKey(target.FullName))
                    {
                        ErrorVariables.Remove(target.FullName);
                    }
                }
            }
            else
            {
                resultMessage = $"{fullName} Variable not found.";
            }

            return resultMessage;
        }

        public void RemoveError(string error)
        {
            ErrorVariables.Remove(error);
        }
        public Dictionary<string, ConfigVariable> GetErrors()
        {
            return ErrorVariables;
        }
        
        public YamlMappingNode ConvertYamlFromCode()
        {
            YamlMappingNode root = new YamlMappingNode();
            foreach (var variable in YmlVariables)
            {
                AddVariableToYamlNode(root, variable);
            }
            return root;
        }

        private void AddVariableToYamlNode(YamlNode parentNode, ConfigVariable variable)
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

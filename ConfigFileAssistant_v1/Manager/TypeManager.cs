using ConfigFileAssistant_v1;
using CoPick.Setting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace ConfigTypeFinder
{
    public class TypeManager
    {
        private static Dictionary<string, Func<object, (bool, string)>> validatorFunction;
        private static Dictionary<string, object[]> FunctionArgs;
        private static Dictionary<string, Type> types;

        public static void init()
        {
            validatorFunction = new Dictionary<string, Func<object, (bool, string)>>();
            FunctionArgs = new Dictionary<string, object[]>();
            types = new Dictionary<string, Type>();
            types.Add(typeof(Dictionary<,>).Name, typeof(Dictionary<,>));
            types.Add(typeof(List<>).Name, typeof(List<>));
            types.Add(typeof(string).Name, typeof(string));
            types.Add(typeof(bool).Name, typeof(bool));
        }

        public static Dictionary<string, Type> GetAllTypes()
        {
            return types;
        }
        public static void AddType(string typeName, Type type)
        {
            if (!types.ContainsKey(typeName))
            {
                types.Add(typeName, type);
            }
        }
        public static void MakeFunction(string name, ValidatorType validatorType, string info)
        {
            var infoArray = info.Split(' ');
            var size = infoArray[1].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            if (int.TryParse(size[1], out int objectSize))
            {
                object[] args = new object[objectSize];
                string valuesString = info.Substring(info.IndexOf('{') + 1).Trim(' ', '}');
                string[] valueStrings = valuesString.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .ToArray();


                for (int i = 0; i < objectSize; i++)
                {
                    string typeString = valueStrings[i * 2];
                    string valueString = valueStrings[i * 2 + 1];
                    if (typeString.Contains("Int32") && int.TryParse(valueString, out int intValue))
                    {
                        args[i] = intValue;
                    }
                    else if (typeString.Contains("Single") && float.TryParse(valueString, out float floatValue))
                    {
                        args[i] = floatValue;
                    }
                    else if (typeString.Contains("Double") && double.TryParse(valueString, out double doubleValue))
                    {
                        args[i] = doubleValue;
                    }

                }
                if (TypeValidator.ValidationDict.TryGetValue(validatorType, out var validatorFunc))
                {
                    if (!validatorFunction.ContainsKey(name))
                    {
                        validatorFunction.Add(name, validatorFunc(args));
                        if (args.Count() > 0)
                        {
                            FunctionArgs.Add(name, args);
                        }
                    }
                }
            }
            AddType(validatorType.ToString(), typeof(string));
        }
        public static string IsValidateType(VariableInfo variableInfo, string value)
        {
            if (value == "")
            {
                return string.Empty;
            }
            if (variableInfo.Type == typeof(bool))
            {
                return (value == "True" || value == "False") ? string.Empty : "Please enter True or False.";
            }
            if (validatorFunction.TryGetValue(variableInfo.Name, out var validatorFunc))
            {
                var isValidate = validatorFunc(value).Item1;
                var message = validatorFunc(value).Item2;
                if (!isValidate && FunctionArgs.ContainsKey(variableInfo.Name))
                {
                    object[] args = FunctionArgs[variableInfo.Name];
                    message = $"Please enter a value between {args[0]} and {args[1]}.";
                }
                return message;
            }
            else
            {
                bool isValid = false;
                string message = string.Empty;
                if (variableInfo.Type == typeof(Int32))
                {
                    isValid = Int32.TryParse(value.ToString(), out int intValue);
                    message = isValid ? string.Empty : "Please enter a value between -2,147,483,648 and 2,147,483,647";
                }
                else if (variableInfo.Type == typeof(Int64))
                {
                    isValid = Int64.TryParse(value.ToString(), out long longValue);
                    message = isValid ? string.Empty : "Please enter a value between -9,223,372,036,854,775,808 and 9,223,372,036,854,775,807";
                }
                else if (variableInfo.Type == typeof(DateTime))
                {
                    isValid = DateTime.TryParse(value.ToString(), out DateTime dateTimeValue);
                    message = isValid ? string.Empty : "Please enter a format yyyy-MM-ddTHH:mm:ss.fffffffK";
                }

                return message;
            }

        }

        public static void ConvertTypeNameToType(VariableInfo newVariable)
        {
            var typeName = newVariable.TypeName;
            newVariable.Type = types[typeName];
        }
    }

}
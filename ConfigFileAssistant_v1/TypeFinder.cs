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
    public class TypeFinder
    {
        private static Dictionary<string, Func<object, (bool, string)>> validatorFunction = new Dictionary<string, Func<object, (bool, string)>>();
        private static Dictionary<string, object[]> FunctionArgs = new Dictionary<string, object[]>();
        
        public static void MakeFunction(string name,ValidatorType validatorType, string info)
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
                    if(!validatorFunction.ContainsKey(name))
                    {
                        validatorFunction.Add(name, validatorFunc(args));
                        if(args.Count() > 0)
                        {
                            FunctionArgs.Add(name, args);
                        }
                    }
                }
            }
        }
        public static string IsValidateType(VariableInfo variableInfo, string value)
        {
            if (value == "")
            {
                return string.Empty;
            }
            if (variableInfo.Type == typeof(bool))
            {
                return (value == "True" || value == "False") ? string.Empty : "True 또는 False를 입력하세요.";
            }
            if (validatorFunction.TryGetValue(variableInfo.Name, out var validatorFunc))
            {
                var isValidate = validatorFunc(value).Item1;
                var message = validatorFunc(value).Item2;
                if (!isValidate && FunctionArgs.ContainsKey(variableInfo.Name))
                {
                    object[] args = FunctionArgs[variableInfo.Name];
                    message = $"{args[0]} ~ {args[1]} 값을 넣어주세요.";
                }
                return message;
            }
            return string.Empty;
        }
    }

}

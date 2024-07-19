using ConfigFileAssistant_v1;
using CoPick.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigTypeFinder
{
    public class TypeFinder
    {
        public static Func<object, (bool, string)> MakeFunction(ValidatorType validatorType, string info)
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
                    return validatorFunc(args);
                }
                return null;
            }
            return null;
        }
        public static bool IsValidateType(Dictionary<string, Func<object, (bool, string)>> validatorFunctionArgs,string name, Type type,string value)
        {
            if (value == "")
            {
                return true;
            }
            if(type == typeof(bool))
            {
                return (value == "True" || value == "False") ? true : false;
            }
            if (validatorFunctionArgs.TryGetValue(name, out var validatorFunc))
            {
                return validatorFunc(value).Item1;
            }
            return true;
        }
    }

}

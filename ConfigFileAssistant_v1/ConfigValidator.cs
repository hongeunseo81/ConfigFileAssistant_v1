using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
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
        public string Type { get; set; }
        public object Value { get; set; }

        public VariableInfo(string type, object value)
        {
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
        public static Dictionary<string, VariableInfo> ExtractVariables(string contents)
        {
            var variables = new Dictionary<string, VariableInfo>();

            var yamlStream = new YamlStream();
            using (var reader = new StringReader(contents))
            {
                yamlStream.Load(reader);
            }

            var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            foreach (var entry in root.Children)
            {
                var key = ((YamlScalarNode)entry.Key).Value;
                var valueNode = entry.Value;

                if (valueNode is YamlMappingNode)
                {
                    variables[key] = new VariableInfo("Dictionary", new YamlMappingNode());
                }
                else if (valueNode is YamlSequenceNode)
                {
                    variables[key] = new VariableInfo("List", new YamlSequenceNode());
                }
                else
                {
                    var value = ConvertValue(valueNode);
                    variables[key] = new VariableInfo(value.GetType().Name.ToString(), value);
                }
            }


            return variables;
        }

        static object ConvertValue(YamlNode valueNode)
        {
            if (valueNode is YamlScalarNode scalarNode)
            {
                return GetValueType(scalarNode.Value);
            }

            return null;
        }

        static object GetValueType(string value)
        {
            if (bool.TryParse(value, out bool boolResult))
            {
                return boolResult;
            }
            if (int.TryParse(value, out int intResult))
            {
                return intResult;
            }
            if (double.TryParse(value, out double doubleResult))
            {
                return doubleResult;
            }
            return value;
        }

        public static List<Variable> CombineVariables(Dictionary<string, VariableInfo> csVariables, Dictionary<string, VariableInfo> ymlVariables) 
        {
            List<Variable> combinedVariables = new List<Variable>();
            foreach (var item in csVariables)
            {
                var variable = new Variable(item.Key, NoteMessage.CS_ONLY);
                variable.CsType = item.Value.Type;
                combinedVariables.Add(variable);
            }

            foreach (var item in ymlVariables)
            {
                var variableInfo = item.Value as VariableInfo;
                Variable foundVariable = combinedVariables.FirstOrDefault(v => v.Name == item.Key);
                if(foundVariable != null)
                {
                    foundVariable.YmlType = variableInfo.Type;
                    if (!foundVariable.YmlType.Equals(foundVariable.CsType))
                    {
                        foundVariable.Note = NoteMessage.TYPE_MISMATCH;
                    }
                    else
                    {
                        foundVariable.Note = NoteMessage.OK;
                    }
                }
                else
                {
                    var variable = new Variable(item.Key, NoteMessage.YML_ONLY);
                    variable.YmlType = item.Value.Type;
                    combinedVariables.Add(variable);
                }
              
            }
            return combinedVariables;
        }

        public static bool isValidatedVariable(List<Variable> variables)
        {
            foreach(Variable v in variables)
            {
                if(v.Note != NoteMessage.OK) return false;  
            }
            return true;
        }
    }
}

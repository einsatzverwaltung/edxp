using EdxpClient.edxp;
using EdxpClient.tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EdxpClient
{
    class Helper
    {
        public static Dictionary<EmergencyObjectHeaderDataType, Type> registeredDataTypes = new Dictionary<EmergencyObjectHeaderDataType, Type>()
        {
            [EmergencyObjectHeaderDataType.Einsatz] = typeof(Einsatz),
            [EmergencyObjectHeaderDataType.Einsatzmittel] = typeof(Einsatzmittel)
        };


        public static void DeserializeEmergencyObjectBody(EmergencyObject obj)
        {
            if(obj.Data is JObject)
            {
                if(obj.Header.DataType.HasValue)
                {
                    var typ = registeredDataTypes[obj.Header.DataType.Value];
                    obj.Data = ((JObject)obj.Data).ToObject(typ);

                }
            }
        }

        public static void DumpObjectToOutput(object obj, OutputFormatType formatType)
        {



            if (formatType == OutputFormatType.User)
            {
                var dump = ObjectDumper.Dump(obj, 4);
                Console.Out.Write(dump);
            }
            else if (formatType == OutputFormatType.JSON)
            {
                var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                Console.Out.Write(json);
            }
            else if (formatType == OutputFormatType.YAML)
            {
                var yaml = new YamlDotNet.Serialization.Serializer();
                var json = yaml.Serialize(obj);
                Console.Out.Write(json);
            }
        }

        public static int RunHttpCall(Func<Task> act)
        {
            try
            {     
                Task.Run(act).Wait();
                return 0;
            }
            catch(AggregateException aggEx)
            {
                foreach(var ex in aggEx.InnerExceptions)
                {
                    Console.Error.WriteLine(ex.Message);
                }

                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }


        }
    }
}

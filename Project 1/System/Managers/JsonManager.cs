using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Project_1.System.Managers
{
    internal static class JsonManager
    {
        public enum Schemas
        {
            //TEMP
            //Reason: To allow the game to load before Schemas have been properly implemented.
            //Remove Con: All schemas being fully implemented
            None,
            //ENDOFTEMP 
            Player,
            GuildMember,
            Npc,
            CameraSettings,
            Count
            //TODO: Fill this
            //TODO: Write the schema files

        }

        static JSchema[] schemas;
        //static Dictionary<string, JSchema> schemas;

        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        static JsonSerializer jsonSerializer;
        static SchemaValidationEventHandler schemaValidationEventHandler;
        static JsonErrorLog jsonErrorLog;

        public static void Init()
        {
            jsonSerializer = JsonSerializer.Create(serializerSettings);
            schemaValidationEventHandler = (object sender, SchemaValidationEventArgs e) =>
            {
                jsonErrorLog.errors.Add(e.Message);
            };

            LoadSchemas();
        }

        static void LoadSchemas()
        {
            schemas = new JSchema[(int)Schemas.Count - 1]; //TEMP: -1 to not try to load none, see enum Schema

            string baseFolder = Game1.ContentManager.RootDirectory + "\\JSchemas";

            for (int i = 0; i < schemas.Length - 1; i++)//TEMP: -1 to not try to load none, see enum Schema
            {

                Schemas schemaType = (Schemas)(i + 1);
                string path = Path.Combine(baseFolder, schemaType + ".Jschema");

                string json = File.ReadAllText(path);//TEMP: +1 to not try to load none, see enum Schema
                schemas[i] = JSchema.Parse(json);
            }
        }

        public static void ExportData(string aDestination, object aObjectToExport)
        {
            
            string json = JsonConvert.SerializeObject(aObjectToExport, serializerSettings);
            File.WriteAllText(aDestination, json);
        }

        public static T ImportJsonFile<T>(string aJsonFilePath, Schemas aSchema)
        {
            if (!File.Exists(aJsonFilePath)) throw new FileNotFoundException($"The file at {aJsonFilePath} could not be found.");
            string json = File.ReadAllText(aJsonFilePath);

            JObject jObj = JObject.Parse(json);
            if (aSchema != Schemas.None) //TEMP: See enum Schema
            {
                jObj.Validate(schemas[(int)aSchema - 1], schemaValidationEventHandler);
                
                if (jsonErrorLog.ErrorFound)
                {
                    throw jsonErrorLog.Exception;
                }
            }
            
            return jsonSerializer.Deserialize<>
        }



        public static string TrimToNameOnly(string aFile)
        {
            string fileOnly = Path.GetFileName(aFile);
            return Path.GetFileNameWithoutExtension(fileOnly);
        }

        private struct JsonErrorLog
        {
            public bool ErrorFound => errors.Count > 0;

            public List<string> errors = [];

            public JsonException Exception
            {
                get
                {
                    JsonException exception = new JsonException(string.Join("\n", errors));
                    errors.Clear();
                    return exception;
                }
            }

            public JsonErrorLog()
            {
            }
        }
    }
}

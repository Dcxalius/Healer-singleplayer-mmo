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

        public static void Init()
        {
            jsonSerializer = JsonSerializer.Create(serializerSettings);
            LoadSchemas();
        }

        static void LoadSchemas()
        {
            schemas = new JSchema[(int)Schemas.Count - 1]; //TEMP: -1 to not try to load none, see enum Schema

            string baseFolder = Game1.ContentManager.RootDirectory + "\\JSchemas";

            for (int i = 0; i < (int)Schemas.Count - 1; i++)//TEMP: -1 to not try to load none, see enum Schema
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
            ValidatedJson json = ValidatedJson.FromFile(aJsonFilePath);
            json.Validate(aSchema);
            return json.ToObject<T>();
        }
        public static string TrimToNameOnly(string aFile) => Path.GetFileNameWithoutExtension(Path.GetFileName(aFile));


        private sealed class ValidatedJson
        {
            private readonly JObject jObject;
            private readonly List<string> errors = new();

            public bool ErrorFound => errors.Count > 0;

            public JsonException Exception
            {
                get
                {
                    JsonException exception = new JsonException(
                        "JSON validation failed:\n" +
                        string.Join("\n", errors));

                    errors.Clear();
                    return exception;
                }
            }

            private ValidatedJson(JObject aJObject)
            {
                jObject = aJObject;
            }

            public static ValidatedJson FromFile(string aJsonFilePath)
            {
                if (!File.Exists(aJsonFilePath))
                {
                    throw new FileNotFoundException(
                        $"The file at {aJsonFilePath} could not be found.");
                }

                string json = File.ReadAllText(aJsonFilePath);
                JObject jObj = JObject.Parse(json);

                return new ValidatedJson(jObj);
            }

            public void Validate(Schemas aSchema)
            {
                //TEMP: See Enum schema
                if (aSchema == Schemas.None) 
                {
                    return;
                }
                //

                errors.Clear();

                JSchema schema = schemas[(int)aSchema - 1];
                jObject.Validate(schema, OnValidationError);

                if (ErrorFound)
                {
                    throw Exception;
                }
            }

            public T ToObject<T>()
            {
                return jObject.ToObject<T>(jsonSerializer);
            }

            private void OnValidationError(object sender, SchemaValidationEventArgs e)
            {
                errors.Add(e.Message);
            }
        }
    }
}

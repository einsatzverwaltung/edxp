using EdxpClient.edxp;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EdxpClient
{
    [Command(Name = "object", Description = "EDXP Objects")]
    [Subcommand(
        typeof(GetObjectCommand),
        typeof(DeleteObjectCommand),
        typeof(EditObjectCommand)
       )]
    class ObjectCommand : CommandBase
    {
        public int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }

    [Command(Name = "delete", Description = "Delete EDXP Objects")]
    class DeleteObjectCommand : CommandBase
    {
        [Argument(0, Description = "UID of the Object", Name = "Object UID")]
        public string ObjectId { get; set; }

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            var c = ClientFactory.Create(Verbose);
            Guid uuid;
            if (Guid.TryParse(ObjectId, out uuid))
            {
                return Helper.RunHttpCall(async () =>
                {
                    await c.DeleteObjectAsync(uuid);

                    Console.Out.WriteLine("Object with ID " + uuid.ToString() + " deleted!");
                });
            }
            else
            {
                app.ShowHelp();
                return -1;
            }
        }
    }

    [Command(Name = "get", Description = "Retrieve EDXP Objects")]
    class GetObjectCommand : CommandBase
    {
        [Argument(0, Description = "UID of the Object", Name = "Object UID")]
        public string ObjectId { get; set; }

        [Argument(1, Description = "Subpath of the Objecttree", Name = "Subpath")]
        public string Subpath { get; set; }

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            var c = ClientFactory.Create(Verbose);
            Guid uuid;
            if (Guid.TryParse(ObjectId, out uuid))
            {
                return Helper.RunHttpCall(async () =>
                {
                    if (string.IsNullOrEmpty(Subpath))
                    {
                        var res = await c.GetObjectAsync(uuid);
                        Helper.DeserializeEmergencyObjectBody(res);
                        Helper.DumpObjectToOutput(res, OutputFormat);
                    }
                    else
                    {
                        var res = await c.GetObjectPartAsync(uuid, Subpath);                        
                        Helper.DumpObjectToOutput(res, OutputFormat);
                    }
                });
            }
            else
            {
                app.ShowHelp();
                return -1;
            }
        }
    }

    [Command(Name = "edit", Description = "Edits an EDXP Objects")]
    class EditObjectCommand : CommandBase
    {
        [Argument(0, Description = "UID of the Object", Name = "Object UID")]
        public string ObjectId { get; set; }

        [Argument(1, Description = "Subpath of the Objecttree", Name = "Subpath")]
        public string Subpath { get; set; }

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            var c = ClientFactory.Create(Verbose);
            Guid uuid;
            if (Guid.TryParse(ObjectId, out uuid))
            {
                return Helper.RunHttpCall(async () =>
                {
                    if (string.IsNullOrEmpty(Subpath))
                    {
                        var res = await c.GetObjectAsync(uuid);

                        var file = System.IO.Path.GetTempPath() + res.Uid.ToString() + ".json";

                        var json = JsonConvert.SerializeObject(res, Formatting.Indented);
                        File.WriteAllText(file, json);

                        ProcessStartInfo pi = new ProcessStartInfo("notepad.exe");
                        pi.Arguments = Path.GetFileName(file);
                        pi.UseShellExecute = true;
                        pi.WorkingDirectory = Path.GetDirectoryName(file);
                        Process.Start(pi).WaitForExit();

                        var newJson = File.ReadAllText(file);
                        if (newJson != json)
                        {
                            var edxo = JsonConvert.DeserializeObject<EmergencyObject>(newJson);
                            try
                            {
                                var resultObject = await c.UpdateObjectAsync(res.Uid.Value, edxo);
                                Console.Error.WriteLine("Object has been modified!");
                                Helper.DeserializeEmergencyObjectBody(resultObject);
                                Helper.DumpObjectToOutput(resultObject, OutputFormat);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("File not modified!");
                        }
                        File.Delete(file);
                    }
                    else
                    {
                        Subpath = Subpath.Replace('/', '.');
                        Subpath = Subpath.Replace('.', '/');

                        var res = await c.GetObjectPartAsync(uuid, Subpath);

                        var file = System.IO.Path.GetTempPath() + uuid.ToString() + "-part.json";

                        var json = JsonConvert.SerializeObject(res, Formatting.Indented);
                        File.WriteAllText(file, json);

                        ProcessStartInfo pi = new ProcessStartInfo("notepad.exe");
                        pi.Arguments = Path.GetFileName(file);
                        pi.UseShellExecute = true;
                        pi.WorkingDirectory = Path.GetDirectoryName(file);
                        Process.Start(pi).WaitForExit();

                        var newJson = File.ReadAllText(file);
                        if (newJson != json)
                        {
                            var edxo = JsonConvert.DeserializeObject(newJson);
                            try
                            {
                                var resultObject = await c.UpdatePartialObjectAsync(uuid, Subpath, edxo);
                                Console.Error.WriteLine("Object has been modified!");
                                Helper.DeserializeEmergencyObjectBody(resultObject);
                                Helper.DumpObjectToOutput(resultObject, OutputFormat);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("File not modified!");
                        }
                        File.Delete(file);
                    }
                });
            }
            else
            {
                app.ShowHelp();
                return -1;
            }
        }
    }
}

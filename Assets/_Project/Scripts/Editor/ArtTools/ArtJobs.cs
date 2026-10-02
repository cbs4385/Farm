#if FARM_MCP
using System.IO;
using System.Text;
using MCPForUnity.Editor.Tools.AssetGen;
using Newtonsoft.Json.Linq;

namespace Farm.Editor.ArtTools
{
    // Dev-only helper (compiled only when the MCP for Unity package is installed): submits the sprite-sheet jobs listed in
    // tools/art/manifest.json to the image provider and reports their state. Used by the agent through execute_code.
    public static class ArtJobs
    {
        const string JobsFile = "Builds/art_jobs.json";
        const string OutFolder = "Assets/_Project/Art/Generated";

        public static string Submit(string onlyIds)
        {
            var only = string.IsNullOrEmpty(onlyIds) ? null : new System.Collections.Generic.HashSet<string>(onlyIds.Split(','));
            var arr = JArray.Parse(File.ReadAllText("tools/art/manifest.json"));
            var jobs = File.Exists(JobsFile) ? JObject.Parse(File.ReadAllText(JobsFile)) : new JObject();
            var log = new StringBuilder();
            foreach (var s in arr)
            {
                var id = (string)s["id"];
                if (only != null && !only.Contains(id)) continue;
                if (File.Exists($"{OutFolder}/{id}.png") && only == null) continue;
                var p = new JObject
                {
                    ["action"] = "generate", ["provider"] = "openrouter", ["mode"] = "text", ["prompt"] = (string)s["prompt"],
                    ["name"] = id, ["outputFolder"] = OutFolder, ["width"] = 1024, ["height"] = 1024, ["transparent"] = false, ["asSprite"] = false,
                };
                var jr = JObject.FromObject(GenerateImage.HandleCommand(p));
                var jid = (string)jr.SelectToken("data.job_id");
                log.AppendLine(id + " -> " + (jid ?? jr.ToString(Newtonsoft.Json.Formatting.None)));
                if (jid != null) jobs[id] = jid;
            }
            Directory.CreateDirectory("Builds");
            File.WriteAllText(JobsFile, jobs.ToString());
            return log.ToString();
        }

        public static string Status()
        {
            if (!File.Exists(JobsFile)) return "no jobs";
            var jobs = JObject.Parse(File.ReadAllText(JobsFile));
            var log = new StringBuilder();
            foreach (var kv in jobs)
            {
                var r = JObject.FromObject(GenerateImage.HandleCommand(new JObject { ["action"] = "status", ["job_id"] = (string)kv.Value }));
                log.AppendLine($"{kv.Key}: {(string)r.SelectToken("data.state")} {(string)r.SelectToken("data.progress")} {(string)r.SelectToken("message")}");
            }
            return log.ToString();
        }
    }
}
#endif

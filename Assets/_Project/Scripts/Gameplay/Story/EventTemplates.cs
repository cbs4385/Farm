using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Farm.Gameplay
{
    // Reusable scene patterns (T-103). A template is a scene written once with ${placeholders}; an instance fills them in:
    //
    //   "eventTemplates": [ { "id": "heirloom", "params": ["npc", "map", "item"], "defaults": { "when": "true" },
    //                         "event": { "trigger": "map", "map": "${map}", "steps": [ ... "${item}" ... ] } } ]
    //   "eventsFromTemplates": [ { "id": "wren_heart8", "template": "heirloom", "args": { "npc": "wren", ... },
    //                              "event": { "priority": 7, "titleKey": "memory.wren_heart8" } } ]
    //
    // `${id}` is always the instance's id. A placeholder that is the whole of a string takes the argument as it is (a number,
    // a list of effects ...); inside longer text it is replaced as text. Every parameter must be given or have a default,
    // arguments the template does not declare are errors (they are almost always typos), and nothing may be left unresolved.
    // The result is an ordinary event, validated and played like any other.
    public static class EventTemplates
    {
        static readonly Regex Placeholder = new Regex(@"\$\{([A-Za-z0-9_]+)\}", RegexOptions.Compiled);

        public static bool TryExpand(JObject template, JObject instance, out JObject expanded, out string error)
        {
            expanded = null;
            error = null;
            var id = (string)instance["id"];
            var templateId = (string)template["id"];
            if (string.IsNullOrEmpty(id)) { error = "an event from a template has no id"; return false; }

            var declared = (template["params"] as JArray)?.Select(t => (string)t).ToList() ?? new List<string>();
            var values = new Dictionary<string, JToken> { ["id"] = new JValue(id) };
            if (template["defaults"] is JObject defaults)
                foreach (var p in defaults.Properties()) values[p.Name] = p.Value.DeepClone();
            if (instance["args"] is JObject args)
                foreach (var p in args.Properties())
                {
                    if (!declared.Contains(p.Name) && !(template["defaults"] is JObject d && d.ContainsKey(p.Name)))
                    {
                        error = $"event '{id}': the template '{templateId}' has no parameter '{p.Name}' (it has: {string.Join(", ", declared)})";
                        return false;
                    }
                    values[p.Name] = p.Value.DeepClone();
                }
            var missing = declared.Where(p => !values.ContainsKey(p)).ToList();
            if (missing.Count > 0)
            {
                error = $"event '{id}': the template '{templateId}' needs {string.Join(", ", missing)}";
                return false;
            }

            var body = template["event"] as JObject;
            if (body == null) { error = $"template '{templateId}' has no 'event'"; return false; }
            var ctx = new Context();
            var result = Substitute(body.DeepClone(), values, ctx);
            if (ctx.Problem != null) { error = $"event '{id}' from template '{templateId}': {ctx.Problem}"; return false; }

            var obj = (JObject)result;
            obj["id"] = id;
            if (instance["event"] is JObject overrides)
                foreach (var p in overrides.Properties()) obj[p.Name] = p.Value.DeepClone();
            expanded = obj;
            return true;
        }

        sealed class Context { public string Problem; }

        static JToken Substitute(JToken token, Dictionary<string, JToken> values, Context ctx)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                {
                    var obj = (JObject)token;
                    foreach (var p in obj.Properties().ToList())
                        p.Value = Substitute(p.Value, values, ctx);
                    return obj;
                }
                case JTokenType.Array:
                {
                    var arr = (JArray)token;
                    var items = new List<JToken>();
                    foreach (var item in arr)
                    {
                        var sub = Substitute(item, values, ctx);
                        // A list argument dropped into a list is spliced in: ["flag:x", "${rewards}"] gives the flag and each reward.
                        if (item.Type == JTokenType.String && sub is JArray spliced && Placeholder.IsMatch((string)item) && Placeholder.Match((string)item).Value == (string)item)
                            items.AddRange(spliced);
                        else items.Add(sub);
                    }
                    return new JArray(items);
                }
                case JTokenType.String:
                {
                    var text = (string)token;
                    var whole = Placeholder.Match(text);
                    if (whole.Success && whole.Value == text)
                    {
                        if (values.TryGetValue(whole.Groups[1].Value, out var value)) return value.DeepClone();
                        ctx.Problem = ctx.Problem ?? $"'{text}' is not a parameter";
                        return token;
                    }
                    return new JValue(Placeholder.Replace(text, m =>
                    {
                        if (values.TryGetValue(m.Groups[1].Value, out var value) && value is JValue v) return Convert.ToString(v.Value, System.Globalization.CultureInfo.InvariantCulture);
                        ctx.Problem = ctx.Problem ?? $"'{m.Value}' is not a text parameter";
                        return m.Value;
                    }));
                }
                default:
                    return token;
            }
        }
    }
}

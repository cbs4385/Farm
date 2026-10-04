using System.Collections.Generic;

namespace Farm.Gameplay
{
    // Checks that a scene is physically possible on its map (T-101): everyone is placed on free floor, scripted walks have
    // a path, props and the camera stay on the map, and no two actors share a cell. Pure: it is given the map's WalkGrid,
    // so the same check runs in tests against the real map scenes and in the validator.
    public static class EventStaging
    {
        // Returns a message per problem (empty when the staging is sound). Events without a map are not checked.
        public static List<string> Check(EventDefinition ev, WalkGrid grid)
        {
            var problems = new List<string>();
            if (ev == null || grid == null) return problems;
            var at = new Dictionary<string, (int x, int y)>();           // the last known cell of each actor
            Walk(ev.Steps, "step", grid, at, problems);
            return problems;
        }

        static void Walk(IList<EventStep> steps, string path, WalkGrid grid, Dictionary<string, (int x, int y)> at, List<string> problems)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                var s = steps[i];
                var where = $"{path}{i}";
                switch (s.Type)
                {
                    case "place":
                        Stand(s.Actor, s.X, s.Y, where, grid, at, problems);
                        break;
                    case "move":
                        if (!grid.IsWalkable(s.X, s.Y)) { problems.Add($"{where}: {s.Actor} walks to ({s.X},{s.Y}), which is not free floor"); break; }
                        if (at.TryGetValue(s.Actor ?? string.Empty, out var from) && grid.FindPath(from.x, from.y, s.X, s.Y) == null)
                            problems.Add($"{where}: no path for {s.Actor} from ({from.x},{from.y}) to ({s.X},{s.Y})");
                        Stand(s.Actor, s.X, s.Y, where, grid, at, problems, alreadyChecked: true);
                        break;
                    case "spawn":
                        if (!grid.IsWalkable(s.X, s.Y)) problems.Add($"{where}: the prop '{s.Id}' is put at ({s.X},{s.Y}), which is not free floor");
                        break;
                    case "camera":
                        if ((s.Name == "focus" || s.Name == "pan") && string.IsNullOrEmpty(s.Actor) && !grid.Contains(s.X, s.Y))
                            problems.Add($"{where}: the camera looks at ({s.X},{s.Y}), which is off the map");
                        break;
                    case "parallel":
                        Walk(s.Steps, where + ".", grid, at, problems);
                        break;
                }
            }
        }

        static void Stand(string actor, int x, int y, string where, WalkGrid grid, Dictionary<string, (int x, int y)> at, List<string> problems, bool alreadyChecked = false)
        {
            if (string.IsNullOrEmpty(actor)) return;
            if (!alreadyChecked && !grid.IsWalkable(x, y)) problems.Add($"{where}: {actor} is placed at ({x},{y}), which is not free floor");
            foreach (var other in at)
                if (other.Key != actor && other.Value.x == x && other.Value.y == y)
                    problems.Add($"{where}: {actor} and {other.Key} would stand on the same cell ({x},{y})");
            at[actor] = (x, y);
        }
    }
}

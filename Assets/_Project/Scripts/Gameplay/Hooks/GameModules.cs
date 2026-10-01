using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    public sealed class ModuleContext
    {
        public EventBus Bus;
        public GameSession Session;
        public GameHooks Hooks;
        public GameDatabase Db;
        public SettingsData Settings;

        // 0 = the player turned the horror layer off, 1 = mild, 2 = full. Modules must respect this.
        public int HorrorLevel => Settings != null ? Settings.HorrorLevel : 2;
    }

    // An optional layer of content/behaviour in its own assembly (e.g. Farm.Mythos). Core code never references a
    // module; a module registers itself with GameModules.Register from a [RuntimeInitializeOnLoadMethod]
    // (BeforeSceneLoad) method and is initialized once the persistent services exist.
    public interface IGameModule
    {
        string Id { get; }
        void Initialize(ModuleContext context);
    }

    public static class GameModules
    {
        static readonly List<IGameModule> Modules = new List<IGameModule>();

        public static IReadOnlyList<IGameModule> All => Modules;

        public static void Register(IGameModule module)
        {
            if (module == null) return;
            foreach (var m in Modules)
                if (m.Id == module.Id) return;   // already registered (domain reload may re-run registration)
            Modules.Add(module);
        }

        public static void InitializeAll(ModuleContext context)
        {
            foreach (var m in Modules)
            {
                try { m.Initialize(context); }
                catch (System.Exception e) { Log.Error($"Module '{m.Id}' failed to initialize: {e}"); }
            }
        }

        public static void ClearForTests() => Modules.Clear();
    }
}

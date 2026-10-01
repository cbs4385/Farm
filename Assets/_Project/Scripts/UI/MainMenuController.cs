using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.UI
{
    // MainMenu scene entry point: builds the title menu on the persistent UI canvas.
    public sealed class MainMenuController : MonoBehaviour
    {
        MainMenuScreen _menu;

        void Start()
        {
            var ui = ServiceLocator.Get<UiService>();
            ui.SetHudVisible(false);
            ServiceLocator.Get<GameSession>().EndGame();
            _menu = new MainMenuScreen(ui);
            _menu.Open();
            var open = CommandLine.GetArg("-farmOpen");
            if (open == "newgame") _menu.OpenNewGame();
            else if (open == "options") ui.ShowOptions();
        }

        void OnDestroy()
        {
            _menu?.Close();
        }
    }
}

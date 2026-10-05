namespace Farm.Gameplay
{
    // Something the player can use with the Interact button while facing it.
    public interface IInteractable
    {
        void Interact(PlayerActions player);

        // A short plain-language name for what this is, shown when the mouse is over it (null: nothing to show).
        string HoverLabel => null;
    }
}

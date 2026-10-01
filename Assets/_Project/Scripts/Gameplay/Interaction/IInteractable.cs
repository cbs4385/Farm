namespace Farm.Gameplay
{
    // Something the player can use with the Interact button while facing it.
    public interface IInteractable
    {
        void Interact(PlayerActions player);
    }
}

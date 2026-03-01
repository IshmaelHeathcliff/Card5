namespace Core
{
    public class InputSystem : AbstractSystem
    {
        PlayerInput _input;


        public PlayerInput.PlayerActions PlayerActionMap { get; private set; }
        public PlayerInput.PlayerActions StashActionMap { get; private set; }
        public PlayerInput.PlayerActions PackageActionMap { get; private set; }
        public PlayerInput.PlayerActions EquipmentActionMap { get; private set; }
        public PlayerInput.PlayerActions MenuActionMap { get; private set; }


        protected override void OnInit()
        {
            _input = new PlayerInput();
            PlayerActionMap = _input.Player;
        }
    }
}

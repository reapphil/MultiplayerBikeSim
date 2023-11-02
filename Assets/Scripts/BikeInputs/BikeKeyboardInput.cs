using Enums;

namespace BikeInputs
{
    public class BikeKeyboardInput : BikeGamePadInput
    {
        public override InputMode InputMode => InputMode.KeyboardWASD;
    }
}
using System.Collections.Generic;

namespace SCP294.Types
{
    public enum DrinkSound
    {
        Normal = 0,
        Unstable = 1
    }
    public class DrinkSoundFiles {
        public List<string> List = new List<string>() {
            "294dispense.ogg",
            "294explode.ogg"
        };
    }
}

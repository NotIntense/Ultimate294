using System.Collections.Generic;
using ProjectMER.Features.Objects;
using SCP294.Classes;
using SCP294.Types;

namespace SCP294.Handlers
{
    public class serverHandler
    {
        public void WaitingForPlayers() {
            SCP294.Instance.SpawnedSCP294s = new Dictionary<SchematicObject, bool>();
            SCP294.Instance.PlayersNear294 = new List<string>();
            SCP294.Instance.CustomDrinkItems = new Dictionary<ushort, DrinkInfo>();
            SCP294.Instance.PlayerVoicePitch = new Dictionary<string, float>();
            SCP294.Instance.Encoders = new Dictionary<ReferenceHub, OpusComponent>();
            SCP294Object.SpawnSCP294();
        }
    }
}

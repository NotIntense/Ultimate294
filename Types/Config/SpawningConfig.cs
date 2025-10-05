using System.Collections.Generic;
using Exiled.API.Enums;

namespace SCP294.Types.Config
{
    public class SpawningConfig
    {
        /// <summary>
        /// Amount of SCP-294s that will spawn in the given round
        /// </summary>
        public int SpawnAmount { get; set; } = 1;
        /// <summary>
        /// Dictionary of rooms SCP-294 can spawn in, Key is Exiled RoomType, Value is a List of possible Transforms, Relative to the Room's Rotation
        /// </summary>
        public Dictionary<RoomType, List<SpawnTransform>> SpawnRooms { get; set; } = new Dictionary<RoomType, List<SpawnTransform>>();
    }
}

using Exiled.API.Enums;
using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;

namespace SCP294.Utils
{
    public static class RoomHandler
    {
        public static Room GetRandomRoom(RoomType roomType)
        {
            var roomList = Room.Get(room => room.Type == roomType);
            return roomList.ElementAtOrDefault(UnityEngine.Random.Range(0, roomList.Count()));
        }
        public static Room GetRandomRoom(List<RoomType> roomType)
        {
            var roomList = Room.Get(room => roomType.Contains(room.Type));
            return roomList.ElementAtOrDefault(UnityEngine.Random.Range(0, roomList.Count()));
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoffeeCat;
using UnityEngine;

namespace RandomDungeonWithBluePrint
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class Section
    {
        public int Index;               // Section의 인덱스
        public RectInt Rect;            // Section의 사이즈(x,y,width,height)
        public Room Room;               // Section 안의 Room
        public Relay Relay;             // 길의 연결부위
        public Vector2Int MinRoomSize;  // Section의 최소 사이즈
        public int MakeRoomWeight;      // Room 생성 가중치
        public bool RoomIndispensable;  // Room 생성 필수 여부

        public int Width => Rect.width;
        public int Height => Rect.height;
        public bool ExistRoom => Room != null;

        public Section() { }

        public Section(FieldBluePrint.Section bluePrint)
        {
            Index = bluePrint.Index;
            Rect = bluePrint.Rect;
            MakeRoomWeight = bluePrint.MakeRoomWeight;
            RoomIndispensable = bluePrint.RoomIndispensable;
            MinRoomSize = bluePrint.MinRoomSize;
        }

        public int AdjoiningWithDirection(Section other)
        {
            return Rect.AdjoiningWithDirection(other.Rect);
        }

        public bool AdjoinWith(Section other)
        {
            return AdjoiningWithDirection(other) != Constants.Direction.Error;
        }

        public Vector2Int GetEdge(Section other, Vector2Int initial = default)
        {
            return Rect.GetEdge(AdjoiningWithDirection(other), initial);
        }

        public IEnumerable<Joint> GetUnConnectedJoints(int direction)
        {
            return Room.GetUnconnectedJoints(direction);
        }

        public IEnumerable<Joint> GetConnectedJoints(int direction)
        {
            return Room.GetConnectedJoints(direction);
        }

        public bool ExistUnconnectedJoints(int direction)
        {
            return !ExistRoom || GetUnConnectedJoints(direction).Any();
        }

        public void Dispose() {
            Room?.Dispose();
            Room = null;    
        }
    }
}
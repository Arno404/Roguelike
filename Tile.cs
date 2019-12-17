using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace Roguelike
{   [DataContract]
    [KnownType(typeof(TileFlyweight.Type))]
    [KnownType(typeof(BaseEntity))]
    class Tile
    {   [DataMember]
        public TileFlyweight.Type Type { get; set; }
        [DataMember]
        public BaseEntity Object { get; set; }
        [DataMember]
        public bool Visible { get; set; }

        public Tile(TileFlyweight.Type type, BaseEntity obj = null)
        {
            Type = type;
            Object = obj;
        }
    }
}

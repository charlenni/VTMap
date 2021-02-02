using System.Collections.Generic;
using System.Text;

namespace VTMap.Core
{
    public class TagSet : Dictionary<string, string>
    {
        private const char KeyValueSeparator = '=';

        /// The key of the house number OpenStreetMap tag
        public const string KeyHouseNumber = "addr:housenumber";

        /// The key of the name OpenStreetMap tag
        public const string KeyName = "name";

        /// The key of the reference OpenStreetMap tag
        public const string KeyRef = "ref";

        /// The key of the elevation OpenStreetMap tag
        public const string KeyEle = "ele";

        /// The key of the id tag
        public const string KeyId = "id";

        public const string KeyAmenity = "amenity";
        public const string KeyBuilding = "building";
        public const string KeyHighway = "highway";
        public const string KeyLanduse = "landuse";

        public const string ValueYes = "yes";
        public const string ValueNo = "no";

        // S3DB
        public const string KeyArea = "area";
        public const string KeyBuildingColor = "building:colour";
        public const string KeyBuildingLevels = "building:levels";
        public const string KeyBuildingMaterial = "building:material";
        public const string KeyBuildingMinLevel = "building:min_level";
        public const string KeyBuildingPart = "building:part";
        //public const string KEY_COLOR = "colour";
        public const string KeyHeight = "height";
        //public const string KEY_MATERIAL = "material";
        public const string KeyMinHeight = "min_height";
        public const string KeyRoof = "roof";
        public const string KeyRoofAngle = "roof:angle";
        public const string KeyRoofColor = "roof:colour";
        public const string KeyRoofDirection = "roof:direction";
        public const string KeyRoofHeight = "roof:height";
        public const string KeyRoofLevels = "roof:levels";
        public const string KeyRoofMaterial = "roof:material";
        public const string KeyRoofOrientation = "roof:orientation";
        public const string KeyRoofShape = "roof:shape";
        public const string KeyVolume = "volume";

        // Roof shape values
        public const string ValueDome = "dome";
        public const string ValueFlat = "flat";
        public const string ValueGabled = "gabled";
        public const string ValueGambrel = "gambrel";
        public const string ValueHalfHipped = "half_hipped";
        public const string ValueHipped = "hipped";
        public const string ValueMansard = "mansard";
        public const string ValueOnion = "onion";
        public const string ValuePyramidal = "pyramidal";
        public const string ValueRound = "round";
        public const string ValueSaltbox = "saltbox";
        public const string ValueSkillion = "skillion";

        public const string ValueAcross = "across"; // orientation across

        /// <summary>
        /// Add a key value pair in form of key=value to TagSet
        /// </summary>
        /// <param name="tag">Tag in key=value format</param>
        public void Parse(string tag)
        {
            var splitPosition = tag.IndexOf(KeyValueSeparator);

            if (splitPosition > 0)
                this[tag.Substring(0, splitPosition)] = tag.Substring(splitPosition + 1);
        }

        /// <summary>
        /// Sets the tags from 'tagSet'
        /// </summary>
        /// <param name="tagSet">TagSet to use to fill this TagSet</param>
        public void Set(TagSet tagSet)
        {
            Clear();
            foreach (var pair in tagSet)
                Add(pair.Key, pair.Value);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in this)
                sb.Append($"Tag[{pair.Key},{pair.Value}]");

            return sb.ToString();
        }
    }
}

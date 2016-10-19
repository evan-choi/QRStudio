using System.Linq;

namespace QRStudio.Google
{
    public struct GeoAddress
    {
        public string formatted_address { get; set; }

        public SubAddress[] address_components { get; set; }

        public Geometry geometry { get; set; }
        public bool partial_match { get; set; }
        public string place_id { get; set; }
        public string[] types { get; set; }

        public SubAddress FromType(string type)
        {
            return address_components
                .FirstOrDefault(adr => adr.types
                    .Count(t => string.Compare(t, type, true) == 0) > 0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuazalWV
{
    public class TelemetryTag : DbModel
    {
        public uint TrackingId { get; set; }
        public string Tag { get; set; }
        public string Attributes { get; set; }
        public uint DeltaTime { get; set; }
    }
}

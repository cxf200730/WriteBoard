using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class WritingDeviceInfo
    {
        public string DeviceName { get; set; }
        public ushort NVId { get; set; }
        public object DType { get; set; }
        public ushort NPId { get; set; }
    }
}
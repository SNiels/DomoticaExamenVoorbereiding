using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class WCFNode
    {
        private string _name;

        public string ItemId { get; set; }

        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = ItemId.Split('.').Last();
                }
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public object Value { get; set; }

        public override string ToString()
        {
            return ItemId;
        }
    }
}

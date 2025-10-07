using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.SolicitaPedidos.Models
{
    public class Request
    {
       public Guid userId { get; set; }
        public string currency { get; set; }
        public List<Item> items { get; set; }
    }

    public class Item
    {
        public Guid jogoId { get; set; }
        public string description { get; set; }
        public int unitPrice { get; set; }
        public int quantity { get; set; }
    }


}

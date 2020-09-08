using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaClient
{
    public class Order
    {
        public string Id { get; set; }
        public string Count { get; set; }
        public string DateTime { get; set; }
        public string FilmSession_Id { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class ValidationFault
    {
        [DataMember] public bool jeste {  get; set; }=false;
        [DataMember] public string Poruka { get; set; } = "";
        [DataMember] public string Polje { get; set; } = "";
        [DataMember] public string OcekivanaVrednost { get; set; } = "";

        public ValidationFault()
        {
            this.jeste = false;
            this.Poruka = "";
            this.Polje = "";
            this.OcekivanaVrednost = "";
        }
    }
}

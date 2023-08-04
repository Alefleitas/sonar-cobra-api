using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    public enum PaymentType
    {
        // DEBIN
        Normal = 0,
        Recursive = 1,
        // E-CHEQ ITAU
        ECHEQ_AL_DIA = 3,
        ECHEQ_CPD = 4,
    }
}

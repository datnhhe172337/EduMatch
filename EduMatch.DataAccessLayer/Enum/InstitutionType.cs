using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Enum
{
	 public enum InstitutionType : byte
    {
      
        /// Trung cấp / Vocational
       
        Vocational = 0,

       
        /// Cao đẳng / College
      
        College = 1,

        
        /// Đại học / University
   
        University = 2,

        
        /// Khác / Other types
       
        Other = 3
    }
}

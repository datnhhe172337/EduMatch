using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IPromptService
    {
        string PromptV1();
        string PromptV2();
        string PromptV3();
        string PromptV4();
    }
}

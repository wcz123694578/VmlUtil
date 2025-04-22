using System;
using System.Collections.Generic;
using System.Text;

namespace VmlUtil.Core
{
    public interface IScan
    {
        TokenType Scan();
        void Rollback();
    }
}

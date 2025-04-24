using System;
using System.Collections.Generic;
using System.Text;

namespace VmlUtil.Core
{
    public interface ISerialize<T>
    {
        VmlDocument Serialize(object obj);
        T Deserialize(VmlDocument vml, Type type);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.CreateBook;

public interface IBookEvent
{
    Guid Id {get;}
}

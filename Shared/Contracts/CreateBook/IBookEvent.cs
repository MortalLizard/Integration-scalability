using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.CreateBook;

internal interface IBookEvent
{
    Guid Id {get;}
}

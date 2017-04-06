
using System.Collections.Generic;

namespace AbiFramework.Entities
{
  public class PagedListRequest
  {
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public IDictionary<string, string> Search { get; set; }
    public object Message { get; set; }
  }

  public class PagedListRequest<TMessage> : PagedListRequest
  {
    public new TMessage Message { get; set; }
  }
}
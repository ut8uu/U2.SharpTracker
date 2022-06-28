using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2.SharpTracker.Core;

public class UrlCacheRecord
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public string Content { get; set; }
    public DateTime ValidTill { get; set; }
}

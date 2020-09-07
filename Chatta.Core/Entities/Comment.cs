using System;
using System.Collections.Generic;
using System.Text;

namespace Chatta.Core.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public virtual Post Post { get; set; }
    }
}

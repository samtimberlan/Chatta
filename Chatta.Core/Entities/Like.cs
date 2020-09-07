using System;
using System.Collections.Generic;
using System.Text;

namespace Chatta.Core.Entities
{
    public class Like
    {
        public Guid Id { get; set; }
        public bool IsLikeActive { get; set; }
        public virtual Poster User { get; set; }
        public virtual Post Post { get; set; }
    }
}

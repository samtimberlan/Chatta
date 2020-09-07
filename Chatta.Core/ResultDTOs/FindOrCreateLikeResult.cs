using Chatta.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chatta.Infrastructure.ResultDTOs
{
    public class FindOrCreateLikeResult
    {
        public bool Success { get; set; }
        public Like LikeObject { get; set; }
    }
}

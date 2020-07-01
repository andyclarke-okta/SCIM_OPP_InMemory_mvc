﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Okta.SCIM.Models
{
    public class SCIMResourceQueryResponse
    {
        public int	itemsPerPage {get; set;}
        public int	startIndex { get; set;}
        public int totalResults {get; set;}
        public List<string> schemas { get; set; }

    }
}

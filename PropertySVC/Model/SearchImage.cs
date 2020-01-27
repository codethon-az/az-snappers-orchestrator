using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PropertySVC.Model
{
    public class SearchImageRequest
    {
        public string org_image_url { get; set; }
        public List<string> comp_image_urls { get; set; }

        public SearchImageRequest()
        {
            comp_image_urls = new List<string>();
        }
    }
    public class SearchImageResponse
    {
        public string comp_image_url { get; set; }
        public string message { get; set; }
    }
}

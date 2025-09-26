using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace App.Web.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-href-active")]
    public class ActiveRouteTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public string AspHrefActive { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext?.HttpContext?.Request?.Path.HasValue != true || string.IsNullOrEmpty(AspHrefActive))
                return;

            var current = ViewContext.HttpContext.Request.Path.Value ?? string.Empty;
            if (string.Equals(current.TrimEnd('/'),
                AspHrefActive.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase)
                || (current.StartsWith(AspHrefActive, StringComparison.OrdinalIgnoreCase)
                && AspHrefActive != "/"))
            {
                var cls = output.Attributes.ContainsName("class") ? output.Attributes["class"].Value?.ToString() + " active" : "active";
                output.Attributes.SetAttribute("class", cls);
            }
            output.Attributes.RemoveAll("asp-href-active");
        }
    }
}

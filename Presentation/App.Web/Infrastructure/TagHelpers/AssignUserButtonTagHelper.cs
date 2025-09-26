using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace App.Web.Infrastructure.TagHelpers
{
    [HtmlTargetElement("assign-user-button")]
    public class AssignUserButtonTagHelper : TagHelper
    {
        public string Level { get; set; } // Department / Unit / SubUnit
        public int EntityId { get; set; } // Id of Dept/Unit/SubUnit
        public string ModalId { get; set; } = "assignUserModal";
        public string Text { get; set; } = "+ Assign";
        public string CssClass { get; set; } = "btn btn-sm btn-success ms-2";

        private static bool _scriptAdded = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // الزرار نفسه
            output.TagName = "button";
            output.Attributes.SetAttribute("type", "button");
            output.Attributes.SetAttribute("class", CssClass);
            output.Attributes.SetAttribute("data-bs-toggle", "modal");
            output.Attributes.SetAttribute("data-bs-target", $"#{ModalId}");
            output.Attributes.SetAttribute("data-entity-id", EntityId);
            output.Attributes.SetAttribute("data-level", Level);
            output.Content.SetContent(Text);

            // الجافاسكريبت يضاف مرة واحدة
            if (!_scriptAdded)
            {
                var script = new StringBuilder();
                script.AppendLine("<script>");
                script.AppendLine("document.addEventListener('DOMContentLoaded', function () {");
                script.AppendLine("  var assignModal = document.getElementById('" + ModalId + "');");
                script.AppendLine("  if(assignModal){");
                script.AppendLine("    assignModal.addEventListener('show.bs.modal', function (event) {");
                script.AppendLine("      var button = event.relatedTarget;");
                script.AppendLine("      var entityId = button.getAttribute('data-entity-id');");
                script.AppendLine("      var level = button.getAttribute('data-level');");
                script.AppendLine("      console.log('Modal triggered by:', level, 'with ID:', entityId);");
                script.AppendLine("      // reset all hidden");
                script.AppendLine("      document.getElementById('modalDepartmentId').value = '';");
                script.AppendLine("      document.getElementById('modalUnitId').value = '';");
                script.AppendLine("      document.getElementById('modalSubUnitId').value = '';");
                script.AppendLine("      if (level === 'Department') document.getElementById('modalDepartmentId').value = entityId;");
                script.AppendLine("      if (level === 'Unit') document.getElementById('modalUnitId').value = entityId;");
                script.AppendLine("      if (level === 'SubUnit') document.getElementById('modalSubUnitId').value = entityId;");
                script.AppendLine("    });");
                script.AppendLine("  }");
                script.AppendLine("});");
                script.AppendLine("</script>");

                output.PostElement.AppendHtml(script.ToString());
                _scriptAdded = true;
            }
        }
    }
}

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace App.Web.Infrastructure.TagHelpers
{
    [HtmlTargetElement("modal-button")]
    public class ModalButtonTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-action")]
        public string Action { get; set; }

        [HtmlAttributeName("asp-entity-id")]
        public int EntityId { get; set; }

        [HtmlAttributeName("asp-entity-param-name")]
        public string EntityParamName { get; set; } = "entityId"; // 👈 configurable

        [HtmlAttributeName("asp-entity-type")]
        public string EntityType { get; set; } = "Default";

        [HtmlAttributeName("modal-title")]
        public string ModalTitle { get; set; } = "Modal Title";

        [HtmlAttributeName("button-text")]
        public string ButtonText { get; set; } = "Open Modal";

        [HtmlAttributeName("button-class")]
        public string ButtonClass { get; set; } = "btn btn-primary";

        [HtmlAttributeName("submit-text")]
        public string SubmitText { get; set; } = "Confirm";

        [HtmlAttributeName("modal-type")]
        public string ModalType { get; set; } = "custom"; // upload | delete | confirm | custom

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var modalId = $"{EntityType}_Modal_{EntityId}_{ModalType}";
            var buttonId = $"{EntityType}_Btn_{EntityId}_{ModalType}";

            // الزرار
            output.TagName = "button";
            output.Attributes.SetAttribute("id", buttonId);
            output.Attributes.SetAttribute("type", "button");
            output.Attributes.SetAttribute("class", ButtonClass);
            output.Attributes.SetAttribute("data-bs-toggle", "modal");
            output.Attributes.SetAttribute("data-bs-target", $"#{modalId}");
            output.Attributes.SetAttribute("data-entity-id", EntityId);
            output.Content.SetContent(ButtonText);

            // body حسب النوع
            string GetModalBody()
            {
                return ModalType.ToLower() switch
                {
                    "upload" => @"
                        <div class='mb-3'>
                            <input class='form-control' type='file' name='file' />
                        </div>
                        <div class='dropzone border p-3 text-center'>
                            Drag & Drop file here
                        </div>
                    ",

                    "delete" => $@"<p class='text-danger'>Are you sure you want to delete this {EntityType}?</p>",

                    "confirm" => "<p>Do you want to proceed with this action?</p>",

                    _ => "<p>Custom modal content goes here.</p>"
                };
            }

            var modalHtml = $@"
<div class='modal fade' id='{modalId}' data-bs-backdrop='static' data-bs-keyboard='false' tabindex='-1' aria-labelledby='{modalId}Label' aria-hidden='true'>
  <div class='modal-dialog modal-dialog-centered modal-lg'>
    <div class='modal-content'>
      <form method='post' action='{Action}' enctype='multipart/form-data'>
        <div class='modal-header'>
          <h5 class='modal-title' id='{modalId}Label'>{ModalTitle}</h5>
          <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button>
        </div>
        <div class='modal-body'>
          <input type='hidden' name='{EntityParamName}' value='{{ENTITY_ID}}' />
          <input type='hidden' name='entityType' value='{EntityType}' />
          {GetModalBody()}
        </div>
        <div class='modal-footer'>
          <button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>Close</button>
          <button type='submit' class='btn btn-primary'>{SubmitText}</button>
        </div>
      </form>
    </div>
  </div>
</div>";

            var script = $@"
<script>
document.getElementById('{buttonId}').addEventListener('click', function() {{
    let entityId = this.getAttribute('data-entity-id');
    if (!document.getElementById('{modalId}')) {{
        let modalTemplate = `{modalHtml}`;
        modalTemplate = modalTemplate.replace('{{ENTITY_ID}}', entityId);
        document.body.insertAdjacentHTML('beforeend', modalTemplate);
    }}
    var modal = new bootstrap.Modal(document.getElementById('{modalId}'));
    modal.show();
}});
</script>";

            output.PostElement.AppendHtml(script);
        }
    }
}

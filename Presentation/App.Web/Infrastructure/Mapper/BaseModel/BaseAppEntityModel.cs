namespace App.Web.Infrastructure.Mapper.BaseModel
{
    public record class BaseAppEntityModel : BaseAppModel
    {
        public int? Id { get; set; }
    }
}

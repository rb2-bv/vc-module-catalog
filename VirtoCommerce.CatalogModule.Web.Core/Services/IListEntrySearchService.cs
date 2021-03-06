using VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Services
{
    public interface IListEntrySearchService
    {
        ListEntrySearchResult Search(VirtoCommerce.Domain.Catalog.Model.SearchCriteria criteria);
    }
}

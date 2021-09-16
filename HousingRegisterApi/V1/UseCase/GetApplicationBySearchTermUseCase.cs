using HousingRegisterApi.V1.Boundary.Request;
using HousingRegisterApi.V1.Boundary.Response;
using HousingRegisterApi.V1.Factories;
using HousingRegisterApi.V1.Gateways;
using HousingRegisterApi.V1.Infrastructure;
using HousingRegisterApi.V1.UseCase.Interfaces;
using System.Linq;

namespace HousingRegisterApi.V1.UseCase
{
    public class GetApplicationBySearchTermUseCase : IGetApplicationBySearchTermUseCase
    {
        private readonly IApplicationApiGateway _gateway;
        private readonly IPaginationHelper _paginationHelper;

        public GetApplicationBySearchTermUseCase(IApplicationApiGateway gateway, IPaginationHelper paginationHelper)
        {
            _gateway = gateway;
            _paginationHelper = paginationHelper;
        }

        public PaginatedApplicationListResponse Execute(SearchApplicationRequest searchParameters)
        {
            var totalItems = _gateway.GetAllBySearchTerm(searchParameters);

            var updateditems = _paginationHelper.OrderData(totalItems, searchParameters.OrderBy);
            updateditems = _paginationHelper.PageData(updateditems, searchParameters.Page, searchParameters.NumberOfItemsPerPage);

            return _paginationHelper.BuildResponse(searchParameters, updateditems.ToResponse(), totalItems.Count());
        }
    }
}

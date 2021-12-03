using Hackney.Core.Http;
using Hackney.Core.JWT;
using HousingRegisterApi.V1.Domain.Report.Novalet;
using HousingRegisterApi.V1.Gateways;
using HousingRegisterApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HousingRegisterApi.V1.UseCase
{
    public class ApproveNovaletExportUseCase : IApproveNovaletExportUseCase
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHttpContextWrapper _contextWrapper;
        private readonly ITokenFactory _tokenFactory;
        private readonly IFileGateway _fileGateway;

        public ApproveNovaletExportUseCase(
            IHttpContextAccessor contextAccessor,
            IHttpContextWrapper contextWrapper,
            ITokenFactory tokenFactory,
            IFileGateway fileGateway)
        {
            _contextAccessor = contextAccessor;
            _contextWrapper = contextWrapper;
            _tokenFactory = tokenFactory;
            _fileGateway = fileGateway;
        }

        public async Task<bool> Execute(string fileName)
        {
            var attributeDictionary = await _fileGateway.GetAttributes(fileName, "Novalet").ConfigureAwait(false);
            var attributes = attributeDictionary.ToObject();

            var token = _tokenFactory.Create(_contextWrapper.GetContextRequestHeaders(_contextAccessor.HttpContext));

            attributes.ApprovedOn = System.DateTime.UtcNow;
            attributes.ApprovedBy = token?.Name;

            await _fileGateway.UpdateAttributes(fileName, attributes.ToDictionary(), "Novalet").ConfigureAwait(false);

            return true;
        }
    }
}

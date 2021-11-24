using HousingRegisterApi.V1.Domain;
using HousingRegisterApi.V1.Domain.FileExport;
using HousingRegisterApi.V1.Gateways;
using HousingRegisterApi.V1.Services;
using HousingRegisterApi.V1.UseCase.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HousingRegisterApi.V1.UseCase
{
    public class CreateNovaletExportUseCase : ICreateNovaletExportUseCase
    {
        private readonly ILogger<CreateNovaletExportUseCase> _logger;
        private readonly IFileGateway _fileGateway;
        private readonly IApplicationApiGateway _gateway;
        private readonly ICsvService _csvService;

        public CreateNovaletExportUseCase(
            ILogger<CreateNovaletExportUseCase> logger,
            IApplicationApiGateway gateway,
            IFileGateway fileGateway,
            ICsvService csvService)
        {
            _logger = logger;
            _gateway = gateway;
            _fileGateway = fileGateway;
            _csvService = csvService;
        }

        public async Task<ExportFile> Execute()
        {
            var applications = _gateway.GetApplicationsAtStatus(ApplicationStatus.Active);
            string fileName = $"LBH-APPLICANT FEED-{DateTime.UtcNow:ddMMyyyy}.csv";

            if (!applications.Any())
            {
                _logger.LogInformation($"No export file was generated this time");
                return null;
            }

            var exportDataSet = applications.Select(x => new NovaletExportDataRow(x)).ToArray();
            var bytes = await _csvService.Generate(exportDataSet).ConfigureAwait(false);
            var file = new ExportFile(fileName, "text/csv", bytes);

            if (file != null)
            {
                // save file to s3 gateway
                var response = _fileGateway.SaveFile(file).ConfigureAwait(false);
                _logger.LogInformation($"File {file.FileName} was succesfully generated");
                return file;
            }
            else
            {
                _logger.LogInformation($"No export file was generated this time");
                return null;
            }
        }
    }
}
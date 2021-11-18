using HousingRegisterApi.V1.Domain.FileExport;
using System.Threading.Tasks;

namespace HousingRegisterApi.V1.Services
{
    public interface ICSVService
    {
        Task<byte[]> Generate<T>(T source, CsvParameters csvParameters);
    }
}

using Hackney.Core.Http;
using Hackney.Core.JWT;
using HousingRegisterApi.V1.Domain;
using HousingRegisterApi.V1.Factories;
using HousingRegisterApi.V1.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HousingRegisterApi.V1.Gateways
{
    public class ApplicationActivityHistory : IActivityHistory
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHttpContextWrapper _contextWrapper;
        private readonly ITokenFactory _tokenFactory;
        private readonly ISnsGateway _snsGateway;
        private readonly ISnsFactory _snsFactory;
        private readonly ILogger<ApplicationActivityHistory> _logger;

        public ApplicationActivityHistory(
            IHttpContextAccessor contextAccessor,
            IHttpContextWrapper contextWrapper,
            ITokenFactory tokenFactory,
            ISnsGateway snsGateway,
            ISnsFactory snsFactory,
            ILogger<ApplicationActivityHistory> logger)
        {
            _contextAccessor = contextAccessor;
            _contextWrapper = contextWrapper;
            _tokenFactory = tokenFactory;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
            _logger = logger;
        }

        public void LogActivity(Application application, EntityActivity<ApplicationActivityType> activity)
        {
            // we only want to log activites after an application has been submitted
            if (activity == null
                || application == null
                || application.Status == ApplicationStatus.Verification
                || application.Status == ApplicationStatus.New)
            {
                return;
            }

            var token = GetToken(application, activity);

            if (token != null && activity.HasChanges())
            {
                var applicationSnsMessage = _snsFactory.Update(application.Id, activity.OldData, activity.NewData, token);
                _snsGateway.Publish(applicationSnsMessage);
            }
            else if (token == null)
            {
                _logger.LogWarning("Unable to publish activity. No valid auth token has been found");
            }
        }

        private Token GetToken(Application application, EntityActivity<ApplicationActivityType> activity)
        {
            var token = _tokenFactory.Create(_contextWrapper.GetContextRequestHeaders(_contextAccessor.HttpContext));

            // residents will not have an auth token so
            // generate a simple token to hold some user info
            if (ActivityPerformedByResident(activity) == true || token == null)
            {
                token = new Token()
                {
                    Name = application.MainApplicant.Person.FullName,
                    Email = application.MainApplicant.ContactInformation.EmailAddress,
                };
            }

            return token;
        }

        private static bool ActivityPerformedByResident(EntityActivity<ApplicationActivityType> activity)
        {
            return activity.ActivityType == ApplicationActivityType.SubmittedByResident;
        }
    }
}

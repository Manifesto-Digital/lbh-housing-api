using HousingRegisterApi.V1.Domain;
using HousingRegisterApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HousingRegisterApi.V1.Gateways
{
    public interface IActivityHistory
    {
        /// <summary>
        /// Records application activity when status is not in draft and a valid auth token exists.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="activity"></param>
        void LogActivity(Application application, EntityActivity<ApplicationActivityType> activity);

        /// <summary>
        /// Gets the activity history for an application
        /// </summary>
        Task<IList<EntityActivity<ApplicationActivityType>>> GetHistory(Guid applicationId);
    }
}

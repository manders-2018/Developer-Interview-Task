using System;
using System.Collections.Generic;
using InterviewTask.Models;

namespace InterviewTask.Services
{
    public interface IHelperServiceRepository
    {
        IEnumerable<HelperServiceModel> Get();
        HelperServiceModel Get(Guid id);
    }
}

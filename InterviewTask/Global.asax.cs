using InterviewTask.Controllers;
using InterviewTask.Helpers;
using InterviewTask.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InterviewTask
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyResolver.SetResolver(new SimpleDependencyResolver());
        }
    }

    public class SimpleDependencyResolver : IDependencyResolver
    {
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IHelperServiceRepository))
            {
                return new HelperServiceRepository();
            }

            if (serviceType == typeof(HomeController))
            {
                var repository = new HelperServiceRepository();
                SimpleServiceLogger logger = new SimpleServiceLogger();
                return new HomeController(repository, logger);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }
    }

}

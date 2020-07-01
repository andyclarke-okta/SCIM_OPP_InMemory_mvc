[assembly: WebActivator.PostApplicationStartMethod(typeof(OktaSCIMConn.App_Start.SimpleInjectorWebApiInitializer), "Initialize")]

namespace OktaSCIMConn.App_Start
{
    using System.Web.Http;
    using SimpleInjector;
    using SimpleInjector.Integration.WebApi;
    using OktaSCIMConn.Connectors;
    public static class SimpleInjectorWebApiInitializer
    {
        /// <summary>Initialize the container and register it as Web API Dependency Resolver.</summary>
        public static void Initialize()
        {
            var container = new Container();
            //container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();
            
            InitializeContainer(container);

            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
       
            container.Verify();
            
            GlobalConfiguration.Configuration.DependencyResolver =
                new SimpleInjectorWebApiDependencyResolver(container);
        }
     
        private static void InitializeContainer(Container container)
        {

            // This is where you register your connector with the IOC container
            // For instance:
            // container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Scoped);
            container.Register<ISCIMConnector, InMemorySCIMConnector>(Lifestyle.Singleton);
            container.Register<ICacheService, CacheService>(Lifestyle.Singleton);
            //container.Register<ISCIMConnector, SQLServerSCIMConnector>();
        }
    }
}
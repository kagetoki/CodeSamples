using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace AgileFusion.Banking.Services.WebUtils.Routing
{
    /// <summary>
    /// Class for gathering routes of WebApi controllers
    /// and storing them into thread safe table
    /// </summary>
    public class RouteTable
    {
        private ConcurrentDictionary<string, Route> _routes;
        private readonly HashSet<Type> _registeredTypes;
        private readonly object _sync = new object();
        private RouteTable()
        {
            _registeredTypes = new HashSet<Type>();
        }

        #region Public

        /// <summary>
        /// Gets registered routes
        /// </summary>
        public IEnumerable<Route> RegisteredRoutes
        {
            get { return _routes.Values; }
        }

        /// <summary>
        /// Adds route to table, using "Rel" as it's key
        /// </summary>
        /// <param name="route"></param>
        public void Add(Route route)
        {
            _routes.TryAdd(route.Rel, route);
        }

        /// <summary>
        /// Adds routes to table, using "Rel" as a key
        /// </summary>
        /// <param name="routes"></param>
        public void AddRange(IEnumerable<Route> routes)
        {
            foreach (var route in routes)
            {
                Add(route);
            }
        }

        /// <summary>
        /// Gets all the routes from all the ApiControllers in given assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static RouteTable BuildForAssembly(Assembly assembly)
        {
            var result = new RouteTable();
            result._routes = new ConcurrentDictionary<string, Route>();
            var controllerTypes = assembly.GetExportedTypes().Where(t => !t.IsAbstract && typeof(ApiController).IsAssignableFrom(t));
            foreach (var controllerType in controllerTypes)
            {
                result.AddRoutesFrom(controllerType);
            }
            return result;
        }

        /// <summary>
        /// Gets registered route by it's relation
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        public Route this[string relation]
        {
            get { return _routes[relation]; }
        }

        #endregion

        #region Private

        private void AddRoutesFrom(Type controllerType)
        {
            AddType(controllerType);
            StoreEndpoints(controllerType, _routes);
        }

        private void AddType(Type controllerType)
        {
            lock (_sync)
            {
                if (!_registeredTypes.Add(controllerType))
                {
                    throw new InvalidOperationException(String.Format("Routes from {0} are already registered", controllerType));
                }
            }
        }
        
        private static void StoreEndpoints(Type controllerType, ConcurrentDictionary<string, Route> endpointStorage)
        {
            var controllerIdAttr = controllerType.GetCustomAttribute<ControllerIdAttribute>();
            if(controllerIdAttr == null) { throw new InvalidOperationException(String.Format("Controller {0} must have 'ControllerId' attribute specified", controllerType.Name)); }

            var controllerId = controllerIdAttr.Value;
            var endpoints = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var endpoint in endpoints)
            {
                var relativeAttr = (endpoint.GetCustomAttribute(typeof(RelationAttribute)) as RelationAttribute);
                if(relativeAttr == null) { continue; }
                var verb = GetVerb(endpoint);
                endpointStorage.TryAdd(relativeAttr.Name, UrlHelper.CreateRoute(relativeAttr.Name, verb, controllerId, endpoint.Name));
            }
        }

        private static HttpMethod GetVerb(MethodInfo method)
        {
            var attribute = method.GetCustomAttributes().SingleOrDefault(a => a is IActionHttpMethodProvider) as IActionHttpMethodProvider;
            
            return attribute == null ? HttpMethod.Get : attribute.HttpMethods.Single();
        }

        #endregion
    }
}

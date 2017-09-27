using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Routing;
using IDS.Portal;
using IDS.Portal.Service;
using IDS.Web;
using Route = AgileFusion.Banking.Services.WebUtils.Routing.Route;

namespace AgileFusion.Banking.Services.WebUtils.Routing
{
    public class UrlHelper
    {
        private static readonly ConcurrentDictionary<string, string> _controllerInstances = new ConcurrentDictionary<string, string>();

        public static Func<PortalController, object, PortalUrl> GetUrlResolver(string controllerDefinitionId, string routeName)
        {
            return (controller, routeValues) => BuildUrl(controller, controllerDefinitionId, routeName, routeValues);
        }

        /// <summary>
        /// Creates the controller action route.
        /// </summary>
        /// <param name="rel">The relative.</param>
        /// <param name="verb">The verb.</param>
        /// <param name="controllerDefinitionId">The controller definition id.</param>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="anonymous">if set to <c>true</c> route allow anonymous access..</param>
        /// <returns></returns>
        public static Route CreateRoute(string rel, HttpMethod verb, string controllerDefinitionId, string routeName, bool anonymous = false)
        {
            return new Route(rel, verb, GetUrlResolver(controllerDefinitionId, routeName), routeName, anonymous);
        }

        private static PortalUrl BuildUrl(PortalController controller, string controllerDefinitionId, string routeName, object routeValues)
        {
            if (controller == null)
                controller = FindDefaultControllerByDefinitionId(controllerDefinitionId);

            var url = controller.GetUrl(new HttpContextWrapper(HttpContext.Current), routeName, routeValues == null ? null : new RouteValueDictionary(routeValues));
            return new PortalUrl(HttpUrlBuilder.BuildUrl("~/" + url, null));
        }

        private static PortalController FindDefaultControllerByDefinitionId(string controllerDefinitionId)
        {
            string controllerInstanceId;
            if (!_controllerInstances.TryGetValue(controllerDefinitionId, out controllerInstanceId))
            {
                controllerInstanceId = ConfigurationManager.CurrentScheme.InstanceConfiguration.Controllers
                    .Where(c => string.Equals(c.DefinitionID, controllerDefinitionId, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.ID).FirstOrDefault();

                if (!string.IsNullOrEmpty(controllerInstanceId))
                    _controllerInstances.AddOrUpdate(controllerDefinitionId, controllerInstanceId, (k, v) => controllerInstanceId);
            }

            var controller = ConfigurationManager.CurrentScheme.ControllerManager.FindControllerByID(controllerInstanceId);
            return controller;
        }
    }
}
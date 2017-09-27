using System;
using System.Net.Http;
using System.Web;
using IDS.Portal;
using IDS.Portal.Service;

namespace AgileFusion.Banking.Services.WebUtils.Routing
{
    /// <summary>
    /// Represents the api route descriptor.
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Route"/> class.
        /// </summary>
        /// <param name="rel">The route name constant.</param>
        /// <param name="verb">The route http verb.</param>
        /// <param name="urlResolver">The URL resolver.</param>
        /// <param name="routeName">Name of the route controller method.</param>
        /// <param name="allowAnonymous">if set to <c>true</c> route allow anonymous access.</param>
        public Route(string rel, HttpMethod verb, Func<PortalController, object, PortalUrl> urlResolver, string routeName,
            bool allowAnonymous = false)
        {
            Rel = rel;
            Verb = verb;
            UrlResolver = urlResolver;
            AllowAnonymous = allowAnonymous;
            ActionMethod = ActionMethod;
            ActionMethod = routeName;
        }

        private Route()
        {
        }

        public bool AllowAnonymous { get; set; }

        public string Rel { get; set; }

        public string ActionMethod { get; set; }

        public Func<PortalController, object, PortalUrl> UrlResolver { get; set; }

        public HttpMethod Verb { get; set; }

        public object RouteValues { get; set; }

        public Route WithValues(object routeValues)
        {
            return new Route
            {
                Rel = this.Rel,
                UrlResolver = this.UrlResolver,
                Verb = this.Verb,
                RouteValues = routeValues,
                AllowAnonymous = this.AllowAnonymous
            };
        }

        /// <summary>
        /// Creates portal controller link instance.
        /// </summary>
        /// <param name="controller">The optional controller instance to link to. If not set, first found(by definition id) controller is used.</param>
        /// <returns></returns>
        public ResourceLink ToLink(PortalController controller)
        {
            var url = UrlResolver(controller, RouteValues);
            if (url == null)
                return null;

            return new ResourceLink
            {
                Rel = Rel,
                Href = ToRelativeUrl(url.ResolveUrl()),
                Token = AllowAnonymous ? null : url.GetAjaxRequestToken(Verb.Method),
                Verb = Verb.Method
            };
        }

        /// <summary>
        /// Converts URL to the relative URL having part starting after application root url.
        /// </summary>
        /// <param name="virtualUrl">The virtual URL.</param>
        /// <returns>Relative url which is starting after application root url.</returns>
        public static string ToRelativeUrl(string virtualUrl)
        {
            if (String.IsNullOrEmpty(virtualUrl))
                return virtualUrl;

            return VirtualPathUtility.ToAppRelative(virtualUrl).Replace("~", "");
        }
    }
}
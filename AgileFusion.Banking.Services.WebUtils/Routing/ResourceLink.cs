using System.Net.Http;
using Newtonsoft.Json;

namespace AgileFusion.Banking.Services.WebUtils.Routing
{
    /// <summary>
    /// Model class that represents link to the resource.
    /// </summary>
    public class ResourceLink
    {
        private string _verb = HttpMethod.Get.ToString();

        /// <summary>
        /// The self relation name
        /// </summary>
        public const string SelfRel = "self";

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBase" /> class.
        /// </summary>
        public ResourceLink()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBase" /> class using provided href and access token values.
        /// </summary>
        /// <param name="href">The URL.</param>
        /// <param name="token">The token.</param>
        public ResourceLink(string href, string token)
        {
            Href = href;
            Token = token;
        }

        /// <summary>
        /// Gets or sets resource relation.
        /// </summary>
        public string Rel { get; set; }

        /// <summary>
        /// Gets or sets the resource href.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the resource access tokens.
        /// </summary>
        [JsonProperty("X-Request-Token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets HTTP verb.
        /// </summary>
        public string Verb
        {
            get { return _verb; }
            set { _verb = value; }
        }

        /// <summary>
        /// Returns whether this link it empty (points to nowhere) or not.
        /// </summary>
        /// <returns>True if link is empty; false otherwise.</returns>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Href) && string.IsNullOrEmpty(Token);
        }

        /// <summary>
        /// Creates a copy of the link, changing relation name to "self".
        /// </summary>
        /// <returns>"self" link relation for the resource.</returns>
        public ResourceLink ToSelfLink()
        {
            var copy = (ResourceLink)MemberwiseClone();
            copy.Rel = SelfRel;

            return copy;
        }
    }
}

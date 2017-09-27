using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using AgileFusion.Banking.Services.Models.Rsa;
using AgileFusion.Banking.Services.Models.Rsa.Response;
using IDS.Logging;
using IDS.Portal.Security;
using IDS.Portal.Security.Authentication;
using IDS.Security;
using AuthenticationManager = IDS.Portal.Security.Authentication.AuthenticationManager;
using AuthenticationMethod = AgileFusion.Banking.Services.Models.Rsa.AuthenticationMethod;

namespace AgileFusion.Banking.Services.Authentication
{
    /// <summary>
    /// Represents Keypoint specific authentication manager.
    /// </summary>
    public class RsaManager
    {
        private readonly AuthenticationManager _authManager;
        private readonly TimeSpan _challengeTimeout = TimeSpan.Zero;
        private static readonly LogHelper Logger = new LogHelper(LogSystem.CreateTypeContextLogger(MethodBase.GetCurrentMethod().DeclaringType));

        ///<summary>Authentication credentials for HTTP authentication.</summary>
        public const string AuthorizationHeader = "KpcuRsaAuthorization";
        public const string RsaSessionKey = "X-Keypoint-RSA-Token";
        public const string CurrentRiskDataSessionKey = "Keypoint.RiskEvalueation";

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaManager"/> class.
        /// </summary>
        /// <param name="manager">The authentication manager.</param>
        /// <param name="challengeTimeout">The challenge timeout.</param>
        /// <exception cref="System.ArgumentNullException">AuthenticationManager is null.</exception>
        public RsaManager(AuthenticationManager manager, TimeSpan challengeTimeout)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            _authManager = manager;
            _challengeTimeout = challengeTimeout;
        }

        /// <summary>
        /// Gets the RSA token stored by last RSA validation.
        /// </summary>
        public string RsaToken
        {
            get
            {
                return HttpContext.Current.Session[RsaSessionKey] as string;
            }

            set
            {
                HttpContext.Current.Session[RsaSessionKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last risk evaluation value.
        /// </summary>
        public RsaResult CurrentRiskEvaluation
        {
            get
            {
                return HttpContext.Current.Session[CurrentRiskDataSessionKey] as RsaResult;
            }

            set
            {
                HttpContext.Current.Session[CurrentRiskDataSessionKey] = value;

            }
        }

        /// <summary>
        /// Gets a value indicating whether [RSA challenge active].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [RSA challenge active]; otherwise, <c>false</c>.
        /// </value>
        public bool RsaChallengeActive
        {
            get
            {
                var risk = CurrentRiskEvaluation;
                return risk != null && (_challengeTimeout == TimeSpan.Zero || risk.IssueTime - DateTime.UtcNow < _challengeTimeout);
            }
        }

        /// <summary>
        /// Generates the RSA access token and caches it into session.
        /// </summary>
        /// <returns>Generated token</returns>
        public string GenerateRsaAccessToken()
        {
            var token = Guid.NewGuid().ToString("N");
            RsaToken = token;
            return token;
        }

        public virtual void CompleteChallengeAction()
        {
            RsaToken = null;
            CurrentRiskEvaluation = null;
        }

        /// <summary>
        /// Determines whether the risk is acceptable to proceed request.
        /// </summary>
        /// <param name="risk">The risk value.</param>
        /// <returns></returns>
        public static bool IsRiskAcceptable(AuthenticationManager.Risk risk)
        {
            var result = risk == AuthenticationManager.Risk.Acceptable ||
                         risk == AuthenticationManager.Risk.ServiceDownAllowAccess ||
                         risk == AuthenticationManager.Risk.RefreshCollect ||
                         risk == AuthenticationManager.Risk.RefreshCollectRequired ||
                         risk == AuthenticationManager.Risk.CollectOptional ||
                         risk == AuthenticationManager.Risk.CollectRequired;

            return result;
        }

        /// <summary>
        /// Checks whether request contains the RSA authentication information.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="eventInfo">The rsa event information.</param>
        /// <param name="throwOnAuthFailure">if set to <c>true</c> [throw on authentication failure].</param>
        /// <param name="getEntityHash">The get entity hash action.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        public virtual bool IsRsaAuthenticated(HttpRequest request, object eventInfo, bool throwOnAuthFailure, Func<object, string> getEntityHash = null)
        {
            getEntityHash = getEntityHash ?? Convert.ToString;
            var authHeader = request.Headers.GetValues(AuthorizationHeader);
            var isAuthenticationAttempt = RsaChallengeActive && authHeader != null && authHeader.Length == 1
                                            && eventInfo != null && !string.IsNullOrEmpty(RsaToken);

            var authenticated = isAuthenticationAttempt && RsaToken.Equals(authHeader[0]) &&
                string.Equals(CurrentRiskEvaluation.RsaEventHash, getEntityHash(eventInfo));

            if (throwOnAuthFailure && !authenticated)
            {
                Logger.LogDebug("Incorrect Keypoint RSA verification for {0} {1}", request.HttpMethod, request.AppRelativeCurrentExecutionFilePath);
                var content = new ObjectContent<UnexpectedErrorRsaResponse>(new UnexpectedErrorRsaResponse("RSA challenge failed.", Models.Properties.ExtendedResources.UnexpectedErrorCallSupport), GlobalConfiguration.Configuration.Formatters.JsonFormatter);
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = content };
                throw new HttpResponseException(response);
            }

            return authenticated;
        }

        /// <summary>
        /// Authenticates the request against the current RSA risk evaluation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="user">The user to execute authentication for.</param>
        /// <param name="eventInfo">The event information.</param>
        /// <param name="getEntityHash">The get entity hash action.</param>
        /// <returns></returns>
        /// <exception cref="RsaResponse">null;null</exception>
        public virtual AuthenticationRiskResult AuthenticateRequest(HttpRequest request, IUser user, object eventInfo, Func<object, string> getEntityHash = null)
        {
            getEntityHash = getEntityHash ?? Convert.ToString;
            if (IsRsaAuthenticated(request, eventInfo, false, getEntityHash))
            {
                CompleteChallengeAction();
                Logger.LogDebug("Keypoint RSA verification passed for {0} {1}", request.HttpMethod, request.AppRelativeCurrentExecutionFilePath);
                return new AuthenticationRiskResult(AuthenticationRisk.Pass, new List<IDS.Portal.Security.Authentication.AuthenticationMethod>());
            }
            else
            {
                var riskResponse = _authManager.EvaluateRisk(user, eventInfo);
                if (riskResponse.Risk != AuthenticationRisk.Pass)
                {
                    if (riskResponse.AuthenticationMethods.Count != 0)
                    {
                        var methods = riskResponse.AuthenticationMethods ?? new List<IDS.Portal.Security.Authentication.AuthenticationMethod>();
                        CurrentRiskEvaluation = new RsaResult(riskResponse.Risk, user, getEntityHash(eventInfo), methods.Select(m => new AuthenticationMethod(m)).ToList());
                    }
                }
                return riskResponse;
            }
        }

        public static HttpResponseException PrepareRsaChallengeResponse(RsaResponse rsaData)
        {
            var content = new ObjectContent<RsaResponse>(rsaData, GlobalConfiguration.Configuration.Formatters.JsonFormatter);
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = content };
            return new HttpResponseException(response);
        }
    }
}
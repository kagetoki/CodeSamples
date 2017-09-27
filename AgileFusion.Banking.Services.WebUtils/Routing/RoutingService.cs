using System;
using IDS.ComponentModel;
using IDS.Portal;
using IDS.Portal.Service;

namespace AgileFusion.Banking.Services.WebUtils.Routing
{
    /// <summary>
    /// Allows configuring cross-controllers references within deployment bundle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To have multiple instances of the same controllers within single schema we have to distinguish them and be able to configure routes to the right controllers.
    /// To make it work we allow to explicitly select all related controller references within single service. Service is used since controller instance is not long-living object
    /// and from single controller reference we only can have scheme id for that controller, but not configured instance. so we can't reach second level of referenced controllers from it.
    /// But we can reference service instance from controller, so it's more suitable in terms of reaching any other controller in bundle through single service reference.
    /// </para>
    /// 
    /// <para>
    /// The use case for having multiple bundles(set of instances) within same schema is not very clear since we'll likely change definitionId(in manifest) if breaking change in code will be introduced.
    /// So this mechanism is more like optional solution for future use.
    /// </para>
    /// </remarks>
    public class RoutingService
    {
        [ComponentSetting("Entry Point Controller", "Entry Point Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl EntryController { get; set; }

        public PortalController Entry { get { return FindController(EntryController); } }


        [ComponentSetting("Login Controller", "Login Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl LoginController { get; set; }

        public PortalController Login { get { return FindController(LoginController); } }


        [ComponentSetting("Accounts Controller", "Account Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl AccountsController { get; set; }

        public PortalController Accounts { get { return FindController(AccountsController); } }


        [ComponentSetting("Fast Balances Controller", "Fast Balances Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl FastBalancesController { get; set; }

        public PortalController FastBalances { get { return FindController(FastBalancesController); } }


        [ComponentSetting("eStatements Controller", "eStatements Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl InfoImageController { get; set; }

        public PortalController InfoImage { get { return FindController(InfoImageController); } }


        [ComponentSetting("Marketing Controller", "Marketing Controller instance reference(first found if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl MarketingController { get; set; }

        public PortalController Marketing { get { return FindController(MarketingController); } }


        [ComponentSetting("Rdc Controller", "Rdc Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl RdcController { get; set; }

        public PortalController Rdc { get { return FindController(RdcController); } }


        [ComponentSetting("Settings Controller", " Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl SettingsController { get; set; }

        public PortalController Settings { get { return FindController(SettingsController); } }

        [ComponentSetting("User Controller", " Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl UserController { get; set; }

        public PortalController User { get { return FindController(UserController); } }

        [ComponentSetting("Transfers Controller", " Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl TransfersController { get; set; }

        public PortalController Transfers { get { return FindController(TransfersController); } }


        [ComponentSetting("Bill Pay Controller", "Bill Pay Controller instance reference(first found used if not set)", "Controllers", ComponentEditorType.ControllerSelector)]
        public PortalUrl BillPayController { get; set; }

        public PortalController BillPay { get { return FindController(BillPayController); } }

        protected virtual PortalController FindController(PortalUrl controllerReference)
        {
            if (controllerReference == null || controllerReference.UrlType != UrlType.PortalController || string.IsNullOrEmpty(controllerReference.PortalControllerID))
                return null;

            int length = controllerReference.PortalControllerID.IndexOf("/", StringComparison.Ordinal);
            if (length > 0 && length <= controllerReference.PortalControllerID.Length - 1)
            {
                return ConfigurationManager.CurrentScheme.ControllerManager.FindControllerByID(controllerReference.PortalControllerID.Substring(0, length));
            }
           
            return null;
        }
    }
}

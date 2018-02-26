using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;



namespace LMS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Application["AppTitle"] = "";
            GlobalFilters.Filters.Add(new ValidateInputAttribute(false));

            //registering our custom model validation provider
            ModelValidatorProviders.Providers.Clear();
            ModelValidatorProviders.Providers.Add(new DisallowHtmlMetadataValidationProvider());

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Response.Clear();

            HttpException httpException = exception as HttpException;

            if (httpException != null)
            {
                string action;

                switch (httpException.GetHttpCode())
                {

                    case 404:
                        // page not found
                        action = "HttpError404";
                        Server.ClearError();
                        Response.Clear();
                        Response.Redirect("~/Error/FileNotFound");
                        break;
                    case 401:
                        // page not found
                        action = "HttpError404";
                        Server.ClearError();
                        Response.Clear();
                        Response.Redirect("~/Error/Unauthorised");
                        break;
                    default:
                        // action = "General";
                        Server.ClearError();
                        Response.Clear();
                        Response.Redirect(String.Format("~/Error?message={0}", exception.Message));
                        break;
                }

                // clear error on server
                //Server.ClearError();
                //Response.Redirect(String.Format("~/Error/General?message={0}", exception.Message));

            }
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            string culture = "hi";

            try
            {
                culture = Convert.ToString(Session["lang"]);
            }
            catch (Exception ex)
            {
                //culture = Request.UserLanguages[0];
            }
            //if (Request.UserLanguages != null)
            //{
            //    culture = Request.UserLanguages[0];
            //}

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
        }
    }

    public class DisallowHtmlAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var tagWithoutClosingRegex = new Regex(@"<[^>]+>");

            var hasTags = tagWithoutClosingRegex.IsMatch(value.ToString());

            if (!hasTags)
                return ValidationResult.Success;

            return new ValidationResult(validationContext.DisplayName + " " + LMSResourse.Common.Common.msgAltHtmlTagsFalse);

        }
    }

    public class DisallowHtmlMetadataValidationProvider : DataAnnotationsModelValidatorProvider
    {
        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata,
           ControllerContext context, IEnumerable<Attribute> attributes)
        {
            if (attributes == null)
                return base.GetValidators(metadata, context, null);
            if (string.IsNullOrEmpty(metadata.PropertyName))
                return base.GetValidators(metadata, context, attributes);
            //DisallowHtml should not be added if a property allows html input
            var isHtmlInput = attributes.OfType<AllowHtmlAttribute>().Any();
            if (isHtmlInput)
                return base.GetValidators(metadata, context, attributes);
            attributes = new List<Attribute>(attributes) { new DisallowHtmlAttribute() };
            return base.GetValidators(metadata, context, attributes);
        }
    }
}

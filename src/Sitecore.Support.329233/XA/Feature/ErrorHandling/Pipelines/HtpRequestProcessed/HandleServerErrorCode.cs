using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Web;
using Sitecore.XA.Foundation.Abstractions;
using Sitecore.XA.Foundation.Multisite;

namespace Sitecore.XA.Feature.ErrorHandling.Pipelines.HtpRequestProcessed
{
    public class HandleServerErrorCode : HttpRequestProcessor
    {
        protected ISiteInfoResolver SiteInfoResolver;
        protected IContext Context { get; }= ServiceLocator.ServiceProvider.GetService<IContext>();

        public HandleServerErrorCode()
        {
            SiteInfoResolver = ServiceLocator.ServiceProvider.GetService<ISiteInfoResolver>();
        }

        public override void Process(HttpRequestArgs args)
        {
            if (args.Context.Error != null && !Context.Site.Name.Equals("shell"))
            {
                var siteInfos = GetPossibleSites();
                var site = SiteInfoResolver.ResolveSiteFromRequest(siteInfos, new HttpRequestWrapper(args.Context.Request));

                if (site != null)
                {
                    var url = GetStaticErrorPageUrl(site);
                    if (File.Exists(FileUtil.MapPath(url)))
                    {
                        args.Context.Server.TransferRequest(url);
                    }
                    else
                    {
                        Log.Warn($"Could not find proper static error page for site: {site.Name}. Please generate it.", this);
                    }
                }
            }
        }

        protected virtual SiteInfo[] GetPossibleSites()
        {
            return SiteInfoResolver.Sites.OrderByDescending(info => info.VirtualFolder.Length).ToArray();
        }

        protected virtual string GetStaticErrorPageUrl(SiteInfo site)
        {
            return $"{Constants.ErrorPagesFolder}/{site.Name}.html";
        }
    }
}
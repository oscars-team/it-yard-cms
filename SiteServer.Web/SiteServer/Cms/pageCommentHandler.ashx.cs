using SiteServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Web;

namespace SiteServer.API.SiteServer.Cms
{
    /// <summary>
    /// pageCommentHandler 的摘要说明
    /// </summary>
    public class pageCommentHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string action = context.Request.QueryString["action"];
            string idString = context.Request.QueryString["ids"];
            string[] ids = idString.Split(',');
            if (action == "delete")
            {
                doDelete(context, ids);
            }

            if (action == "audit")
            {
                doAudit(context, ids);
            }
        }

        private void doDelete(HttpContext context, string[] ids)
        {
            try
            {
                using (var conn = SqlUtils.GetIDbConnection(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString))
                {
                    var sql = "delete from siteserver_Comment where id=@id";
                    int a = conn.Execute(sql, ids.Select(p => new { id = p }));
                    context.Response.Write("ok");
                }
            }
            catch (Exception ex)
            {
                context.Response.Write(ex.Message);
            }

        }
        private void doAudit(HttpContext context, string[] ids)
        {
            using (var conn = SqlUtils.GetIDbConnection(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString))
            {
                var sql = "update siteserver_Comment set isCheck=1 where id=@id";
                int a = conn.Execute(sql, ids.Select(p => new { id = p }));
                context.Response.Write("ok");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
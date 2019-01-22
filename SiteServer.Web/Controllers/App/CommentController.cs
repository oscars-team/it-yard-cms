using Dapper;
using SiteServer.CMS.Core;
using SiteServer.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SiteServer.API.Controllers.App
{

    [RoutePrefix("app/comment")]
    public class CommentController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult Get(string contentid)
        {
            try
            {
                return Ok(getCommentByContentId(contentid));
            }
            catch (Exception e)
            {
                LogUtils.AddErrorLog(e);
                return InternalServerError(e);
            }
        }
        [HttpPost, HttpGet, Route("insert")]
        public IHttpActionResult insert(string content, string author, string contentid)
        {
            try
            {
                return Ok(insertComment(content, author, contentid));
            }
            catch (Exception e)
            {
                LogUtils.AddErrorLog(e);
                return InternalServerError(e);
            }
        }

        public dynamic[] getCommentByContentId(string contentid)
        {
            dynamic[] comment;
            using (var conn = SqlUtils.GetIDbConnection(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString))
            {
                var sql = "select comm.* from siteserver_Comment comm inner join siteserver_Content_1 con  on comm.content_id=con.Id  where comm.isCheck=1 and comm.content_id=@contentid order by comm.create_time desc";
                comment = conn.Query(sql, new { contentid = contentid }).ToArray();
            }
            return comment;
        }


        public int insertComment(string content, string author, string contentid)
        {
            if (checkMore(author, contentid) > 2)
            {
                return 0;
            }
            else {
                var createtime = DateTime.Now;
                using (var conn = SqlUtils.GetIDbConnection(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString))
                {
                    var sql = "insert into  siteserver_Comment(create_time,author,content_id,title,isCheck) values(@createtime,@author,@contentid,@content,@isCheck)";
                    int a = conn.Execute(sql, new { createtime = createtime, author = author, contentid = contentid, content = content,isCheck=0});
                    return a;
                }
            }
        }

        public int checkMore(string author, string contentid) {
            using (var conn = SqlUtils.GetIDbConnection(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString))
            {
                var sql = "select * from siteserver_Comment c where c.content_id=@contentid and  c.author=@author";
                var count = conn.Query(sql, new { contentid = contentid, author = author }).ToArray();
                return count.Length;
            }
        }
    }
}

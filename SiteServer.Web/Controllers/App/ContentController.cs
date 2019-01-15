using Dapper;
using SiteServer.CMS.Core;
using SiteServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SiteServer.API.Controllers.App
{
    [RoutePrefix("app/content")]
    public class ContentController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult Get(int cid, int page = 1, int size = 10)
        {
            try
            {
                return Ok(getConentsByChannel(cid, page, size));
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        private dynamic[] getConentsByChannel(int channelId, int page = 1, int size = 10)
        {
            dynamic[] contents;
            using (var conn = SqlUtils.GetIDbConnection(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString))
            {
                int min = (page - 1) * size;
                int max = page * size;
                var sqlQuery = @"select   ct.ChannelId channelId
                                        , cn.ChannelName channelName
                                        , ct.Id id
                                        , Title title
                                        , ct.AddDate time
                                        , ct.ImageUrl image
                                        , VideoUrl video
                                        , ct.Content content 
                                        , isTop
                                        , isHot
                                        , isColor
                                        , isRecommend
                                        from siteserver_Content_1 ct
                                        inner join siteserver_Channel cn on ct.ChannelId = cn.Id
                                        where channelId=@channelId";
                //var sqlCount = $"select count(1) from( {sqlQuery} ) AS t";
                string order = "order by isTop desc, isHot desc, isColor desc, isRecommend desc";
                var sqlPagedQuery = $"Select * From (Select ROW_NUMBER() Over ( {order} )  As rowNum, * From ( {sqlQuery} ) As T ) As N Where rowNum > @min And rowNum <= @max";
                contents = conn.Query(sqlPagedQuery, new { channelId, min, max }).ToArray();
            }
            return contents;
        }
    }
}

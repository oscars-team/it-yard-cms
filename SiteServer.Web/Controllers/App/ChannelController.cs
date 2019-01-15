using SiteServer.CMS.Core;
using SiteServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dapper;
namespace SiteServer.API.Controllers.App
{
    internal class Channel
    {
        public int cateId { get; set; }
        public string title { get; set; }
        public int pid { get; set; }
        public List<Channel> children { get; set; }

        public Channel()
        {
            children = new List<Channel>();
        }
    }
    [RoutePrefix("app/channel")]
    public class ChannelController : ApiController
    {
        Channel[] mappingChannels;

        [HttpGet, Route("")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                Channel[] channels;
                using (var conn = SqlUtils.GetIDbConnection(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString))
                {
                    channels = conn.Query<Channel>("select ChannelName title, Id id, ParentId pid from siteserver_Channel where ParentId=@parentId", new { parentId = id }).ToArray();
                }
                return Ok(channels);
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpGet, Route("tree")]
        public IHttpActionResult GetTree(int id)
        {
            if (id < 1) id = 1;
            using (var conn = SqlUtils.GetIDbConnection(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString))
            {
                mappingChannels = conn.Query<Channel>(@"with channel as (
                                        select Id cateId, ChannelName title, ParentId pid from siteserver_Channel where Id = @id
                                        union all
                                        select Id cateId, ChannelName title, ParentId pid from siteserver_Channel c 
                                        inner join channel c2 on c.ParentId = c2.cateId
                                        ) select * from channel", new { id }).ToArray();
            }
            return Ok(mapChannel(id));
        }

        private Channel mapChannel(int id)
        {
            Channel root;
            root = new Channel { cateId = id };
            doMap(root);
            return root;
        }

        private void doMap(Channel root)
        {
            var children = mappingChannels.Where(p => p.pid == root.cateId).ToList();
            children.ForEach(e =>
            {
                var child = new Channel()
                {
                    cateId = e.cateId,
                    pid = e.pid,
                    title = e.title
                };
                doMap(child);
                root.children.Add(child);
            });
        }
    }
}

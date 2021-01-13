﻿using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Actions
{
    [Route("actions")]
    public class ActionsController : SettingsApp
    {
        public Response Post(string action)
        {
            Console.WriteLine("POST");

            if (action == "restart")
            {
                Console.WriteLine("restart");
                Core.Instance.Actions.RestartSystem();
            }

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = new Uri(Context.Referrer.AbsoluteUri)
            };
        }
    }
}

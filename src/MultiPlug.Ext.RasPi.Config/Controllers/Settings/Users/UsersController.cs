using System;
using System.Linq;
using System.Threading.Tasks;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components.Users;
using MultiPlug.Ext.RasPi.Config.Utils;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.SharedRazor;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Users
{
    [Route("users")]
    public class UsersController : SettingsApp
    {
        public Response Get(string msg)
        {
            if (! Hardware.isRunningRaspberryPi)
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Redirect,
                    Location = new Uri(Context.Paths.Base + Context.Paths.Home)
                };
            }

            Response Result = null;

            if (string.IsNullOrEmpty(msg))
            {
                Result = new Response
                {
                    Model = new UsersProperties(),
                    Template = Templates.Users
                };
            }
            else
            {
                switch(msg)
                {
                    case "1":
                        Result = new Response
                        {
                            Model = new UsersProperties { Error = true, Message = "Username can not be blank" },
                            Template = Templates.Users
                        };
                        break;
                    case "2":
                        Result = new Response
                        {
                            Model = new UsersProperties { Error = true, Message = "Existing password can not be blank" },
                            Template = Templates.Users
                        };
                        break;
                    case "3":
                        Result = new Response
                        {
                            Model = new UsersProperties { Error = true, Message = "New passwords do not match" },
                            Template = Templates.Users
                        };
                        break;
                    case "4":
                        Result = new Response
                        {
                            Model = new UsersProperties { Error = true, Message = "New password can not be blank" },
                            Template = Templates.Users
                        };
                        break;
                    case "5":
                        Result = new Response
                        {
                            Model = new UsersProperties { Error = true, Message = "Username is not found" },
                            Template = Templates.Users
                        };
                        break;
                    case "6":
                        Result = new Response
                        {
                            Model = new UsersProperties { Error = true, Message = "System Error: More than one entry found with Username" },
                            Template = Templates.Users
                        };
                        break;
                    case "7":
                        Result  = new Response
                        {
                            Model = new UsersProperties { Error = true, Message = "Incorrect Existing Password" },
                            Template = Templates.Users
                        };
                        break;

                    case "9":
                        Result = new Response
                        {
                            Model = new UsersProperties { Error = true, Message = "System Error while trying to set new Password" },
                            Template = Templates.Users
                        };
                        break;

                    case "10":
                        Result = new Response
                        {
                            Model = new UsersProperties { Error = false, Message = "Password was changed sucessfully" },
                            Template = Templates.Users
                        };
                        break;

                    default:
                        Result = new Response
                        {
                            Model = new UsersProperties(),
                            Template = Templates.Users
                        };
                        break;
                }
            }

            return Result;
        }

        public Response Post(UsersProperties theModel)
        {
            if ( string.IsNullOrEmpty( theModel.Username ) )
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=1")
                };
            }

            if (string.IsNullOrEmpty(theModel.OldPassword))
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=2")
                };
            }

            if (theModel.NewPassword != theModel.NewPasswordCheck)
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=3")
                };
            }

            if (string.IsNullOrEmpty(theModel.NewPassword))
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=4")
                };
            }

            Task<ProcessResult> UserEntries = ProcessRunner.GetProcessResultAsync("grep", "^" + theModel.Username + " /etc/shadow");

            UserEntries.Wait();

            var CommandResultLines = UserEntries.Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                                                .Select(line => line.TrimEnd())
                                                                .ToArray();

            if( CommandResultLines.Length == 0)
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=5")
                };
            }
            
            if(CommandResultLines.Length > 1)
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=6")
                };
            }

            // Format of Line is Username$AlgorithmType$Salt$HashedPassword:OtherMeta
            string UserEntry = CommandResultLines[0];

            const string Dollar = "$";
            const string Colon = ":";

            int StartIndex = UserEntry.IndexOf(Dollar);
            int EndHashAlgorithmTypeIndex = UserEntry.IndexOf(Dollar, StartIndex + Dollar.Length);
            int EndSaltIndex = UserEntry.IndexOf(Dollar, EndHashAlgorithmTypeIndex + Dollar.Length);
            int EndHashIndex = UserEntry.IndexOf(Colon, StartIndex + Dollar.Length);

            string HashAlgorithmType = UserEntry.Substring(StartIndex, EndHashAlgorithmTypeIndex - StartIndex + Dollar.Length);

            // Existing Hash, including the Algorithm Type $6$, the Salt and the Hash
            string ExistingHash = UserEntry.Substring(StartIndex, EndHashIndex - StartIndex);
            string Salt = UserEntry.Substring(EndHashAlgorithmTypeIndex + Dollar.Length, EndSaltIndex - EndHashAlgorithmTypeIndex - Dollar.Length);

            string NewHash = Linux.Crypt(theModel.OldPassword, HashAlgorithmType + Salt + Dollar);

            if(ExistingHash != NewHash)
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=7")
                };
            }

            Task<ProcessResult> ChangePassword = ProcessRunner.GetProcessResultAsync("bash", "-c \"echo '" + theModel.Username + ":" + theModel.NewPassword + "' | chpasswd\"");

            ChangePassword.Wait();

            if(ChangePassword.Result.Okay())
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=10")
                };
            }
            else
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Moved,
                    Location = new Uri(RemoveOldQueryString(Context.Request.AbsoluteUri) + "?msg=9")
                };
            }
        }

        private string RemoveOldQueryString( string theAbsoluteUri)
        {
            return theAbsoluteUri.Split('?')[0];
        }
    }
}

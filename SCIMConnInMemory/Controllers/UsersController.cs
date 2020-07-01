using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Okta.SCIM.Models;
using OktaSCIMConn.Exceptions;
using log4net;
using OktaSCIMConn.Connectors;



namespace OktaSCIMConn.Controllers
{
   // [Authorize]
    public class UsersController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // this connector will be initilized via the IOC container.  Passed to the controller constructor.
        // see SimpleInjectorWebAppInitializer.cs
        private static ISCIMConnector connector;

        public UsersController(ISCIMConnector conn)
        {
            connector = conn;
        }

        [HttpGet]
        public IHttpActionResult getAllUsers(int startIndex, int count)
        {
            // used on okta import
            logger.Debug("Enter getAllUsers without filter ");
            PaginationProperties pp = new PaginationProperties(count, startIndex);
            try
            {
                Okta.SCIM.Models.SCIMUserQueryResponse rGetUsers = connector.getUsers(pp, null);
                if (rGetUsers == null)
                {
                    logger.Debug("Exit no users not found");
                    return NotFound();
                }
                else
                {
                    logger.Debug("Exit Successfully found users ");
                    return Ok<SCIMUserQueryResponse>(rGetUsers);
                }
            }
            catch (EntityNotFoundException e)
            {
                logger.Debug(" Exit entity not found trying to get user");
                return NotFound();
            }
            catch (Exception e)
            {
                logger.Debug("Exit Error at getAllUsers without filter");
                logger.Error(e);

                return InternalServerError(e);
            }
        }

        [HttpGet]
        public IHttpActionResult getAllUsers(string filter, int startIndex, int count)
        {
            logger.Debug("Enter getAllUsers by Filter ");
            SCIMFilter f = SCIMFilter.TryParse(filter);
            PaginationProperties pp = new PaginationProperties(count, startIndex);
            try
            {
                Okta.SCIM.Models.SCIMUserQueryResponse rGetUsers = connector.getUsers(pp, f);
                if (rGetUsers == null)
                {
                    logger.Debug("Exit no users not found");
                    return NotFound();
                }
                else
                {
                    logger.Debug("Exit Successfully user found ");
                    return Ok<SCIMUserQueryResponse>(rGetUsers);
                }

            }
            catch (EntityNotFoundException e)
            {
                logger.Debug("Exit entity not found trying to get user");
                return NotFound();
            }
            catch (Exception e)
            {
                logger.Debug("Exit Error at getUser  ");
                logger.Error(e);
                return InternalServerError(e);
            }
        }



        //for debug only
        [HttpGet]
        public IHttpActionResult getBulkUsers()
        {
            logger.Debug("Enter getBulkUsers ");

            SCIMFilter f = new SCIMFilter();
            PaginationProperties pp = new PaginationProperties(200, 1);

            try
            {


                Okta.SCIM.Models.SCIMUserQueryResponse rGetUsers = connector.getUsers(pp, f);
                logger.Debug("Exit Successful getBulk Users ");
                return Ok<SCIMUserQueryResponse>(rGetUsers);
            }
            catch (Exception e)
            {
                logger.Debug("Exit Error at getBulkUsers ");
                logger.Error(e);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        public IHttpActionResult getUser(string id)
        {

            SCIMUser scimUserOut = new SCIMUser();

            if (id == null)
            {
                logger.Error("Error at getUser by Id, null value");
                return BadRequest();
            }
            else
            {
                logger.Debug("Enter getUser " + id);
            }

            try
            {
                scimUserOut = connector.getUser(id);
                if (string.IsNullOrEmpty(scimUserOut.id))
                {
                    logger.Debug("Exit Success but user not found");
                    return NotFound();
                }
                else
                {
                    logger.Debug("Exit Successfully found  user  username " + scimUserOut.userName + "  appId " + scimUserOut.id);
                    return Ok(scimUserOut);
                }
            }
            catch (EntityNotFoundException e)
            {
                logger.Debug("Exit entity not found trying to get user");
                return NotFound();
            }
            catch (Exception e)
            {
                logger.Debug("Exit Error at getUser  ");
                logger.Error(e);
                return InternalServerError(e);
            }
        }


        [HttpPost]
        public IHttpActionResult PostUser([FromBody] SCIMUser user)
        {
            logger.Debug("Enter PostUser " + user.userName);
            SCIMUser scimUserOut = new SCIMUser();
            try
            {
                scimUserOut = connector.createUser(user);
                if (string.IsNullOrEmpty(scimUserOut.id))
                {
                    logger.Error("Exit error create user name " + user.userName);
                    SCIMException createException = new SCIMException();
                    createException.ErrorMessage = "error create user name " + user.userName;
                    createException.ErrorSummary = "error create user name " + user.userName;
                    return InternalServerError(createException);
                }
                else
                {
                    //return Ok();
                    logger.Debug("Exit Successfully created  user  username " + scimUserOut.userName + "  appId " + scimUserOut.id);
                    string uri = Url.Link("DefaultAPI", new { id = user.id });
                    return Created<SCIMUser>(uri, scimUserOut);
                }

            }
            catch (Exception e)
            {
                logger.Debug("Exit Exception at PostUser ");
                logger.Error(e);

                return InternalServerError(e);
            }
        }


        [HttpPut]
        public IHttpActionResult Put(string id, [FromBody]SCIMUser user)
        {

            SCIMUser scimUserOut = new SCIMUser();
            if (id == null)
            {
                logger.Error("Error at PUT User, id missing ");
                return BadRequest();
            }
            else
            {
                logger.Debug("Enter Put " + user.displayName + " Id " + id);
            }

            try
            {
                user.id = id;
                scimUserOut = connector.updateUser(user);
                if (string.IsNullOrEmpty(scimUserOut.id))
                {
                    logger.Error("Exit error update user id " + id);
                    SCIMException updateException = new SCIMException();
                    updateException.ErrorMessage = "error update user id " + id;
                    updateException.ErrorSummary = "error update user id " + id;
                    return InternalServerError(updateException);
                }
                else
                {
                    //return Ok();
                    logger.Debug("Exit Successfully updated  user  username " + scimUserOut.userName + "  appId " + scimUserOut.id);
                    return Ok<SCIMUser>(scimUserOut);
                }
            }

            catch (Exception e)
            {
                logger.Debug("Exit Error at PUT User");
                logger.Error(e);
                return InternalServerError(e);
            }
        }

        //Okta SCIM interface uses PATCH to delete/disable user
        [HttpPatch]
        public IHttpActionResult Patch(String id, [FromBody]SCIMUserOperation operation)
        {
            bool result = true;
            SCIMUser scimUserOut = new SCIMUser();
            if (id == null)
            {
                logger.Error("Error at PATCH User, id missing ");
                return BadRequest();
            }
            else
            {
                logger.Debug("Enter Patch  Id " + id);
            }
            try
            {
                if (operation.Operations[0].op == "replace")
                {
                    if (operation.Operations[0].value.active)
                    {
                        scimUserOut = connector.reactivateUser(id);
                    }
                    else
                    {
                        scimUserOut = connector.deactivateUser(id);
                    }

                    if (string.IsNullOrEmpty(scimUserOut.id))
                    {
                        logger.Error("Exit error update user id " + id);
                        SCIMException updateException = new SCIMException();
                        updateException.ErrorMessage = "error update user id " + id;
                        updateException.ErrorSummary = "error update user id " + id;
                        return InternalServerError(updateException);
                    }
                    else
                    {
                        //return Ok();
                        logger.Debug("Exit Successfully updated  user  username " + scimUserOut.userName + "  appId " + scimUserOut.id);
                        return Ok<SCIMUser>(scimUserOut);
                    }

                }
                else
                {
                    logger.Debug("Exit Patch user failed with unknown operation " + id);
                    SCIMException patchException = new SCIMException();
                    patchException.ErrorMessage = "Patch user failed with unknown operation id " + id;
                    patchException.ErrorSummary = "Patch user failed with unknown operation " + id;
                    return InternalServerError(patchException);
                }

            }
            catch (Exception e)
            {
                logger.Debug("Exit Error at Patching User Status  ");
                logger.Error(e);
                return InternalServerError(e);
            }
        }

        //Delete is not supported for Okta SCIM interface
        [HttpDelete]
        public IHttpActionResult Delete(String id)
        {
            logger.Debug("Enter Delete " + id);


            try
            {
                connector.deactivateUser(id);
                logger.Debug("Exit delete user id " + id);
                //return Ok();
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                logger.Debug("Exit Error at Delete User  ");
                logger.Error(e);
                return InternalServerError(e);
            }
        }
    }
}

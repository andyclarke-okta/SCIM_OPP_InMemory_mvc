using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Okta.SCIM.Models;
using OktaSCIMConn.Exceptions;
using OktaSCIMConn.Connectors;
using log4net;
using System.Text.RegularExpressions;

namespace OktaSCIMConn.Controllers
{
    
    public class GroupsController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // this connector will be initilized via the IOC container.  Passed to the controller constructor.
        // see SimpleInjectorWebAppInitializer.cs
        private static ISCIMConnector connector;
        public GroupsController(ISCIMConnector conn)
        {
            connector = conn;
        }



        [HttpGet]
        public IHttpActionResult getAllGroups(int startIndex, int count)
        {
            logger.Debug("getAllGroups ");
            try
            {
                PaginationProperties pp = new PaginationProperties(count, startIndex);
                return Ok<SCIMGroupQueryResponse>(connector.getGroups(pp));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        public IHttpActionResult getGroup(string id)
        {
            logger.Debug("getGroup " + id);
            try
            {
                return Ok(connector.getGroup(id));
            }
            catch (EntityNotFoundException e)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPost]
        public IHttpActionResult PostGroup([FromBody] SCIMGroup group)
        {
            logger.Debug("PostGroup " + group.displayName);
            try
            {
                group = connector.createGroup(group);
                string uri = Url.Link("DefaultAPI", new { id = group.id });
                return Created<SCIMGroup>(uri, group);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpPut]
        public IHttpActionResult Put(string id, [FromBody]SCIMGroup group)
        {

            SCIMGroup scimGroupOut = new SCIMGroup();
            if (id == null)
            {
                logger.Error("Error at PUT Group, id missing ");
                return BadRequest();
            }
            else
            {
                logger.Debug("Enter Put " + group.displayName + " Id " + id);
            }

            try
            {
                group.id = id;
                scimGroupOut = connector.updateGroup(group);
                if (string.IsNullOrEmpty(scimGroupOut.id))
                {
                    logger.Error("Exit error update group id " + id);
                    SCIMException updateException = new SCIMException();
                    updateException.ErrorMessage = "error update group id " + id;
                    updateException.ErrorSummary = "error update group id " + id;
                    return InternalServerError(updateException);
                }
                else
                {
                    //return Ok();
                    logger.Debug("Exit Successfully updated  group " + scimGroupOut.displayName );
                    return Ok<SCIMGroup>(scimGroupOut);
                }
            }

            catch (Exception e)
            {
                logger.Debug("Exit Error at PUT User");
                logger.Error(e);
                return InternalServerError(e);
            }

        }


        
        [HttpPatch]
        public IHttpActionResult Patch(String id, [FromBody]SCIMGroupOperation operation)
        {
            bool result = false;
            bool response = false;
        

            if (id == null)
            {
                logger.Error("Error at PATCH Group, id missing ");
                return BadRequest();
            }
            else
            {
                logger.Debug("Enter Patch  Id " + id);
            }
            try
            {
                foreach (var nextOp in operation.Operations)
                {

                    switch (nextOp.op)
                    {
                        case "add":
                            Member addMember = new Member();
                            response = false;
                            addMember.value = nextOp.value[0].value;
                            addMember.display = nextOp.value[0].display;

                            response =  connector.addGroupMember(id,addMember);
                            if (response)
                            {
                                result = true;
                            }
                            
                            break;
                        case "remove":
                            Member removeMember = new Member();
                            response = false;

                            string path = nextOp.path;
                            var index = path.IndexOf("eq");
                            var path1 = path.Substring(index + 2);
                            var text = Regex.Replace(path1, "[^\\w\\._]", "");
                            removeMember.value = text;
                            response = connector.removeGroupMember(id,removeMember);
                            if (response)
                            {
                                result = true;
                            }

                            break;
                        case "replace":
                            Member replaceMember = new Member();
                            response = false;
                            response = connector.removeGroupMember(id, replaceMember);
                            if (response)
                            {
                                response = connector.addGroupMember(id, replaceMember);
                                if (response)
                                {
                                    result = true;
                                }
                            }
                  
                            break;
                        default:
                            logger.Debug("Exit Patch group failed with unknown operation id" + id);
                            SCIMException patchException = new SCIMException();
                            patchException.ErrorMessage = "Patch group failed with unknown operation id " + id;
                            patchException.ErrorSummary = "Patch group failed with unknown operation id" + id;
                            return InternalServerError(patchException);
                    }

                }//end foreach
                if (result)
                {
                    logger.Debug("Exit Successfully Patch group id " + id);
                    //both a 204 and 200 with full object are legal
                    return Ok(connector.getGroup(id));
                    //return StatusCode(HttpStatusCode.NoContent);
                }
                else
                {
                    logger.Debug("Exit Patch group failed id " + id);
                    SCIMException patchException = new SCIMException();
                    patchException.ErrorMessage = "Patch group failed  id " + id;
                    patchException.ErrorSummary = "Patch group failed id " + id;
                    return InternalServerError(patchException);
                }

            }//end try
            catch (Exception e)
            {
                logger.Debug("Exit Error at Patching Group  ");
                logger.Error(e);
                return InternalServerError(e);
            }
        }




        [HttpDelete]
        public IHttpActionResult Delete(String id)
        {
            logger.Debug("Delete id " + id);
            try
            {
                SCIMGroup group = connector.getGroup(id);
                if (group == null)
                {
                    return NotFound();
                }   

                connector.deleteGroup(id);
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }  
        }

    }
}

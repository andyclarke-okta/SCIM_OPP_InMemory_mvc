using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okta.SCIM.Models;
using OktaSCIMConn.Exceptions;

namespace OktaSCIMConn.Connectors
{
    // implement this interface to build an Okta connector
    public interface ISCIMConnector
    {
        SCIMUser createUser(SCIMUser user);
        SCIMUser getUser(String id);
        SCIMUserQueryResponse getUsers(PaginationProperties pageProperties, SCIMFilter filter);
        SCIMUser deactivateUser(String id);
        SCIMUser reactivateUser(String id);
        SCIMUser updateUser(SCIMUser user);

        //Groups
        SCIMGroup createGroup(SCIMGroup group);
        SCIMGroup getGroup(String id);
        SCIMGroupQueryResponse getGroups(PaginationProperties pageProperties);
        bool deleteGroup(String id);
        SCIMGroup updateGroup(SCIMGroup group);

        //Group Membeship
        bool addGroupMember(string id, Member member);
        bool removeGroupMember(string id, Member member);
        ServiceProviderConfiguration getServiceProviderConfig();
    }
}

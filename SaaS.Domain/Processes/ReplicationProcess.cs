#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace SaaS.Processes
{
    /// <summary>
    /// Replicates changes between <see cref="ISecurityAggregate"/> and 
    /// individual instances of <see cref="IUserAggregate"/>
    /// </summary>
    public sealed class SecurityUserAggregateReplication
    {
        // 'Domain' is the name of the primary Bounded Context
        // in this system
        readonly DomainSender _send;

        public SecurityUserAggregateReplication(DomainSender send)
        {
            _send = send;
        }

        public void When(SecurityPasswordAdded e)
        {
            _send.ToUser(new CreateUser(e.UserId, e.Id));
        }

        public void When(SecurityIdentityAdded e)
        {
            _send.ToUser(new CreateUser(e.UserId, e.Id));
        }

        public void When(SecurityItemRemoved e)
        {
            _send.ToUser(new DeleteUser(e.UserId));
        }
    }

    /// <summary>
    /// Replicates changes between <see cref="IUserAggregate"/> and 
    /// individual instances of <see cref="ISecurityAggregate"/>
    /// </summary>
    public sealed class UserAggregateSecurityReplication
    {
        // 'Domain' is the name of the primary Bounded Context
        // in this system
        readonly DomainSender _send;

        public UserAggregateSecurityReplication(DomainSender send)
        {
            _send = send;
        }

        public void When(UserDeleted e)
        {
            _send.ToSecurity(new RemoveSecurityItem(e.SecurityId, e.Id));
        }
    }
}
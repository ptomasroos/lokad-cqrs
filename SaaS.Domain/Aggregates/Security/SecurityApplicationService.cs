#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.Aggregates.Security
{
    public sealed class SecurityApplicationService : ISecurityApplicationService, IApplicationService
    {
        readonly IEventStore _eventStore;
        readonly IDomainIdentityService _ids;
        readonly PasswordGenerator _pwds;

        public SecurityApplicationService(
            IEventStore eventStore, 
            IDomainIdentityService ids, 
            PasswordGenerator pwds)
        {
            _eventStore = eventStore;
            _ids = ids;
            _pwds = pwds;
        }

        public void Execute(object o)
        {
            RedirectToWhen.InvokeCommand(this, o);
        }

        void Update(ICommand<SecurityId> c, Action<SecurityAggregate> action)
        {
            var eventStream = _eventStore.LoadEventStream(c.Id);
            var state = new SecurityState(eventStream.Events);
            var agg = new SecurityAggregate(state);
            action(agg);
            _eventStore.AppendEventsToStream(c.Id, eventStream.StreamVersion, agg.Changes);
        }

        public void When(CreateSecurityAggregate c)
        {
            Update(c, ar => ar.CreateSecurityAggregate(c.Id));
        }

        public void When(CreateSecurityFromRegistration c)
        {
            Update(c, ar => ar.When(_ids, _pwds, c));
        }

        public void When(AddSecurityPassword c)
        {
            Update(c, a => a.AddPassword(_ids, _pwds, c.DisplayName, c.Login, c.Password));
        }

        public void When(AddSecurityIdentity c)
        {
            Update(c, a => a.AddIdentity(_ids, _pwds, c.DisplayName, c.Identity));
        }


        public void When(RemoveSecurityItem c)
        {
            Update(c, ar => ar.RemoveSecurityItem(c.UserId));
        }

        public void When(UpdateSecurityItemDisplayName c)
        {
            Update(c, ar => ar.UpdateDisplayName(c.UserId, c.DisplayName));
        }

        public void When(AddPermissionToSecurityItem c)
        {
            Update(c, ar => ar.AddPermissionToSecurityItem(c.UserId, c.Permission));
        }
    }
}
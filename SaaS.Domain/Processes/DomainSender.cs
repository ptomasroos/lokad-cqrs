namespace SaaS.Processes
{
    public sealed class DomainSender
    {
        readonly ICommandSender _service;

        public DomainSender(ICommandSender service)
        {
            _service = service;
        }

        public void ToRegistration(ICommand<RegistrationId> cmd)
        {
            _service.SendCommand(cmd);
        }

        public void ToUser(ICommand<UserId> cmd)
        {
            _service.SendCommand(cmd);
        }

        public void ToSecurity(ICommand<SecurityId> cmd)
        {
            _service.SendCommand(cmd);
        }

        public void ToService(ICommand cmd)
        {
            _service.SendCommand(cmd);
        }
    }
}
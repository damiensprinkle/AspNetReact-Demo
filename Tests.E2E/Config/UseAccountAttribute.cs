namespace Tests.E2E.Config
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UseAccountAttribute : Attribute
    {
        public AutomationAccount Account { get; }

        public UseAccountAttribute(AutomationAccount account) => Account = account;
    }
}

namespace Usher.Platforms.Generic
{
    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class ManagerAttribute : System.Attribute
    {
        public ManagerAttribute (string id)
        {
            Id = id;
        }
        
        public string Id { get; }
    }
}
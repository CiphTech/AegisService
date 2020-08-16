using System;

namespace Aegis.Model
{
    public class AegisPersonInfo
    {
        public Guid Id { get; private set; }
        
        public string Name { get; private set; }

        public AegisPersonInfo(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
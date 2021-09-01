using System;

namespace Trellis.Communications
{
    public record Envelope
    {
        public Envelope(ulong Id, string By, DateTime CreatedOn, CommunicationTypes Type, object Data, string Tag)
        {
            this.Id = Id;
            this.By = By;
            this.CreatedOn = CreatedOn;
            this.Type = Type;
            this.Data = Data;
            this.Tag = Tag;
        }
        
        public ulong Id { get; init; }
        public string By { get; init; }
        public DateTime CreatedOn { get; init; }
        public CommunicationTypes Type { get; init; }
        public object Data { get; init; }
        public string Tag { get; init; }
        
        
        public void Deconstruct(out ulong Id, out string By, out DateTime CreatedOn, out CommunicationTypes Type, out object Data, out string Tag)
        {
            Id = this.Id;
            By = this.By;
            CreatedOn = this.CreatedOn;
            Type = this.Type;
            Data = this.Data;
            Tag = this.Tag;
        }
    }

}
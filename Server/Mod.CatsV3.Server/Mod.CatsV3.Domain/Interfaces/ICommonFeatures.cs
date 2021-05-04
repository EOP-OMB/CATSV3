using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.CatsV3.Domain.Interfaces
{
    public interface ICommonFeatures
    {
        public string Icon {get;}
        public string DocumentsCount { get; }
        public string ReferenceDocuments { get; }
        public string ReviewDocuments { get; }
        public string FinalDocuments { get; }
        public string Originators { get; }
        public string Reviewers { get; }
        public string FYIUsers { get; }
        public string CompletedUsers { get; }
        public string DraftUsers { get; }
        public string CompletedRounds { get; }
    }
}

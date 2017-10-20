using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bp.ot.s.API.Models.TransEu
{
    public class TransEuTokenDTO
    {
        public TransEuTokenDTO()
        {
            this.Errors = new List<string>();
            this._issuedAt = DateTime.Now;
        }
        public string Access_token { get; set; }
        public int Expires_in { get; set; }
        private DateTime _issuedAt { get; set; }
        public string Token_type { get; set; }
        public string Scope { get; set; }
        public string Refresh_token { get; set; }
        public List<string> Errors { get; set; }
        public DateTime ValidUntil { get => this._issuedAt.AddSeconds((this.Expires_in - 60)); }
    }
}

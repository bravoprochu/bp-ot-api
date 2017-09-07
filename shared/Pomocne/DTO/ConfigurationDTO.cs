using System;
using System.Collections.Generic;
using System.Text;

namespace bp.Pomocne.DTO
{
    public class ConfigurationDTO
    {
        public ConfigurationConnectionStringsDTO ConnectionStrings { get; set; }
        public ConfigurationTokenDTO Tokens { get; set; }

    }


    public class ConfigurationConnectionStringsDTO
    {
        public string Ident { get; set; }
        public string Dane { get; set; }
    }

    public class ConfigurationTokenDTO
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
    }






}

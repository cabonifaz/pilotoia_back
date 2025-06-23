using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PilotoIA_Backend.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpireMinutes { get; set; }
    }

    public class beMySettings
    {
        public JwtSettings Jwt { get; set; }
        public string DbConnection { get; set; }
        public string Secret { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string EmailSMTPPort { get; set; }
        public string EmailSMTPMail { get; set; }
        public string EmailSenderMail { get; set; }
        public string EmailSecurityProtocol { get; set; }
        public string EmailRequiereSSL { get; set; }
        public string EmailTipoCredencial { get; set; }
        public string EmailReplyTo { get; set; }
        public string EmailNotifyTo { get; set; }
        public string EmailAttach { get; set; }
        public string EmailNombreAttach { get; set; }
        public string EmailAttach2 { get; set; }
        public string EmailNombreAttach2 { get; set; }
        public string URlSistema { get; set; }
        public string URlFrontEnd { get; set; }
        public string WebApiBaseUrl { get; set; }
        public string DiasPorVencer { get; set; }
        public string AlertaDiasPorVencer { get; set; }
        public string AlertaDiasATraer { get; set; }
        public string UrlBaseApiSUC { get; set; }
        public string EndPointValidarSUC { get; set; }
    }
}

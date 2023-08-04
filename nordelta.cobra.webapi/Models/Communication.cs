using nordelta.cobra.webapi.Controllers.Contracts;
using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    [Table("Communications")]
    public class Communication : IFilterableByDepartment, IFilterableByBU
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CommunicationCreatorUserId { get; set; }
        public int? AccountBalanceId { get; set; }
        [ForeignKey("AccountBalanceId")]
        public AccountBalance AccountBalance { get; set; }
        public int? ContactDetailId { get; set; }
        [ForeignKey("ContactDetailId")]
        public ContactDetail ContactDetail { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public DateTime? NextCommunicationDate { get; set; }
        [Required]
        public bool Incoming { get; set; }
        [Required]
        [ForeignKey("ClientId")]
        public User Client { get; set; }
        [Required]
        public EComChannelType CommunicationChannel { get; set; }
        public ECommunicationResult CommunicationResult { get; set; }
        [Required]
        public string Description { get; set; }
        [NotMapped]
        public SsoUser SsoUser { get; set; }

        public string GetBU()
        {
            return AccountBalance.BusinessUnit;
        }

        public AccountBalance.EDepartment GetDepartment()
        {
            return AccountBalance.Department;
        }
        public string GetPublishDebt()
        {
            return AccountBalance.PublishDebt;
        }
    }


    public enum ECommunicationResult
    {
        SinRespuesta = 0,
        DatosErroneosDeContacto = 1,
        BuzonDeVoz = 2,
        DerivadoAResponsable = 3,
        ContactoSinCompromiso = 4,
        CompromisoDePagoParcial = 5,
        CompromisoDePago = 6,
        PagoParcialRealizado = 7,
        PagoRealizado = 8,
        SinIntencionDePago = 9,
        Revisar = 10
    }

    public enum EComChannelType
    {
        Reunion,
        Mediacion,
        Whatsapp,
        Telefono,
        CorreoElectronico,
        Videoconferencia,
        CartaDocumento,
        Otro
    }

}
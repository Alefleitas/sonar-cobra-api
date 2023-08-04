using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;
using ForeignKeyAttribute = SQLiteNetExtensions.Attributes.ForeignKeyAttribute;

namespace nordelta.cobra.webapi.Models
{
    [Table("SsoUser")]
    public class SsoUser
    {
        public SsoUser()
        {
            this.Roles = new List<SsoUserRole>();
            this.UserDataCuits = new List<SsoUserCuit>();
            this.Empresas = new List<SsoUserEmpresa>();
        }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public string ClientReference { get; set; }
        public bool IsForeignCuit { get; set; }
        public string IdApplicationUser { get; set; }
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string TipoUsuario { get; set; }
        public string Email { get; set; }
        [OneToMany]
        public List<SsoUserCuit> UserDataCuits { get; set; }
        [OneToMany]
        public List<SsoUserRole> Roles { get; set; }
        [OneToMany]
        public List<SsoUserEmpresa> Empresas { get; set; }
    }

    public class SsoUserEmpresa
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(SsoUser))]
        public string UserId { get; set; }
        public string Empresa { get; set; }

    }

    public class SsoUserCuit
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(SsoUser))]
        public string UserId { get; set; }
        public string Cuit { get; set; }
        public string RazonSocial { get; set; }
    }

    public class SsoUserRole
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(SsoUser))]
        public string UserId { get; set; }
        public string Role { get; set; }
    }
}



namespace ServiceMonitoramento.DTO.ApiAntiga
{
    public class DTODeProjecaoRegistrosUploadMobileApiAntiga
    {
        public int Id_usuario { get; set; }
        public int Id_empresa { get; set; }
        public int Id_turno { get; set; }
        public string Uuid_device { get; set; }
        public DateTime Data_base { get; set; }
        public DateTime Data_hora_alteracao { get; set; }
        public string Versao_app { get; set; }

        public int Id_pac { get; set; }

        public int Id_item { get; set; }
        public string Id_num_docto { get; set; }
        public string Num_docto { get; set; }
        public int Tipo_do_dado_item { get; set; }
        public int Tipo_do_item { get; set; }

        public int Num_registro { get; set; } //Sequencia
        public string Code_controller { get; set; }
        public int? Id_api { get; set; }
        public int Status_conformidade { get; set; }
        public int? Status { get; set; }
        public DateTime? Data_hora { get; set; }
        public int? Tipo { get; set; }
        public string Valor { get; set; }
        public int Tipo_interface { get; set; }
        public int Tipo_do_dado { get; set; }
        public string Observacoes { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int Digitado { get; set; } = 1;
        public DateTime? Data_hora_envio { get; set; }
        public string Code_controller_registro_editado { get; set; }
        public List<DTODeRegistrosNCMobileApiAntiga> Ncs { get; set; }
    }
    
}

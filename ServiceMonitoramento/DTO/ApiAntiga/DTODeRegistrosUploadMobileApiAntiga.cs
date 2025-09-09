

namespace ServiceMonitoramento.DTO.ApiAntiga
{
    public class DTODeRegistrosNCMobileApiAntiga
    {
        public int Id_usuario { get; set; }
        public int Id_empresa { get; set; }
        public int Id_turno { get; set; }
        public string Uuid_device { get; set; }
        public DateTime Data_base { get; set; }
        public string Versao_app { get; set; }
        public List<DTODeRegistrosPACMobile> Pacs { get; set; }
    }

    public class DTODeRegistrosPACMobile
    {
        public int Id_pac { get; set; }
        public int? Status { get; set; }
        public List<DTODeRegistrosItensMobile> Itens { get; set; }
    }

    public class DTODeRegistrosItensMobile
    {
        public int Id_item { get; set; }
        public string Id_num_docto { get; set; }
        public string Num_docto { get; set; }
        public List<DTODeRegistrosDadosMobile> Registros { get; set; }
    }

    public class DTODeRegistrosDadosMobile
    {
        public int Num_registro { get; set; } //Sequencia
        public string Code_controller { get; set; }
        public int? Id_item { get; set; }
        public int? Id_api { get; set; }
        public int? Id_usuario { get; set; }
        public int? Id_turno { get; set; }
        public int Status_conformidade { get; set; }
        public DateTime? Data_hora { get; set; }
        public int? Tipo { get; set; }
        public int Tipo_interface { get; set; }
        public int Tipo_do_dado { get; set; }
        public string Valor { get; set; }
        public int Digitado { get; set; } = 1;
        public string Observacoes { get; set; }
        public int? Status { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int Id_cab { get; set; }
        public List<DTODeRegistrosNCMobile> Ncs { get; set; }

    }

    public class DTODeRegistrosNCMobile
    {
        public int Id_nc { get; set; }
        public string Observacoes { get; set; }
        public List<DTODeImgMAPPosicoes> Posicoes { get; set; }
        public List<DTODeRegistrosNCResponsavelMobile> Responsaveis { get; set; }
        public List<DTODeRegistrosAcoesMobile> Acoes { get; set; }
    }

    public class DTODeRegistrosAcoesMobile
    {
        public int Id_acao { get; set; }
        public string Descricao_resumida { get; set; }
        public int Id_resultado { get; set; }
        public int? IdQuemMonitora { get; set; }
        public int? IdQuemVerifica { get; set; }
        public DateTime? Data_hora { get; set; }
        public DateTime? Data_hora_monitoramento { get; set; }
        public DateTime? Data_hora_verificacao { get; set; }
        public string Observacoes { get; set; }
    }

    public class DTODeRegistrosNCResponsavelMobile
    {
        public int Id { get; set; }
        public string Nome { get; set; }

    }

    //Registros com imagem map (mapa de imagem)
    public class DTODeImgMAPPosicoes
    {
        public int tr { get; set; }
        public int td { get; set; }
        public string color_hexa { get; set; }
    }


    //Registro com lote produto
    public class DTODeRegistrosDadosLoteMobile
    {
        public int Id { get; set; }
        public string codigo_lote { get; set; }
    }


    //Regsittro com itens limiados
    public class DTODeRegistroItensComLimitadorMobile
    {
        //public int Id { get; set; }
        public bool Conforme { get; set; }
        public int? Id_valor_permitido { get; set; }
        //public int? nc { get; set; }
        public string Valor_permitido { get; set; }
    }

    public class ProjecaoDeRegistroItensComLimitadorMobile
    {
        public bool Conforme { get; set; }
        public int? Id_valor_permitido { get; set; }
    }

}

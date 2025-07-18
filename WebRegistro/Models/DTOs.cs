namespace WebRegistro.Models // Ou o namespace que preferir, como WebRegistro.ViewModels
{
    /// <summary>
    /// DTO para a requisição de VERIFICAÇÃO de biometria (usado no Ponto e no Login).
    /// </summary>
    public class BiometriaVerificacaoRequest
    {
        public string BiometricData { get; set; }
    }

    /// <summary>
    /// DTO para a requisição de CADASTRO de biometria.
    /// </summary>
    public class BiometriaCadastroRequest
    {
        public string Cpf { get; set; }
        public string BiometricData { get; set; }
    }
}

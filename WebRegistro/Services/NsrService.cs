// Services/NsrService.cs
using WebRegistro.Data;
using WebRegistro.Models;

public class NsrService
{
    private readonly ApplicationDbContext _context;
    // Objeto usado para "trancar" a operação e evitar condição de corrida
    private static readonly object _lock = new object();

    public NsrService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<long> GerarProximoNsrAsync()
    {
        // O lock garante que apenas uma thread por vez possa executar este bloco de código
        // na mesma instância da aplicação, prevenindo condições de corrida a nível de aplicação.
        // A transação do banco de dados (abaixo) previne a nível de banco.
        lock (_lock)
        {
            // Usar uma transação é CRUCIAL para garantir a atomicidade no banco de dados.
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Busca o contador na tabela.
                    // O .FirstOrDefault() é só para o caso de a tabela estar vazia.
                    var contador = _context.Contadores.FirstOrDefault(c => c.NomeContador == "NSR_GERAL");

                    if (contador == null)
                    {
                        // Se o contador não existir, cria-o com valor inicial.
                        contador = new Contador { NomeContador = "NSR_GERAL", UltimoValor = 0 };
                        _context.Contadores.Add(contador);
                    }

                    // 2. Incrementa o valor.
                    contador.UltimoValor++;

                    // 3. Salva a alteração no banco de dados.
                    _context.SaveChanges();

                    // 4. Confirma a transação. Todas as operações foram um sucesso.
                    transaction.Commit();

                    // 5. Retorna o novo número gerado.
                    return contador.UltimoValor;
                }
                catch (Exception)
                {
                    // Se algo der errado, desfaz tudo.
                    transaction.Rollback();
                    throw; // Propaga o erro
                }
            }
        }
    }
}
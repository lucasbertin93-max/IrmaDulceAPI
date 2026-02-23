using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Enums;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class PessoaService : IPessoaService
{
    private readonly IPessoaRepository _pessoaRepo;
    private readonly IUsuarioRepository _usuarioRepo;

    public PessoaService(IPessoaRepository pessoaRepo, IUsuarioRepository usuarioRepo)
    {
        _pessoaRepo = pessoaRepo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<PessoaResponse> CriarAsync(PessoaCreateRequest request)
    {
        // Validação de CPF duplicado
        var existente = await _pessoaRepo.GetByCpfAsync(request.CPF);
        if (existente != null)
            throw new InvalidOperationException($"Já existe uma pessoa cadastrada com o CPF {request.CPF}.");

        // Gera ID funcional
        var (prefix, padding) = request.Perfil switch
        {
            PerfilUsuario.Aluno => ("A", 6),
            PerfilUsuario.Docente => ("D", 4),
            PerfilUsuario.Administrativo or PerfilUsuario.Master => ("AD", 4),
            _ => throw new ArgumentException("Perfil inválido.")
        };

        var nextId = await _pessoaRepo.GetNextSequentialIdAsync(prefix);
        var idFuncional = $"{prefix}{nextId.ToString().PadLeft(padding, '0')}";

        // Se tem dados de responsável financeiro embutidos, cria primeiro
        int? responsavelId = request.ResponsavelFinanceiroId;
        if (responsavelId == null && request.ResponsavelFinanceiro != null)
        {
            var resp = request.ResponsavelFinanceiro;
            var responsavel = new Pessoa
            {
                IdFuncional = $"RF{nextId.ToString().PadLeft(4, '0')}", // RF = Responsável Financeiro
                Perfil = PerfilUsuario.Aluno, // Responsável não tem acesso ao sistema
                NomeCompleto = resp.NomeCompleto,
                RG = resp.RG,
                CPF = resp.CPF,
                EstadoCivil = resp.EstadoCivil,
                DataNascimento = resp.DataNascimento,
                Naturalidade = resp.Naturalidade,
                Nacionalidade = resp.Nacionalidade,
                Logradouro = resp.Logradouro,
                Numero = resp.Numero,
                CEP = resp.CEP,
                Bairro = resp.Bairro,
                Cidade = resp.Cidade,
                PontoReferencia = resp.PontoReferencia,
                NomePai = resp.NomePai,
                NomeMae = resp.NomeMae,
            };
            await _pessoaRepo.AddAsync(responsavel);
            responsavelId = responsavel.Id;
        }

        var pessoa = new Pessoa
        {
            IdFuncional = idFuncional,
            Perfil = request.Perfil,
            NomeCompleto = request.NomeCompleto,
            RG = request.RG,
            CPF = request.CPF,
            EstadoCivil = request.EstadoCivil,
            DataNascimento = request.DataNascimento,
            Naturalidade = request.Naturalidade,
            Nacionalidade = request.Nacionalidade,
            Logradouro = request.Logradouro,
            Numero = request.Numero,
            CEP = request.CEP,
            Bairro = request.Bairro,
            Cidade = request.Cidade,
            PontoReferencia = request.PontoReferencia,
            NomePai = request.NomePai,
            NomeMae = request.NomeMae,
            ResponsavelFinanceiroId = responsavelId,
        };

        await _pessoaRepo.AddAsync(pessoa);

        // Auto-cria login para perfis com acesso ao sistema
        if (request.Perfil != PerfilUsuario.Aluno || true) // Todos podem ter login
        {
            var login = idFuncional.ToLower();
            var senhaInicial = BCrypt.Net.BCrypt.HashPassword("123456"); // Senha padrão

            var usuario = new Usuario
            {
                PessoaId = pessoa.Id,
                Login = login,
                SenhaHash = senhaInicial,
                Perfil = request.Perfil,
            };
            await _usuarioRepo.AddAsync(usuario);
        }

        return MapToResponse(pessoa);
    }

    public async Task<PessoaResponse?> GetByIdAsync(int id)
    {
        var pessoa = await _pessoaRepo.GetByIdAsync(id);
        return pessoa == null ? null : MapToResponse(pessoa);
    }

    public async Task<PessoaResponse?> GetByIdFuncionalAsync(string idFuncional)
    {
        var pessoa = await _pessoaRepo.GetByIdFuncionalAsync(idFuncional);
        return pessoa == null ? null : MapToResponse(pessoa);
    }

    public async Task<IEnumerable<PessoaResponse>> GetAllAsync(PerfilUsuario? perfil = null)
    {
        IEnumerable<Pessoa> pessoas;
        if (perfil.HasValue)
            pessoas = await _pessoaRepo.FindAsync(p => p.Perfil == perfil.Value && p.Ativo);
        else
            pessoas = await _pessoaRepo.FindAsync(p => p.Ativo);

        return pessoas.Select(MapToResponse);
    }

    public async Task<PessoaResponse> AtualizarAsync(int id, PessoaCreateRequest request)
    {
        var pessoa = await _pessoaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Pessoa com ID {id} não encontrada.");

        // Verifica se CPF mudou e se já existe outro com esse CPF
        if (pessoa.CPF != request.CPF)
        {
            var existente = await _pessoaRepo.GetByCpfAsync(request.CPF);
            if (existente != null)
                throw new InvalidOperationException($"Já existe uma pessoa cadastrada com o CPF {request.CPF}.");
        }

        // Atualiza campos (IdFuncional e Perfil são imutáveis)
        pessoa.NomeCompleto = request.NomeCompleto;
        pessoa.RG = request.RG;
        pessoa.CPF = request.CPF;
        pessoa.EstadoCivil = request.EstadoCivil;
        pessoa.DataNascimento = request.DataNascimento;
        pessoa.Naturalidade = request.Naturalidade;
        pessoa.Nacionalidade = request.Nacionalidade;
        pessoa.Logradouro = request.Logradouro;
        pessoa.Numero = request.Numero;
        pessoa.CEP = request.CEP;
        pessoa.Bairro = request.Bairro;
        pessoa.Cidade = request.Cidade;
        pessoa.PontoReferencia = request.PontoReferencia;
        pessoa.NomePai = request.NomePai;
        pessoa.NomeMae = request.NomeMae;
        pessoa.ResponsavelFinanceiroId = request.ResponsavelFinanceiroId;

        await _pessoaRepo.UpdateAsync(pessoa);
        return MapToResponse(pessoa);
    }

    public async Task DesativarAsync(int id)
    {
        var pessoa = await _pessoaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Pessoa com ID {id} não encontrada.");

        pessoa.Ativo = false;
        await _pessoaRepo.UpdateAsync(pessoa);

        // Desativa o login também
        var usuario = await _usuarioRepo.GetByPessoaIdAsync(id);
        if (usuario != null)
        {
            usuario.Ativo = false;
            await _usuarioRepo.UpdateAsync(usuario);
        }
    }

    private static PessoaResponse MapToResponse(Pessoa p) => new(
        Id: p.Id,
        IdFuncional: p.IdFuncional,
        NomeCompleto: p.NomeCompleto,
        CPF: p.CPF,
        RG: p.RG,
        EstadoCivil: p.EstadoCivil,
        DataNascimento: p.DataNascimento,
        Naturalidade: p.Naturalidade,
        Nacionalidade: p.Nacionalidade,
        Logradouro: p.Logradouro,
        Numero: p.Numero,
        CEP: p.CEP,
        Bairro: p.Bairro,
        Cidade: p.Cidade,
        PontoReferencia: p.PontoReferencia,
        NomePai: p.NomePai,
        NomeMae: p.NomeMae,
        Perfil: p.Perfil,
        Ativo: p.Ativo,
        ResponsavelFinanceiroId: p.ResponsavelFinanceiroId
    );
}
